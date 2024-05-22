using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxResizer : MonoBehaviour {
    public RectTransform panelRectTransform;
    public TextMeshProUGUI textComponent;
    public Vector2 padding = new Vector2(20, 20);
    private Vector2 initialSize = new Vector2(100, 50); // Set initial size

    void Start() {
        // Set the initial size of the panel
        panelRectTransform.sizeDelta = initialSize + padding;
    }

    void Update() {
        ResizeToFitText();
    }

    void ResizeToFitText() {
        Vector2 textSize = new Vector2(textComponent.preferredWidth, textComponent.preferredHeight);
        Vector2 targetSize = textSize + padding;
        // Smoothly resize the panel
        panelRectTransform.sizeDelta = Vector2.Lerp(panelRectTransform.sizeDelta, targetSize, Time.deltaTime * 10);
    }
}