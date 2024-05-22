using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Dialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DialogueData dialogue;
    protected int currentDialogueIndex = 0;
    protected GameObject dialogueBox;
    protected TypewriterEffect typewriterEffect;

    protected virtual void Start() {
        dialogueBox = dialogueManager.CreateDialogueBox();
        dialogueBox.SetActive(false);
        typewriterEffect = dialogueBox.GetComponentInChildren<TypewriterEffect>();
    }

    protected virtual void ShowCurrentDialogue() {
        if (dialogue.dialogueLines.Count > 0 && currentDialogueIndex < dialogue.dialogueLines.Count) {
            dialogueManager.ShowDialogue(dialogueBox, dialogue.dialogueLines[currentDialogueIndex]);
            if (currentDialogueIndex == dialogue.dialogueLines.Count - 1 && dialogue.choices.Count > 0) {
                dialogueManager.ShowChoices(dialogueBox, dialogue.choices);
            }
        }
    }

    public void SelectChoice(DialogueChoice choice) {
        dialogue = choice.nextDialogue;
        currentDialogueIndex = 0;
        ShowCurrentDialogue();
    }

    public void AdvanceDialogue() {
        if (typewriterEffect.IsTyping()) {
            typewriterEffect.StopTypingAndShowFullText();
        }
        else {
            currentDialogueIndex++;
            if (currentDialogueIndex >= dialogue.dialogueLines.Count) {
                if (dialogue.choices.Count > 0) {
                    dialogueManager.ShowChoices(dialogueBox, dialogue.choices);
                }
                else {
                    currentDialogueIndex = 0; // Reset index if there are no choices
                    dialogueBox.SetActive(false); // Hide dialogue box if no choices
                }
            }
            else {
                ShowCurrentDialogue();
            }
        }
    }

    protected virtual void OnDestroy() {
        if (dialogueBox != null) {
            dialogueManager.DestroyDialogueBox(dialogueBox);
        }
    }
}
