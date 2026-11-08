using UnityEngine;

namespace FarmGame2D.UI
{
    public readonly struct HudViewport
    {
        public HudViewport(float scale, float offsetX, float offsetY)
        {
            Scale = scale;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public float Scale { get; }
        public float OffsetX { get; }
        public float OffsetY { get; }
    }

    public readonly struct DialogueHudLayout
    {
        public DialogueHudLayout(Rect panel, Rect portrait, Rect name, Rect body)
        {
            Panel = panel;
            Portrait = portrait;
            Name = name;
            Body = body;
        }

        public Rect Panel { get; }
        public Rect Portrait { get; }
        public Rect Name { get; }
        public Rect Body { get; }
    }

    public readonly struct InventoryHudLayout
    {
        public InventoryHudLayout(
            Rect panel,
            Rect title,
            Rect description,
            Rect hoeButton,
            Rect seedsButton,
            Rect wateringCanButton,
            Rect pickaxeButton,
            Rect fishingRodButton,
            float firstStatsY,
            Rect closeButton)
        {
            Panel = panel;
            Title = title;
            Description = description;
            HoeButton = hoeButton;
            SeedsButton = seedsButton;
            WateringCanButton = wateringCanButton;
            PickaxeButton = pickaxeButton;
            FishingRodButton = fishingRodButton;
            FirstStatsY = firstStatsY;
            CloseButton = closeButton;
        }

        public Rect Panel { get; }
        public Rect Title { get; }
        public Rect Description { get; }
        public Rect HoeButton { get; }
        public Rect SeedsButton { get; }
        public Rect WateringCanButton { get; }
        public Rect PickaxeButton { get; }
        public Rect FishingRodButton { get; }
        public float ToolButtonsY => HoeButton.y;
        public float FirstStatsY { get; }
        public Rect CloseButton { get; }

        public Rect GetStatsLabel(int column, int row)
        {
            var x = Panel.x + (column == 0 ? 48f : 420f);
            return new Rect(x, FirstStatsY + (row * 44f), 352f, 42f);
        }

        public Rect GetSeedButton(int index)
        {
            var column = index % 2;
            var row = index / 2;
            return new Rect(Panel.x + 48f + (column * 372f), Panel.y + 240f + (row * 34f), 344f, 30f);
        }
    }

    public readonly struct PhoneHudLayout
    {
        public PhoneHudLayout(Rect panel, Rect title, Rect questBody, Rect saveButton, Rect loadButton, Rect closeButton)
        {
            Panel = panel;
            Title = title;
            QuestBody = questBody;
            SaveButton = saveButton;
            LoadButton = loadButton;
            CloseButton = closeButton;
        }

        public Rect Panel { get; }
        public Rect Title { get; }
        public Rect QuestBody { get; }
        public Rect SaveButton { get; }
        public Rect LoadButton { get; }
        public Rect CloseButton { get; }
    }

    public readonly struct ToastHudLayout
    {
        public ToastHudLayout(Rect panel, Rect message)
        {
            Panel = panel;
            Message = message;
        }

        public Rect Panel { get; }
        public Rect Message { get; }
    }

    public readonly struct ShopHudLayout
    {
        public ShopHudLayout(Rect panel, Rect title, float firstProductY, Rect closeButton)
        {
            Panel = panel;
            Title = title;
            FirstProductY = firstProductY;
            CloseButton = closeButton;
        }

        public Rect Panel { get; }
        public Rect Title { get; }
        public float FirstProductY { get; }
        public Rect CloseButton { get; }

        public Rect GetProductButton(int index)
        {
            return new Rect(Panel.x + 45f, FirstProductY + (index * 58f), 530f, 48f);
        }

        public Rect GetHalfWidthButton(int row, bool right)
        {
            return new Rect(Panel.x + (right ? 325f : 45f), FirstProductY + (row * 58f), 250f, 48f);
        }

        public Rect GetInfoLabel(int row, float height)
        {
            return new Rect(Panel.x + 55f, FirstProductY + (row * 58f), 510f, height);
        }
    }

    public static class HudLayout
    {
        public const float ReferenceWidth = 1280f;
        public const float ReferenceHeight = 720f;
        private const float DialogueAtlasWidth = 1618f;
        private const float DialogueAtlasHeight = 356f;

        public static HudViewport CalculateViewport(float screenWidth, float screenHeight)
        {
            var scale = Mathf.Min(screenWidth / ReferenceWidth, screenHeight / ReferenceHeight);
            var offsetX = (screenWidth - (ReferenceWidth * scale)) * 0.5f;
            var offsetY = (screenHeight - (ReferenceHeight * scale)) * 0.5f;
            return new HudViewport(scale, offsetX, offsetY);
        }

        public static DialogueHudLayout CalculateDialogueLayout()
        {
            const float panelWidth = 1100f;
            var panelHeight = panelWidth * DialogueAtlasHeight / DialogueAtlasWidth;
            var panel = new Rect((ReferenceWidth - panelWidth) * 0.5f, 640f - panelHeight, panelWidth, panelHeight);
            return new DialogueHudLayout(
                panel,
                ScaleDialogueSlot(panel, new Rect(80f, 107f, 201f, 193f)),
                ScaleDialogueSlot(panel, new Rect(362f, 27f, 335f, 50f)),
                ScaleDialogueSlot(panel, new Rect(355f, 118f, 1170f, 165f)));
        }

        public static InventoryHudLayout CalculateInventoryLayout()
        {
            var panel = new Rect((ReferenceWidth - 820f) * 0.5f, 70f, 820f, 550f);
            return new InventoryHudLayout(
                panel,
                new Rect(panel.x + 235f, panel.y + 60f, panel.width - 275f, 38f),
                new Rect(panel.x + 55f, panel.y + 132f, panel.width - 110f, 32f),
                new Rect(panel.x + 48f, panel.y + 178f, 126f, 44f),
                new Rect(panel.x + 198f, panel.y + 178f, 126f, 44f),
                new Rect(panel.x + 348f, panel.y + 178f, 126f, 44f),
                new Rect(panel.x + 498f, panel.y + 178f, 126f, 44f),
                new Rect(panel.x + 648f, panel.y + 178f, 126f, 44f),
                panel.y + 382f,
                new Rect(panel.x + 235f, panel.y + 449f, 170f, 36f));
        }

        public static PhoneHudLayout CalculatePhoneLayout()
        {
            var panel = new Rect((ReferenceWidth - 400f) * 0.5f, 150f, 400f, 350f);
            return new PhoneHudLayout(
                panel,
                new Rect(panel.x + 130f, panel.y + 34f, 230f, 40f),
                new Rect(panel.x + 45f, panel.y + 105f, 310f, 82f),
                new Rect(panel.x + 40f, panel.y + 195f, 140f, 48f),
                new Rect(panel.x + 220f, panel.y + 195f, 140f, 48f),
                new Rect(panel.x + 130f, panel.y + 270f, 140f, 48f));
        }

        public static ToastHudLayout CalculateToastLayout()
        {
            var panel = new Rect((ReferenceWidth - 620f) * 0.5f, 132f, 620f, 72f);
            return new ToastHudLayout(
                panel,
                new Rect(panel.x + 140f, panel.y + 12f, panel.width - 172f, 48f));
        }

        public static ShopHudLayout CalculateShopLayout()
        {
            var panel = new Rect((ReferenceWidth - 620f) * 0.5f, 92f, 620f, 550f);
            return new ShopHudLayout(
                panel,
                new Rect(panel.x + 120f, panel.y + 58f, 380f, 38f),
                panel.y + 146f,
                new Rect(panel.x + 170f, panel.y + 446f, 280f, 44f));
        }

        private static Rect ScaleDialogueSlot(Rect panel, Rect atlasSlot)
        {
            var scale = panel.width / DialogueAtlasWidth;
            return new Rect(
                panel.x + (atlasSlot.x * scale),
                panel.y + (atlasSlot.y * scale),
                atlasSlot.width * scale,
                atlasSlot.height * scale);
        }
    }
}
