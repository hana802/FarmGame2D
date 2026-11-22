using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed partial class NpcInteractionOverlay
    {
        private bool choosingDivorce;

        private void DrawDivorceConfirm(Rect panel, GameSaveData data)
        {
            DrawRoundedSurface(new Rect(panel.x, panel.y + 5f, panel.width, panel.height),
                new Color32(45, 30, 17, 105), 20f);
            DrawRoundedSurface(panel, new Color32(255, 246, 223, 255), 20f);

            GUI.Label(new Rect(panel.x + 24f, panel.y + 22f, panel.width - 48f, 42f), "이혼", titleStyle);
            GUI.Label(new Rect(panel.x + 28f, panel.y + 78f, panel.width - 56f, 120f),
                $"5,000G를 내면 부부 사이가 끝나요.\n호감도는 20이 되고, 농장일 부탁도 끝나요.\n호감도를 다시 올리면 결혼할 수 있어요.\n지금 {data.money}G.",
                centeredStyle);

            var canPay = data.money >= BondEventService.DivorceCost;
            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && canPay;
            if (DrawDialogueButton(new Rect(panel.x + 36f, panel.yMax - 78f, 200f, 46f), "5,000G 내고 이혼",
                    canPay ? new Color32(232, 196, 184, 255) : new Color32(226, 216, 196, 255))
                && BondEventService.TryDivorce(data, npcId) == BondEventService.DivorceOutcome.Divorced)
            {
                choosingDivorce = false;
                dialogueLine = "알겠어요. 우리 이야기는 여기까지예요.";
                dialogueVisibleUntil = Time.unscaledTime + VillagerDialogueDismiss.Seconds;
                GameSession.Instance.ShowDialogue(speaker, dialogueLine, portraitRow, inInteraction: true);
                GameSession.Instance.SaveCurrentStateSilently();
            }

            GUI.enabled = previousEnabled;
            if (DrawDialogueButton(new Rect(panel.xMax - 236f, panel.yMax - 78f, 200f, 46f), "돌아가기",
                    new Color32(219, 228, 192, 255)))
                choosingDivorce = false;
        }
    }
}
