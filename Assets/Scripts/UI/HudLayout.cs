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
            Rect portrait,
            Rect title,
            Rect description,
            Rect hoeButton,
            Rect shovelButton,
            Rect seedsButton,
            Rect wateringCanButton,
            Rect pickaxeButton,
            Rect fishingRodButton,
            float firstStatsY,
            Rect closeButton)
        {
            Panel = panel;
            Portrait = portrait;
            Title = title;
            Description = description;
            HoeButton = hoeButton;
            ShovelButton = shovelButton;
            SeedsButton = seedsButton;
            WateringCanButton = wateringCanButton;
            PickaxeButton = pickaxeButton;
            FishingRodButton = fishingRodButton;
            FirstStatsY = firstStatsY;
            CloseButton = closeButton;
        }

        public Rect Panel { get; }
        public Rect Portrait { get; }
        public Rect Title { get; }
        public Rect Description { get; }
        public Rect HoeButton { get; }
        public Rect ShovelButton { get; }
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
        public PhoneHudLayout(
            Rect panel,
            Rect content,
            Rect cloudPanel,
            Rect title,
            Rect farmTabButton,
            Rect settingsTabButton,
            Rect questBody,
            Rect volumeLabel,
            Rect volumeSlider,
            Rect muteButton,
            Rect saveButton,
            Rect loadButton,
            Rect quitButton,
            Rect notice,
            Rect closeButton,
            Rect exitConfirmationPanel,
            Rect exitConfirmationMessage,
            Rect cancelExitButton,
            Rect confirmExitButton)
        {
            Panel = panel;
            Content = content;
            CloudPanel = cloudPanel;
            Title = title;
            FarmTabButton = farmTabButton;
            SettingsTabButton = settingsTabButton;
            QuestBody = questBody;
            VolumeLabel = volumeLabel;
            VolumeSlider = volumeSlider;
            MuteButton = muteButton;
            SaveButton = saveButton;
            LoadButton = loadButton;
            QuitButton = quitButton;
            Notice = notice;
            CloseButton = closeButton;
            ExitConfirmationPanel = exitConfirmationPanel;
            ExitConfirmationMessage = exitConfirmationMessage;
            CancelExitButton = cancelExitButton;
            ConfirmExitButton = confirmExitButton;
        }

        public Rect Panel { get; }
        public Rect Content { get; }
        public Rect CloudPanel { get; }
        public Rect Screen => new(Panel.x + 44f, Panel.y + 21f, Panel.width - 69f, Panel.height - 48f);
        public Rect Header => HudLayout.InsetRow(Screen, 14f, 12f, 60f);
        public Rect Title { get; }
        public Rect FarmTabButton { get; }
        public Rect SettingsTabButton { get; }
        public Rect QuestBody { get; }
        public Rect VolumeLabel { get; }
        public Rect VolumeSlider { get; }
        public Rect MuteButton { get; }
        public Rect SaveButton { get; }
        public Rect LoadButton { get; }
        public Rect QuitButton { get; }
        public Rect Notice { get; }
        public Rect CloseButton { get; }
        public Rect ExitConfirmationPanel { get; }
        public Rect ExitConfirmationMessage { get; }
        public Rect CancelExitButton { get; }
        public Rect ConfirmExitButton { get; }
    }

    public readonly struct QuestBoardHudLayout
    {
        public QuestBoardHudLayout(
            Rect panel,
            Rect title,
            Rect status,
            Rect refresh,
            Rect firstQuestButton,
            Rect closeButton)
        {
            Panel = panel;
            Title = title;
            Status = status;
            Refresh = refresh;
            FirstQuestButton = firstQuestButton;
            CloseButton = closeButton;
        }

        public Rect Panel { get; }
        public Rect Title { get; }
        public Rect Status { get; }
        public Rect Refresh { get; }
        public Rect FirstQuestButton { get; }
        public Rect CloseButton { get; }

        public Rect GetQuestButton(int index)
        {
            return new Rect(
                FirstQuestButton.x,
                FirstQuestButton.y + (index * 62f),
                FirstQuestButton.width,
                FirstQuestButton.height);
        }
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

    public readonly struct InteractionPromptHudLayout
    {
        public InteractionPromptHudLayout(Rect panel, Rect keycap, Rect text)
        {
            Panel = panel;
            Keycap = keycap;
            Text = text;
        }

        public Rect Panel { get; }
        public Rect Keycap { get; }
        public Rect Text { get; }
    }

    public readonly struct ShopHudLayout
    {
        public ShopHudLayout(Rect panel) => Panel = panel;

        public Rect Panel { get; }
        public Rect Header => new(Panel.x, Panel.y, Panel.width, 74f);
        public Rect Title => new(Panel.x + 88f, Panel.y + 10f, Panel.width - 318f, 54f);
        public Rect Balance => new(Panel.xMax - 219f, Panel.y + 12f, 145f, 50f);
        public Rect CloseButton => new(Panel.xMax - 60f, Panel.y + 16f, 42f, 42f);
        public Rect Tabs => new(Panel.x + 24f, Header.yMax + 16f, Panel.width - 48f, 46f);
        public Rect ListFooter => new(Panel.x + 26f, Panel.yMax - 66f, Panel.width - 52f, 42f);
        public Rect BackButton => new(Panel.x + 24f, Header.yMax + 16f, 152f, 44f);
        public Rect Detail => new(Panel.x + 24f, BackButton.yMax + 14f, Panel.width - 48f, 318f);
        public Rect DetailFooter => new(Panel.x + 26f, Panel.yMax - 76f, Panel.width - 266f, 52f);
        public Rect ConfirmButton => new(Panel.xMax - 216f, Panel.yMax - 76f, 192f, 52f);

        public Rect GetTab(int index, int count)
        {
            var width = (Tabs.width - 12f) / count;
            return new Rect(Tabs.x + 6f + index * width, Tabs.y + 4f, width, Tabs.height - 8f);
        }

        public Rect GetProduct(int row) => new(Tabs.x, Tabs.yMax + 14f + row * 83f, Tabs.width, 74f);
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
                new Rect(panel.x + 99f, panel.y + 36f, 104f, 104f),
                new Rect(panel.x + 235f, panel.y + 58f, panel.width - 275f, 50f),
                new Rect(panel.x + 55f, panel.y + 132f, panel.width - 110f, 32f),
                new Rect(panel.x + 48f, panel.y + 178f, 112f, 44f),
                new Rect(panel.x + 168f, panel.y + 178f, 112f, 44f),
                new Rect(panel.x + 288f, panel.y + 178f, 112f, 44f),
                new Rect(panel.x + 408f, panel.y + 178f, 112f, 44f),
                new Rect(panel.x + 528f, panel.y + 178f, 112f, 44f),
                new Rect(panel.x + 648f, panel.y + 178f, 112f, 44f),
                panel.y + 382f,
                new Rect(panel.x + 220f, panel.y + 447f, 380f, 40f));
        }

        public static PhoneHudLayout CalculatePhoneLayout(bool settings = false)
        {
            const float width = 992f;
            var panel = new Rect((ReferenceWidth - width) * 0.5f, 64f, width, 592f);
            var content = new Rect(panel.x + 78f, panel.y + 161f, 418f, 360f);
            var cloud = new Rect(panel.x + 532f, content.y, 412f, content.height);
            var confirmation = new Rect(panel.center.x - 202f, content.y + 29f, 404f, 270f);
            return new PhoneHudLayout(
                panel,
                content, cloud,
                new Rect(panel.center.x - 140f, panel.y + 40f, 280f, 42f),
                new Rect(content.x, panel.y + 103f, 116f, 39f),
                new Rect(content.x + 128f, panel.y + 103f, 116f, 39f),
                InsetRow(content, 22f, 83f, 43f),
                InsetRow(content, 22f, 67f, 32f),
                InsetRow(content, 22f, 119f, 32f),
                new Rect(content.x + 22f, content.y + 178f, 144f, 43f),
                settings
                    ? InsetRow(cloud, 22f, 111f, 45f)
                    : new Rect(content.x + 22f, content.y + 270f, 167f, 45f),
                new Rect(content.xMax - 189f, content.y + 270f, 167f, 45f),
                InsetRow(cloud, 22f, 171f, 45f),
                InsetRow(cloud, 22f, 247f, 68f),
                new Rect(panel.xMax - 88f, panel.y + 38f, 48f, 48f),
                confirmation,
                InsetRow(confirmation, 25f, 68f, 87f),
                new Rect(confirmation.x + 25f, confirmation.y + 188f, 170f, 45f),
                new Rect(confirmation.xMax - 195f, confirmation.y + 188f, 170f, 45f));
        }

        public static Rect InsetRow(Rect panel, float horizontalPadding, float top, float height)
        {
            return new Rect(panel.x + horizontalPadding, panel.y + top, panel.width - horizontalPadding * 2f, height);
        }

        public static QuestBoardHudLayout CalculateQuestBoardLayout()
        {
            const float width = 392f;
            var panel = new Rect(ReferenceWidth - width - 36f, 144f, width, 430f);
            return new QuestBoardHudLayout(
                panel,
                InsetRow(panel, 28f, 26f, 38f),
                InsetRow(panel, 28f, 79f, 32f),
                InsetRow(panel, 28f, 116f, 28f),
                InsetRow(panel, 28f, 154f, 52f),
                new Rect(panel.xMax - 137f, panel.yMax - 66f, 109f, 36f));
        }

        public static ToastHudLayout CalculateToastLayout()
        {
            var panel = new Rect((ReferenceWidth - 620f) * 0.5f, 132f, 620f, 72f);
            return new ToastHudLayout(
                panel,
                new Rect(panel.x + 140f, panel.y + 12f, panel.width - 172f, 48f));
        }

        public static InteractionPromptHudLayout CalculateInteractionPromptLayout()
        {
            var panel = new Rect((ReferenceWidth - 360f) * 0.5f, 586f, 360f, 44f);
            return new InteractionPromptHudLayout(
                panel,
                new Rect(panel.x + 10f, panel.y + 8f, 28f, 28f),
                new Rect(panel.x + 50f, panel.y + 5f, panel.width - 62f, 34f));
        }

        public static ShopHudLayout CalculateShopLayout()
        {
            const float width = 770f;
            return new ShopHudLayout(new Rect((ReferenceWidth - width) * 0.5f, 65f, width, 590f));
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
