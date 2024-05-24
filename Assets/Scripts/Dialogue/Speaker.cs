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
        CheckForPlayer();

        if (playerInRange)
        {
            StartTalking();
        }
        else
        {
            StopTalking();
        }
    }
    private void CheckForPlayer()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
        playerInRange = hitColliders.Length > 0;
    }
    private void StartTalking()
    {
        if (!dialogueBox.activeSelf)
        {
            HandleProximityDialogue();
        }

        SetDialogueBoxPosition();
    }
    private void StopTalking()
    {
        if (dialogueBox.activeSelf)
        {
            ResetDialogue();
        }
    }
    private void ResetDialogue()
    {
        if (dialogueBox != null) {
            var textComponent = dialogueBox.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent != null) {
                textComponent.text = string.Empty;
            }
        }
        dialogueBox.SetActive(false);
        isProximityDialogueActive = false;
        currentClickDialogueIndex = 0;
    }

    private void SetDialogueBoxPosition()
    {
        Vector3 dialoguePosition = transform.position + Vector3.up * 2f;
        dialogueBox.transform.position = dialoguePosition;
    }
    private void HandleProximityDialogue()
    {
        ShowProximityDialogue();
        dialogueBox.SetActive(true);
        isProximityDialogueActive = true;
    }
    private void HandleClickDialogue()
    {
        if (clickDialogue.dialogueLines.Count > 0)
        {
            if (!dialogueBox.activeSelf)
            {
                ShowClickDialogue();
                dialogueBox.SetActive(true);
            }
            else
            {
                AdvanceClickDialogue();
            }
        }
    }

    public void Interact(GameObject interactor)
    {
        if (playerInRange)
        {
            if (clickDialogue.dialogueLines.Count > 0) {
                if (isProximityDialogueActive) {
                    StopTalking();
                }

                HandleClickDialogue();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
