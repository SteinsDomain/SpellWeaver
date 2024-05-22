using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour {
    public TextMeshProUGUI textComponent; // Ensure this is assigned in the Inspector
    public float typeSpeed = 0.05f;

    private Coroutine typingCoroutine;
    private string currentText;
    private bool isTyping = false;

    public void StartTyping(string text) {
        currentText = text;
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    public void StopTypingAndShowFullText() {
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        textComponent.text = currentText;
        isTyping = false;
    }

    private IEnumerator TypeText(string text) {
        textComponent.text = "";
        isTyping = true;
        foreach (char letter in text.ToCharArray()) {
            textComponent.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    public bool IsTyping() {
        return isTyping;
    }
}