using UnityEngine;

public class TestNPC : MonoBehaviour {
    public float detectionRadius = 5.0f;
    public LayerMask playerLayer;
    public DialogueManager dialogueManager;
    public DialogueData dialogue; // Reference to a Dialogue ScriptableObject
    private int currentDialogueIndex = 0;

    private bool playerInRange;
    private GameObject dialogueBox;
    private TypewriterEffect typewriterEffect;

    void Start() {
        dialogueBox = dialogueManager.CreateDialogueBox();
        dialogueBox.SetActive(false);
        typewriterEffect = dialogueBox.GetComponentInChildren<TypewriterEffect>();
    }

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
                currentDialogueIndex = 0;
                dialogueBox.SetActive(false);
            }
        }
    }

    private void ShowCurrentDialogue() {
        if (dialogue.dialogueLines.Count > 0 && currentDialogueIndex < dialogue.dialogueLines.Count) {
            dialogueManager.ShowDialogue(dialogueBox, dialogue.dialogueLines[currentDialogueIndex]);
            if (currentDialogueIndex == dialogue.dialogueLines.Count - 1 && dialogue.choices.Count > 0) {
                dialogueManager.ShowChoices(dialogueBox, dialogue.choices);
            }
        }
    }

    public void SelectChoice(int choiceIndex) {
        if (choiceIndex >= 0 && choiceIndex < dialogue.choices.Count) {
            DialogueChoice selectedChoice = dialogue.choices[choiceIndex];
            dialogue = selectedChoice.nextDialogue;
            currentDialogueIndex = 0;
            ShowCurrentDialogue();
        }
    }

    public void AdvanceDialogue() {
        if (playerInRange) {
            if (typewriterEffect.IsTyping()) {
                typewriterEffect.StopTypingAndShowFullText();
            }
            else {
                currentDialogueIndex++;
                if (currentDialogueIndex >= dialogue.dialogueLines.Count) {
                    currentDialogueIndex = 0;
                }
                ShowCurrentDialogue();
            }
        }
    }

    void OnDestroy() {
        if (dialogueBox != null) {
            dialogueManager.DestroyDialogueBox(dialogueBox);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}