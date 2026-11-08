using System;
using System.Collections.Generic;

namespace FarmGame2D.Interactions
{
    public sealed class NpcDialogueSequence
    {
        private readonly string[] lines;
        private int nextIndex;

        public NpcDialogueSequence(IReadOnlyList<string> dialogueLines)
        {
            if (dialogueLines == null || dialogueLines.Count == 0)
            {
                throw new ArgumentException("At least one dialogue line is required.", nameof(dialogueLines));
            }

            lines = new string[dialogueLines.Count];
            for (var index = 0; index < dialogueLines.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(dialogueLines[index]))
                {
                    throw new ArgumentException("Dialogue lines cannot be empty.", nameof(dialogueLines));
                }

                lines[index] = dialogueLines[index];
            }
        }

        public int Count => lines.Length;

        public string TakeNext()
        {
            var line = lines[nextIndex];
            nextIndex = (nextIndex + 1) % lines.Length;
            return line;
        }
    }
}
