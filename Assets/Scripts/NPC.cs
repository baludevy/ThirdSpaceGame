using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour , Interactable
{
    public NPCDialogue dialoguedata;
    public GameObject dialoguepanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool istyping, isdialogueactive;
    public bool CanInteract()
    {
    return !isdialogueactive;
    }

    public void Interact()
    {
        if (dialoguedata != null || (PauseController.IsGamePaused && !isdialogueactive))
        return;
        if (isdialogueactive)
        {
           //Nextline
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isdialogueactive = true;
        dialoguepanel.SetActive(true);
        nameText.SetText(dialoguedata.npcName);
        portraitImage.sprite = dialoguedata.portrait;
        dialogueIndex = 0;
        PauseController.SetPause(true);

    StartCoroutine(TypeLine());
    void Nextline()
        {
            if (isTyping)
            {
                StopAllCroutines();
                dialogueText.SetText(dialoguedata.dialoguelines[dialogueIndex]);
                istyping = false;
            }
            else if(++dialogueindex < dialoguedata.dialoguelines.Length)
            {
                StartCoroutine(TypeLine());
            }
            else
            {
               // EndDialogue();
            }
        }
    }
    IEnumerator TypeLine()
    {
     istyping = true;
     dialogueText.SetText("");
     foreach (char letter in dialoguedata.dialogue[dialogueIndex])
     {
        dialogueText.text += letter;
        yield return new WaitForSeconds(dialoguedata.typingSpeed);
        istyping = false;
        if(dialoguedata.autoprogress.length > dialogueIndex && dialoguedata.autoprogressLines[dialogueIndex])
            {
                yield return new WaitForSeconds(dialoguedata.autoprogressDelay);
                Nextline();
            }
     }
    }
}

