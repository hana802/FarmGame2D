using UnityEngine;

namespace FarmGame2D.World
{
    internal sealed class WanderMotor : MonoBehaviour
    {
        private readonly RaycastHit2D[] pathHits = new RaycastHit2D[16];
        private readonly Collider2D[] nearbyColliders = new Collider2D[12];

        private Rigidbody2D body;
        private Collider2D actorCollider;
        private int separationGroup;

        public Vector2 Position => body.position;

        public void Configure(int group)
        {
            separationGroup = group;
            actorCollider = GetComponent<Collider2D>();
            if (actorCollider == null)
            {
                actorCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            actorCollider.isTrigger = false;
            body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void MovePosition(Vector2 next)
        {
            body.MovePosition(next);
        }

        public bool HasSolidObstacle(Vector2 from, Vector2 to)
        {
            var direction = to - from;
            var distance = direction.magnitude;
            if (distance <= 0.001f)
            {
                return false;
            }

            var hitCount = actorCollider.Cast(direction.normalized, pathHits, distance + 0.02f);
            for (var index = 0; index < hitCount; index++)
            {
                var hit = pathHits[index].collider;
                if (hit != null && !hit.isTrigger && hit != actorCollider)
                {
                    return true;
                }
            }

            return false;
        }

        public Vector2 CalculateSeparationPush()
        {
            if (separationGroup == 0)
            {
                return Vector2.zero;
            }

            var count = Physics2D.OverlapCircleNonAlloc(body.position, 0.65f, nearbyColliders);
            var push = Vector2.zero;
            for (var index = 0; index < count; index++)
            {
                var otherMover = nearbyColliders[index].GetComponent<WanderingMover>();
                var otherMotor = otherMover == null ? null : otherMover.GetComponent<WanderMotor>();
                if (otherMover == null || otherMotor == null || otherMotor == this
                    || otherMotor.separationGroup != separationGroup)
                {
                    continue;
                }

                var away = body.position - otherMotor.Position;
                if (away.sqrMagnitude > 0.0001f)
                {
                    push += away.normalized * 0.35f;
                }
            }

            return Vector2.ClampMagnitude(push, 0.5f);
        }
    }
}
