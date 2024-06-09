using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

    public GameObject dialogueBoxPrefab; // Assign a prefab with DialogueBackground and Text (TMP)
    public GameObject choiceButtonPrefab; // Assign a prefab for choice buttons
    private List<GameObject> activeDialogueBoxes = new List<GameObject>();

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public GameObject CreateDialogueBox() {
        GameObject newDialogueBox = Instantiate(dialogueBoxPrefab, transform);
        activeDialogueBoxes.Add(newDialogueBox);
        return newDialogueBox;
    }
    public void DestroyDialogueBox(GameObject dialogueBox) {
        if (activeDialogueBoxes.Contains(dialogueBox)) {
            activeDialogueBoxes.Remove(dialogueBox);
            Destroy(dialogueBox);
        }
    }

    public void ShowDialogue(GameObject dialogueBox, string text) {
        var typewriterEffect = GetTypewriterEffect(dialogueBox);
        if (typewriterEffect != null) {
            dialogueBox.SetActive(true); // Ensure the dialogue box is active
            StartCoroutine(StartTypingWithDelay(typewriterEffect, text));
        }
        else {
            Debug.LogError("TypewriterEffect component not found on dialogueBox.");
        }
    }
    public void ShowChoices(GameObject dialogueBox, List<DialogueChoice> choices) {
        Debug.Log("ShowChoices called");
        Transform choiceContainer = dialogueBox.transform.Find("ChoiceContainer");
        if (choiceContainer == null) {
            Debug.LogError("ChoiceContainer not found in dialogueBox.");
            return;
        }

        Debug.Log("ChoiceContainer found");
        Debug.Log("ChoiceContainer active: " + choiceContainer.gameObject.activeSelf);

        foreach (Transform child in choiceContainer) {
            Destroy(child.gameObject);
        }

        Debug.Log("Existing choices cleared");

        foreach (DialogueChoice choice in choices) {
            Debug.Log("Creating choice button for: " + choice.choiceText);
            GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);

            if (choiceButton == null) {
                Debug.LogError("ChoiceButton prefab could not be instantiated.");
                continue;
            }

            TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
            if (choiceText == null) {
                Debug.LogError("TextMeshProUGUI component not found on choiceButton.");
                continue;
            }

            choiceText.text = choice.choiceText;

            Button button = choiceButton.GetComponent<Button>();
            if (button == null) {
                Debug.LogError("Button component not found on choiceButton.");
                continue;
            }

            button.onClick.AddListener(() => OnChoiceSelected(dialogueBox, choice));

            Debug.Log("ChoiceButton position: " + choiceButton.transform.position);
            Debug.Log("ChoiceButton size: " + choiceButton.GetComponent<RectTransform>().sizeDelta);

            choiceButton.SetActive(true);
            Debug.Log("ChoiceButton active: " + choiceButton.activeSelf);
        }
    }

    private void OnChoiceSelected(GameObject dialogueBox, DialogueChoice choice) {
        Debug.Log("Choice selected: " + choice.choiceText);

        // Call the method to handle choice selection
        dialogueBox.GetComponent<Dialogue>().SelectChoice(choice);
    }

    private TypewriterEffect GetTypewriterEffect(GameObject dialogueBox) {
        return dialogueBox.GetComponentInChildren<TypewriterEffect>();
    }
    private IEnumerator StartTypingWithDelay(TypewriterEffect typewriterEffect, string text) {
        yield return new WaitForEndOfFrame(); // Small delay to ensure the game object is active
        typewriterEffect.StartTyping(text);
    }
}