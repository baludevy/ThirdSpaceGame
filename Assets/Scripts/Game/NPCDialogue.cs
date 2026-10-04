using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    
    public class NPCDialogue : MonoBehaviour, Interactable
    {
        [Serializable]
        public class DialogueLine
        {
            public string speaker;

            [TextArea(2, 6)]
            public string text;

            public Sprite portrait;
            public  AudioClip voiceClip;

            public DialogueLine()
            {
                speaker = string.Empty;
                text = string.Empty;
            }

            public DialogueLine(string speaker, string text)
            {
                speaker = string.Empty;
                text = string.Empty;
            }

            public DialogueLine Copy()
            {
                return new DialogueLine(speaker, text)
                {
                    portrait = portrait,
                    voiceClip = voiceClip,
                };
            }
        }
        [Header("Interaction")]
        [SerializeField] private ushort interactionId;
        [SerializeField] private InteractionKind interactionKind;
        [SerializeField] private bool interactionAllowed = true;

        [Header("NPC")]
        [SerializeField] private string npcName = "NPC";

        [Header("Dialogue")]
        [SerializeField] private bool canRepeat = true;
        [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

        private bool isTalking;
        private bool hasCompleted;
        private int currentLineIndex = -1;

        public event Action DialogueStarted;
        public event Action<DialogueLine, int, int> LineChanged;
        public event Action DialogueCompleted;
        public event Action DialogueCancelled;

        public string NpcName => npcName;
        public bool IsTalking => isTalking;
        public bool HasCompleted => hasCompleted;
        public bool CanRepeat => CanRepeat;
        public int LineCount => lines == null ? 0 : lines.Count;
        public int CurrentLineIndex => currentLineIndex;

        public bool IsOnLastLine => isTalking && currentLineIndex == LineCount - 1;

        public DialogueLine CurrentLine
        {
            get
            {
                if (!isTalking)
                    return null;

                return GetLine(currentLineIndex);
            }
        }

        public bool CanAdvance { get; internal set; }

        public ushort GetId()
        {
            return interactionId;
        }

        public InteractionKind GetInteractionKind()
        {
            return interactionKind;
        }

        public bool CanInteract()
        {
            if (!isActiveAndEnabled || !interactionAllowed)
                return false;

            if (LineCount == 0)
                return false;

            if (isTalking)
                return true;
            
            return canRepeat || !hasCompleted;
        }

        public void Interact()
        {
            if (!CanInteract())
                return;
            
            if (isTalking)
                AdvanceDialogue();
            else
                StartDialogue();
        }

        public bool StartDialogue()
        {
            if (isTalking || !CanInteract())
                return false;

            isTalking = true;
            currentLineIndex = 0;

            DialogueStarted?.Invoke();

            if (isTalking && currentLineIndex == 0)
                NotifyCurrentLine();

            return true;
        }

        public bool AdvanceDialogue()
        {
            if (!isTalking || !CanInteract())
                return false;

            int nextIndex = currentLineIndex + 1;

            if (nextIndex >= LineCount)
            {
                CompleteDialogue();
                return true;
            }
            
            currentLineIndex = nextIndex;
            NotifyCurrentLine();
            return true;
        }

        public bool CancelDialogue()
        {
            if (!isTalking)
                return false;

            isTalking = false;
            currentLineIndex = -1;
            DialogueCancelled?.Invoke();
            return true;
        }

        public void ResetDialogue()
        {
            hasCompleted = false;
            CancelDialogue();
        }

        public void SetInteractionAllowed(bool allowed)
        {
            interactionAllowed = allowed;

            if (!allowed)
                CancelDialogue();
        }

        public void SetCanRepeat(bool allowed)
        {
            canRepeat = allowed;
        }

        public DialogueLine GetLine(int index)
        {
            if (index < 0 || index >= LineCount)
            return null;

        DialogueLine line = lines[index];

        DialogueLine result = line == null
            ? new DialogueLine()
            : line.Copy();

        if (string.IsNullOrWhiteSpace(result.speaker))
            result.speaker = npcName;

            return result;
        }
        public string GetCurrentText()
        {
            DialogueLine line = CurrentLine;
            return line == null ? string.Empty : line.text;
        }

        public string GetCurrentSpeaker()
        {
            DialogueLine line = CurrentLine;
            return line == null ? string.Empty : line.speaker;
        }
        private void NotifyCurrentLine()
        {
            DialogueLine line = CurrentLine;

            if (line != null)
                LineChanged?.Invoke(line, currentLineIndex, LineCount);
        }

        private void CompleteDialogue()
        {
            isTalking = false;
            hasCompleted = true;
            currentLineIndex = -1;
            DialogueCompleted?.Invoke();
        }

        private void OnDisable()
        {
            CancelDialogue();
        }
    }
}