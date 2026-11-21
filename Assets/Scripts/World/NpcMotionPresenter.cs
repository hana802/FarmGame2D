using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Player;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class NpcMotionPresenter : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private WanderingMover mover;
        private int sheetRow;
        private int worldVariant;
        private CharacterChoice playerArt;
        private int facingRow = 3;
        private int displayedRow = -1;
        private int displayedFrame = -1;

        public void Configure(int npcSheetRow, CharacterChoice characterArt = CharacterChoice.None, int variant = 0)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            sheetRow = npcSheetRow;
            playerArt = characterArt;
            worldVariant = variant;
        }

        public void Face(Vector2 target)
        {
            var direction = target - (Vector2)transform.position;
            facingRow = PlayerMotionPresentation.GetFacingRow(direction, facingRow);
        }

        private void Start()
        {
            mover = GetComponent<WanderingMover>();
        }

        private void LateUpdate()
        {
            if (spriteRenderer == null || mover == null) return;

            var moving = mover.IsMoving && mover.MovementDirection != Vector2.zero;
            if (moving)
                facingRow = PlayerMotionPresentation.GetFacingRow(mover.MovementDirection, facingRow);

            var frame = playerArt == CharacterChoice.None
                ? (moving ? Mathf.FloorToInt(Time.time * 6f) % 2 : 0)
                : PlayerMotionPresentation.GetAnimationFrame(Time.time, moving);
            if (facingRow == displayedRow && frame == displayedFrame) return;

            var sprite = worldVariant > 0
                ? GameArtCatalog.GetVillagerVariantSprite(worldVariant, facingRow, frame != 0)
                : playerArt == CharacterChoice.None
                ? GameArtCatalog.GetNpcDirectionalSprite(sheetRow, facingRow, frame != 0)
                : GameArtCatalog.GetPlayerSprite(playerArt, facingRow, frame);
            if (sprite == null) return;

            spriteRenderer.sprite = sprite;
            displayedRow = facingRow;
            displayedFrame = frame;
        }
    }
}
