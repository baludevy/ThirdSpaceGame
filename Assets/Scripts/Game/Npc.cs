using UnityEngine;

namespace Client
{
    public class NPC : MonoBehaviour, Interactable
    {
        [Header("Identity")]
        [SerializeField] private ushort interactionId;
        [SerializeField] private InteractionKind interactionKind;
        [SerializeField] private string npcName = "NPC";

        [Header("Interaction")]
        [SerializeField] private bool interactionAllowed = true;
        [SerializeField] private NPCDialogue dialogue;

        public string NpcName => npcName;
        public NPCDialogue Dialogue => dialogue;
        public bool IsTalking => dialogue != null && dialogue.IsTalking;

        public bool CanInteract()
        {
            throw new System.NotImplementedException();
        }

        public ushort GetId()
        {
            return interactionId;
        }

        public InteractionKind GetInteractionKind()
        {
            return interactionKind;
        }

        // public bool CanInteract()
        //{
        //     if (!isActiveAndEnabled)
       //         return false;

    //            if (!interactionAllowed || dialogue == null)
      //          return false;

//            if (dialogue.IsTalking)
  //              return dialogue.CanAdvance;

            //return dialogue.CanStart;
    //.    }

        public void Interact()
        {
          //  if (!CanInteract())
         //       return;

     //       if (dialogue.IsTalking)
       //         dialogue.AdvanceDialogue();
           // else
        //        dialogue.StartDialogue(npcName);
        }
// asked
//        public bool Choose(int choiceIndex)
        //{
      //      if (!isActiveAndEnabled || !interactionAllowed)
          //      return false;

      //      if (dialogue == null)
        //        return false;

           //return dialogue.Choose(choiceIndex);
        //}

        public void SetInteractionAllowed(bool allowed)
        {
            interactionAllowed = allowed;

            if (!allowed && dialogue != null)
                dialogue.CancelDialogue();
        }

        private void OnDisable()
        {
            if (dialogue != null)
                dialogue.CancelDialogue();
        }
    }
}