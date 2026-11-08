using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class WanderingMover : MonoBehaviour
    {
        private const int TargetAttempts = 12;

        private WanderArea daytimeArea;
        private WanderArea nighttimeArea;
        private VillageWaypointNetwork waypointNetwork;
        private System.Random random;
        private WanderMotor motor;
        private Vector2 target;
        private Vector2 lastProgressPosition;
        private float speed;
        private float minIdleSeconds;
        private float maxIdleSeconds;
        private float waitUntil;
        private float pausedUntil;
        private float lastProgressAt;
        private int currentNode = -1;
        private int previousNode = -1;
        private int targetNode = -1;
        private int homeNode = -1;
        private bool hasNightAnchor;
        private bool hasTarget;

        public WanderArea RoamingArea => daytimeArea;
        public bool IsMoving => hasTarget && Time.time >= waitUntil && Time.time >= pausedUntil;
        public bool UsesWaypointNetwork => waypointNetwork != null;

        public void Configure(
            WanderArea area,
            float moveSpeed,
            int seed,
            int group = 0,
            float minIdle = 1.2f,
            float maxIdle = 3.5f)
        {
            daytimeArea = area;
            speed = Mathf.Max(0.1f, moveSpeed);
            minIdleSeconds = Mathf.Max(0f, minIdle);
            maxIdleSeconds = Mathf.Max(minIdleSeconds, maxIdle);
            random = new System.Random(seed);
            waitUntil = Time.time + NextIdleDuration();
            hasTarget = false;
            motor = GetComponent<WanderMotor>() ?? gameObject.AddComponent<WanderMotor>();
            motor.Configure(group);
        }

        public void ConfigureRoute(VillageWaypointNetwork network, int returnHomeNode)
        {
            waypointNetwork = network;
            homeNode = Mathf.Clamp(returnHomeNode, 0, network.Count - 1);
            currentNode = network.FindClosestNode(transform.position);
            previousNode = -1;
            hasTarget = false;
        }

        public void ConfigureNightAnchor(Vector2 anchor)
        {
            nighttimeArea = new WanderArea(anchor, new Vector2(0.35f, 0.25f));
            hasNightAnchor = true;
        }

        public void Pause(float seconds)
        {
            pausedUntil = Mathf.Max(pausedUntil, Time.time + Mathf.Max(0f, seconds));
            AbandonTarget(seconds);
        }

        private void FixedUpdate()
        {
            if (random == null || Time.time < pausedUntil)
            {
                return;
            }

            if (!hasTarget && Time.time >= waitUntil)
            {
                ChooseTarget();
            }

            if (hasTarget && Time.time >= waitUntil)
            {
                MoveTowardsTarget();
            }
        }

        private void ChooseTarget()
        {
            if (waypointNetwork != null)
            {
                ChooseRouteTarget();
                return;
            }

            var area = GetActiveArea();
            for (var attempt = 0; attempt < TargetAttempts; attempt++)
            {
                var candidate = area.GetPoint((float)random.NextDouble(), (float)random.NextDouble());
                if (motor.HasSolidObstacle(motor.Position, candidate))
                {
                    continue;
                }

                StartTarget(candidate, -1);
                return;
            }

            waitUntil = Time.time + 0.5f;
        }

        private void ChooseRouteTarget()
        {
            if (IsNightTime())
            {
                if (currentNode != homeNode)
                {
                    StartTarget(waypointNetwork.GetPoint(
                        waypointNetwork.GetNextNodeToward(currentNode, homeNode)), 
                        waypointNetwork.GetNextNodeToward(currentNode, homeNode));
                    return;
                }

                StartTarget(nighttimeArea.Center, -1);
                return;
            }

            var nextNode = waypointNetwork.GetRandomNeighbor(currentNode, previousNode, random);
            StartTarget(waypointNetwork.GetPoint(nextNode), nextNode);
        }

        private void StartTarget(Vector2 destination, int destinationNode)
        {
            target = destination;
            targetNode = destinationNode;
            hasTarget = true;
            lastProgressPosition = motor.Position;
            lastProgressAt = Time.time;
        }

        private void MoveTowardsTarget()
        {
            var current = motor.Position;
            TrackProgress(current);
            if (Time.time - lastProgressAt > 2f)
            {
                AbandonTarget(0.35f);
                return;
            }

            var next = Vector2.MoveTowards(current, target, speed * Time.fixedDeltaTime);
            next += motor.CalculateSeparationPush() * Time.fixedDeltaTime;
            next = GetActiveArea().ConstrainStep(current, next);
            if (motor.HasSolidObstacle(current, next))
            {
                AbandonTarget(0.35f);
                return;
            }

            motor.MovePosition(next);
            if (Vector2.SqrMagnitude(next - target) > 0.0025f)
            {
                return;
            }

            if (targetNode >= 0)
            {
                previousNode = currentNode;
                currentNode = targetNode;
            }

            hasTarget = false;
            targetNode = -1;
            waitUntil = Time.time + NextIdleDuration();
        }

        private void TrackProgress(Vector2 current)
        {
            if (Vector2.SqrMagnitude(current - lastProgressPosition) < 0.0025f)
            {
                return;
            }

            lastProgressPosition = current;
            lastProgressAt = Time.time;
        }

        private void AbandonTarget(float delay)
        {
            hasTarget = false;
            targetNode = -1;
            waitUntil = Time.time + Mathf.Max(0f, delay);
        }

        private WanderArea GetActiveArea()
        {
            return IsNightTime() ? nighttimeArea : daytimeArea;
        }

        private bool IsNightTime()
        {
            if (!hasNightAnchor || GameSession.Instance == null)
            {
                return false;
            }

            var hour = GameSession.Instance.Data.hour;
            return hour >= 20 || hour < 6;
        }

        private float NextIdleDuration()
        {
            return Mathf.Lerp(minIdleSeconds, maxIdleSeconds, (float)random.NextDouble());
        }

    }
}
