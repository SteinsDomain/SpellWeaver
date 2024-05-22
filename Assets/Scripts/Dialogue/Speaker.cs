using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : Dialogue, IInteractable
{
    public float detectionRadius = 5.0f;
    public LayerMask playerLayer;
    private bool playerInRange;
    private bool isProximityDialogueActive;

    void Update()
    {
        // Handle proximity detection
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
        playerInRange = hitColliders.Length > 0;

        if (playerInRange)
        {
            if (!dialogueBox.activeSelf)
            {
                ShowProximityDialogue();
                dialogueBox.SetActive(true);
                isProximityDialogueActive = true;
            }

            Vector3 dialoguePosition = transform.position + Vector3.up * 1.5f;
            dialogueBox.transform.position = dialoguePosition;
        }
        else
        {
            if (dialogueBox.activeSelf)
            {
                dialogueBox.SetActive(false);
                isProximityDialogueActive = false;
                currentClickDialogueIndex = 0;
            }
        }
    }

    public void Interact()
    {
        if (playerInRange)
        {
            if (isProximityDialogueActive)
            {
                // Hide proximity dialogue and reset state
                dialogueBox.SetActive(false);
                isProximityDialogueActive = false;
                currentClickDialogueIndex = 0;
            }

            if (clickDialogue.dialogueLines.Count > 0)
            {
                if (!dialogueBox.activeSelf)
                {
                    ShowCurrentDialogue();
                    dialogueBox.SetActive(true);
                }
                else
                {
                    AdvanceDialogue();
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
