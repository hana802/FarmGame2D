using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class PrototypePlayerController : MonoBehaviour
    {
        private const float MoveSpeed = 4f;
        private readonly Collider2D[] interactionResults = new Collider2D[12];

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveInput;

        public string NearbyPrompt { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = PrototypeSpriteFactory.CreateSprite(new Color(0.22f, 0.55f, 0.32f));
            spriteRenderer.sortingOrder = 10;
            transform.localScale = new Vector3(0.75f, 1f, 1f);
        }

        private void Start()
        {
            GameSession.Instance?.RegisterPlayer(transform);
            UpdateCharacterColor();
        }

        private void Update()
        {
            HandleCharacterSelection();
            if (GameSession.Instance == null || GameSession.Instance.Data.characterChoice == CharacterChoice.None)
            {
                moveInput = Vector2.zero;
                return;
            }

            moveInput = ReadMovementInput();
            UpdateNearbyInteraction();
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWithNearest();
            }
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + (moveInput * MoveSpeed * Time.fixedDeltaTime));
        }

        private static Vector2 ReadMovementInput()
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            if (Mathf.Approximately(horizontal, 0f))
            {
                horizontal = GetKeyboardAxis(KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow);
            }

            if (Mathf.Approximately(vertical, 0f))
            {
                vertical = GetKeyboardAxis(KeyCode.S, KeyCode.W, KeyCode.DownArrow, KeyCode.UpArrow);
            }

            return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }

        private static float GetKeyboardAxis(KeyCode negative, KeyCode positive, KeyCode alternateNegative, KeyCode alternatePositive)
        {
            if (Input.GetKey(negative) || Input.GetKey(alternateNegative))
            {
                return -1f;
            }

            return Input.GetKey(positive) || Input.GetKey(alternatePositive) ? 1f : 0f;
        }

        private void HandleCharacterSelection()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                GameSession.Instance?.ChooseCharacter(CharacterChoice.Male);
                UpdateCharacterColor();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                GameSession.Instance?.ChooseCharacter(CharacterChoice.Female);
                UpdateCharacterColor();
            }
        }

        private void UpdateCharacterColor()
        {
            var choice = GameSession.Instance?.Data.characterChoice ?? CharacterChoice.None;
            spriteRenderer.color = choice switch
            {
                CharacterChoice.Female => new Color(0.8f, 0.35f, 0.25f),
                CharacterChoice.Male => new Color(0.25f, 0.55f, 0.3f),
                _ => Color.gray
            };
        }

        private void UpdateNearbyInteraction()
        {
            var nearest = FindNearestInteractable();
            NearbyPrompt = nearest == null ? string.Empty : $"E: {nearest.Prompt}";
        }

        private void InteractWithNearest()
        {
            FindNearestInteractable()?.Interact(this);
        }

        private InteractableBehaviour FindNearestInteractable()
        {
            var count = Physics2D.OverlapCircleNonAlloc(transform.position, 1.35f, interactionResults);
            InteractableBehaviour nearest = null;
            var nearestDistance = float.MaxValue;

            for (var index = 0; index < count; index++)
            {
                var candidate = interactionResults[index].GetComponent<InteractableBehaviour>();
                if (candidate == null)
                {
                    continue;
                }

                var distance = Vector2.SqrMagnitude(candidate.transform.position - transform.position);
                if (distance >= nearestDistance)
                {
                    continue;
                }

                nearest = candidate;
                nearestDistance = distance;
            }

            return nearest;
        }
    }
}
