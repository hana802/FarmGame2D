using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class PrototypePlayerController : MonoBehaviour
    {
        private readonly Collider2D[] interactionResults = new Collider2D[12];

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private HeldItemPresenter heldItemPresenter;
        private Vector2 moveInput;
        private CharacterChoice displayedChoice;
        private int displayedRow = -1;
        private int displayedFrame = -1;
        private int facingRow = 3;
        private float nearbyPromptVisibleUntil;

        public string NearbyPrompt { get; private set; }
        public string VisibleNearbyPrompt => Time.unscaledTime < nearbyPromptVisibleUntil ? NearbyPrompt : string.Empty;
        public HeldItemPresenter HeldItemPresenter => heldItemPresenter;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var playerCollider = GetComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(0.5f, 0.32f);
            playerCollider.offset = new Vector2(0f, 0.14f);
            spriteRenderer = GetComponent<SpriteRenderer>();
            gameObject.AddComponent<WorldDepthSorter>().Configure(4);
            heldItemPresenter = gameObject.AddComponent<HeldItemPresenter>();
            transform.localScale = Vector3.one;
        }

        private void Start()
        {
            GameSession.Instance?.RegisterPlayer(transform);
            UpdateCharacterVisual(true);
        }

        private void Update()
        {
            HandleCharacterSelection();
            if (GameSession.Instance == null || GameSession.Instance.Data.characterChoice == CharacterChoice.None)
            {
                moveInput = Vector2.zero;
                heldItemPresenter.Present(FarmTool.Hoe, facingRow, false, false);
                return;
            }

            if (GameSession.Instance.InventoryOpen || GameSession.Instance.PhoneOpen || GameSession.Instance.ShopOpen)
            {
                moveInput = Vector2.zero;
                UpdateCharacterVisual(false);
                UpdateHeldItem();
                NearbyPrompt = string.Empty;
                nearbyPromptVisibleUntil = 0f;
                return;
            }

            moveInput = ReadMovementInput();
            UpdateFacingDirection();
            UpdateCharacterVisual(false);
            UpdateHeldItem();
            UpdateNearbyInteraction();
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWithNearest();
            }
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + (moveInput * PlayerMotionPresentation.MoveSpeed * Time.fixedDeltaTime));
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
                UpdateCharacterVisual(true);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                GameSession.Instance?.ChooseCharacter(CharacterChoice.Female);
                UpdateCharacterVisual(true);
            }
        }

        private void UpdateFacingDirection()
        {
            facingRow = PlayerMotionPresentation.GetFacingRow(moveInput, facingRow);
        }

        private void UpdateCharacterVisual(bool force)
        {
            var choice = GameSession.Instance?.Data.characterChoice ?? CharacterChoice.None;
            spriteRenderer.enabled = choice != CharacterChoice.None;
            if (choice == CharacterChoice.None)
            {
                return;
            }

            var frame = PlayerMotionPresentation.GetAnimationFrame(Time.time, moveInput.sqrMagnitude > 0.01f);
            if (!force && displayedChoice == choice && displayedRow == facingRow && displayedFrame == frame)
            {
                return;
            }

            var artSprite = GameArtCatalog.GetPlayerSprite(choice, facingRow, frame);
            if (artSprite == null)
            {
                return;
            }

            spriteRenderer.sprite = artSprite;
            spriteRenderer.color = Color.white;
            displayedChoice = choice;
            displayedRow = facingRow;
            displayedFrame = frame;
        }

        private void UpdateNearbyInteraction()
        {
            var nearest = FindNearestInteractable();
            var nextPrompt = nearest == null ? string.Empty : $"E: {nearest.Prompt}";
            if (nextPrompt == NearbyPrompt)
            {
                return;
            }

            NearbyPrompt = nextPrompt;
            nearbyPromptVisibleUntil = string.IsNullOrEmpty(nextPrompt) ? 0f : Time.unscaledTime + 1f;
        }

        private void UpdateHeldItem()
        {
            var session = GameSession.Instance;
            var available = session.SelectedTool != FarmTool.Seeds
                || session.Data.inventory.Count(session.SelectedSeed) > 0;
            heldItemPresenter.Present(session.SelectedTool, facingRow, moveInput.sqrMagnitude > 0.01f, available);
        }

        private void InteractWithNearest()
        {
            FindNearestInteractable()?.Interact(this);
        }

        public void Teleport(Vector2 destination)
        {
            moveInput = Vector2.zero;
            body.position = destination;
            transform.position = destination;
            NearbyPrompt = string.Empty;
            nearbyPromptVisibleUntil = 0f;
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
