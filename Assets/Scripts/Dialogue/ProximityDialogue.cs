using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityDialogue : Dialogue
{
    public float detectionRadius = 5.0f;
    public LayerMask playerLayer;
    private bool playerInRange;
    public bool randomizeDialogue;

    void Update() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
        playerInRange = hitColliders.Length > 0;

        if (playerInRange) {
            if (!dialogueBox.activeSelf) {
                ShowCurrentDialogue();
                dialogueBox.SetActive(true);
            }

            Vector3 dialoguePosition = transform.position + Vector3.up * 1.5f;
            dialogueBox.transform.position = dialoguePosition;
        }
        else {
            if (dialogueBox.activeSelf) {
                currentClickDialogueIndex = 0;
                dialogueBox.SetActive(false);
            }
        }
    }

    protected override void ShowCurrentDialogue() {
        if (clickDialogue.dialogueLines.Count > 0) {
            int index = randomizeDialogue ? Random.Range(0, clickDialogue.dialogueLines.Count) : 0;
            dialogueManager.ShowDialogue(dialogueBox, clickDialogue.dialogueLines[index]);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
