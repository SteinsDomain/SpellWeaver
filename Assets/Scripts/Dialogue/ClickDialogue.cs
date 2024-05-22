using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickDialogue : Dialogue
{
    public void Interact() {
        if (!dialogueBox.activeSelf) {
            ShowCurrentDialogue();
            dialogueBox.SetActive(true);
        }
        else {
            AdvanceDialogue();
        }
    }

    void Update() {
        if (dialogueBox.activeSelf) {
            Vector3 dialoguePosition = transform.position + Vector3.up * 1.5f;
            dialogueBox.transform.position = dialoguePosition;
        }
    }
}
