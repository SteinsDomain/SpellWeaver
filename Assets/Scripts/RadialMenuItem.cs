using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenuItem : MonoBehaviour
{
    public Image icon; // Assign this via the Unity Editor if using UI Image to represent the item.
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;

    // Properties to associate with specific elements or schools
    public SkillManager.Element AssociatedElement { get; set; }
    public SkillManager.School AssociatedSchool { get; set; }

    // Method to set this item as highlighted.
    public void SetHighlighted(bool highlighted) {
        if (icon != null) {
            icon.color = highlighted ? highlightColor : defaultColor;
        }
    }
}
