using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Scriptable Objects/Dialogue")]
public class DialogueData : ScriptableObject {
    [TextArea(3, 10)]
    public List<string> dialogueLines;
    public List<DialogueChoice> choices;
}

[System.Serializable]
public class DialogueChoice {
    public string choiceText;
    public DialogueData nextDialogue;
}