using FarmGame2D.Art;
using UnityEngine;

namespace FarmGame2D.World
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class DogCompanionController : MonoBehaviour
    {
        private const float FollowSpeed = 3.45f;
        private const float CelebrationDuration = 2.4f;
        private const float CelebrationOrbitRadius = 1.12f;
        private const float CelebrationTurns = 2f;
        private readonly RaycastHit2D[] pathHits = new RaycastHit2D[8];

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private Transform player;
        private Transform visual;
        private SpriteRenderer dogRenderer;
        private Vector2 lastPlayerPosition;
        private Vector2 lastPlayerDirection = Vector2.up;
        private float celebrationElapsed;
        private float celebrationStartAngle;
        private Vector2 lastSafePosition;
        private bool hasLastSafePosition;
        private readonly Collider2D[] overlapResults = new Collider2D[8];
        private bool initialPlacementPending;

        public bool IsCelebrating => celebrationElapsed > 0f;
        public Transform FollowTarget => player;
        public SpriteRenderer DogRenderer => dogRenderer;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            bodyCollider = GetComponent<BoxCollider2D>();
            bodyCollider.isTrigger = true;
            bodyCollider.size = new Vector2(0.72f, 0.48f);

            visual = new GameObject("Farm Dog Visual").transform;
            visual.SetParent(transform, false);
            dogRenderer = visual.gameObject.AddComponent<SpriteRenderer>();
            dogRenderer.sprite = WorldArtCatalog.GetSprite(WorldArtKind.Dog);
        }

        public void Configure(Transform playerTransform)
        {
            player = playerTransform;
            lastPlayerPosition = player == null ? Vector2.zero : player.position;
            initialPlacementPending = player != null;
            if (player != null)
            {
                body.position = (Vector2)player.position + new Vector2(-0.9f, -0.65f);
            }
        }

        public void Celebrate()
        {
            if (player != null)
            {
                var offset = body.position - (Vector2)player.position;
                celebrationStartAngle = offset.sqrMagnitude > 0.04f
                    ? Mathf.Atan2(offset.y, offset.x)
                    : -Mathf.PI * 0.75f;
            }

            celebrationElapsed = 0.001f;
            DogHeartEffect.Spawn((Vector2)transform.position + new Vector2(0f, 0.2f));
        }

        private void Update()
        {
            UpdatePlayerDirection();
            UpdateCelebration();
        }

        private void FixedUpdate()
        {
            if (player == null)
            {
                return;
            }

            var desiredPosition = GetDesiredPosition();
            if (initialPlacementPending)
            {
                initialPlacementPending = false;
                PlaceAtSafeCompanionPosition(desiredPosition);
                return;
            }

            if (IsCelebrating)
            {
                UpdateCelebrationOrbit();
                return;
            }

            var difference = desiredPosition - body.position;
            if (difference.sqrMagnitude < 0.04f)
            {
                return;
            }

            if (Vector2.Distance(body.position, player.position) > 8f)
            {
                PlaceAtSafeCompanionPosition(desiredPosition);
                return;
            }

            var direction = difference.normalized;
            var distance = Mathf.Min(FollowSpeed * Time.fixedDeltaTime, difference.magnitude);
            var move = MoveSafely(FindOpenMovement(direction, distance));
            if (Mathf.Abs(move.x) > 0.01f)
            {
                dogRenderer.flipX = move.x > 0f;
            }
        }

        private void UpdatePlayerDirection()
        {
            if (player == null)
            {
                return;
            }

            var currentPosition = (Vector2)player.position;
            var delta = currentPosition - lastPlayerPosition;
            if (delta.sqrMagnitude > 0.0005f)
            {
                lastPlayerDirection = delta.normalized;
            }

            lastPlayerPosition = currentPosition;
        }

        private Vector2 GetDesiredPosition()
        {
            var side = new Vector2(-lastPlayerDirection.y, lastPlayerDirection.x) * 0.24f;
            return (Vector2)player.position - (lastPlayerDirection * 1.05f) + side;
        }

        private void LateUpdate()
        {
            if (dogRenderer != null)
            {
                dogRenderer.sortingOrder = WorldDepthSorter.CalculateOrder(transform.position.y, 3);
            }

            if (body == null || bodyCollider == null)
            {
                return;
            }

            var currentPosition = body.position;
            if (IsPositionClear(currentPosition))
            {
                lastSafePosition = currentPosition;
                hasLastSafePosition = true;
                return;
            }

            if (hasLastSafePosition)
            {
                body.position = lastSafePosition;
                transform.position = lastSafePosition;
            }
        }

        private void PlaceAtSafeCompanionPosition(Vector2 preferredPosition)
        {
            if (IsPositionClear(preferredPosition))
            {
                SetPosition(preferredPosition);
                return;
            }

            if (player != null)
            {
                for (var index = 0; index < 12; index++)
                {
                    var angle = index * 30f * Mathf.Deg2Rad;
                    var candidate = (Vector2)player.position
                        + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * CelebrationOrbitRadius;
                    if (IsPositionClear(candidate))
                    {
                        SetPosition(candidate);
                        return;
                    }
                }
            }

            if (hasLastSafePosition)
            {
                SetPosition(lastSafePosition);
            }
        }

        private void SetPosition(Vector2 position)
        {
            body.position = position;
            transform.position = position;
            lastSafePosition = position;
            hasLastSafePosition = true;
        }

        private bool IsPositionClear(Vector2 position)
        {
            var filter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = false
            };
            var count = Physics2D.OverlapBox(position, bodyCollider.size, 0f, filter, overlapResults);
            for (var i = 0; i < count; i++)
            {
                var overlap = overlapResults[i];
                if (overlap != null && overlap != bodyCollider)
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2 FindOpenMovement(Vector2 direction, float distance)
        {
            if (!HasObstacle(direction, distance))
            {
                return direction * distance;
            }

            var left = new Vector2(-direction.y, direction.x);
            if (!HasObstacle(left, distance))
            {
                return left * distance;
            }

            var right = -left;
            return HasObstacle(right, distance) ? Vector2.zero : right * distance;
        }

        private Vector2 MoveSafely(Vector2 movement)
        {
            if (movement.sqrMagnitude < 0.000001f)
            {
                return Vector2.zero;
            }

            var destination = body.position + movement;
            if (!IsPositionClear(destination))
            {
                return Vector2.zero;
            }

            body.MovePosition(destination);
            return movement;
        }

        private bool HasObstacle(Vector2 direction, float distance)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            var count = bodyCollider.Cast(direction, filter, pathHits, distance + 0.03f);
            for (var index = 0; index < count; index++)
            {
                if (pathHits[index].collider != null && pathHits[index].collider != bodyCollider)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateCelebration()
        {
            if (!IsCelebrating)
            {
                return;
            }

            celebrationElapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(celebrationElapsed / CelebrationDuration);
            var stride = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * CelebrationTurns * 4f));
            visual.localPosition = Vector3.up * (stride * 0.16f);
            visual.localRotation = Quaternion.identity;
            visual.localScale = new Vector3(1f + (stride * 0.04f), 1f - (stride * 0.04f), 1f);
            if (progress < 1f)
            {
                return;
            }

            celebrationElapsed = 0f;
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one;
        }

        private void UpdateCelebrationOrbit()
        {
            var progress = Mathf.Clamp01(celebrationElapsed / CelebrationDuration);
            var angle = celebrationStartAngle + (progress * Mathf.PI * 2f * CelebrationTurns);
            var orbitOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * CelebrationOrbitRadius;
            var targetPosition = (Vector2)player.position + orbitOffset;
            var difference = targetPosition - body.position;
            var distance = Mathf.Min(FollowSpeed * 1.6f * Time.fixedDeltaTime, difference.magnitude);
            var move = difference.sqrMagnitude > 0.0001f
                ? FindOpenMovement(difference.normalized, distance)
                : Vector2.zero;
            MoveSafely(move);

            var tangent = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
            if (Mathf.Abs(tangent.x) > 0.01f)
            {
                dogRenderer.flipX = tangent.x > 0f;
            }
        }
    }
}
