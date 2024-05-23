using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Dialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DialogueData proximityDialogue;
    public bool randomizeProximityDialogue;
    public DialogueData clickDialogue;
    protected int currentClickDialogueIndex = 0;
    protected GameObject dialogueBox;
    protected TypewriterEffect typewriterEffect;

    private void Awake() {
        if (dialogueManager == null) dialogueManager = FindAnyObjectByType<DialogueManager>();
    }

    protected virtual void Start() {
        dialogueBox = dialogueManager.CreateDialogueBox();
        dialogueBox.SetActive(false);
        typewriterEffect = dialogueBox.GetComponentInChildren<TypewriterEffect>();
    }

    protected virtual void ShowClickDialogue() {
        if (clickDialogue.dialogueLines.Count > 0 && currentClickDialogueIndex < clickDialogue.dialogueLines.Count) {
            dialogueManager.ShowDialogue(dialogueBox, clickDialogue.dialogueLines[currentClickDialogueIndex]);
            if (currentClickDialogueIndex == clickDialogue.dialogueLines.Count - 1 && clickDialogue.choices.Count > 0) {
                dialogueManager.ShowChoices(dialogueBox, clickDialogue.choices);
            }
        }
    }
    protected virtual void ShowProximityDialogue()
    {
        if (proximityDialogue.dialogueLines.Count > 0)
        {
            int index = randomizeProximityDialogue ? Random.Range(0, proximityDialogue.dialogueLines.Count) : 0;
            dialogueManager.ShowDialogue(dialogueBox, proximityDialogue.dialogueLines[index]);
        }
    }
    public void AdvanceClickDialogue() {
        if (typewriterEffect.IsTyping()) {
            typewriterEffect.StopTypingAndShowFullText();
        }
        else {
            currentClickDialogueIndex++;
            if (currentClickDialogueIndex >= clickDialogue.dialogueLines.Count) {
                if (clickDialogue.choices.Count > 0) {
                    dialogueManager.ShowChoices(dialogueBox, clickDialogue.choices);
                }
                else {
                    currentClickDialogueIndex = 0; // Reset index if there are no choices
                    dialogueBox.SetActive(false); // Hide dialogue box if no choices
                }
            }
            else {
                ShowClickDialogue();
            }
        }
    }
    public void SelectChoice(DialogueChoice choice) {
        clickDialogue = choice.nextDialogue;
        currentClickDialogueIndex = 0;
        ShowClickDialogue();
    }


    protected virtual void OnDestroy() {
        if (dialogueBox != null) {
            dialogueManager.DestroyDialogueBox(dialogueBox);
        }
    }
}
