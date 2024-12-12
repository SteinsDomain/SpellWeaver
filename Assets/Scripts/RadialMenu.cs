using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SkillManager;

public class RadialMenu : MonoBehaviour
{
    private int selectedIndex = 0;
    private List<RadialMenuItem> menuItems = new List<RadialMenuItem>();

    public void Initialize(IEnumerable<RadialMenuItem> items) {
        menuItems.Clear();
        foreach (var item in items) {
            RadialMenuItem instantiatedItem = Instantiate(item, transform);
            menuItems.Add(instantiatedItem);
            instantiatedItem.gameObject.SetActive(true);
        }
        UpdateSelection(Vector2.right);  // Default to first item selected
    }

    public void UpdateSelection(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        selectedIndex = CalculateSelectionIndex(angle);
        HighlightSelectedIndex(selectedIndex);
    }

    private int CalculateSelectionIndex(float angle) {
        float sliceAngle = 360 / menuItems.Count;
        return Mathf.FloorToInt(angle / sliceAngle);
    }

    private void HighlightSelectedIndex(int index) {
        for (int i = 0; i < menuItems.Count; i++) {
            menuItems[i].SetHighlighted(i == index);
        }
    }

    public Element SelectElementOption() {
        return menuItems[selectedIndex].AssociatedElement;
    }

    public School SelectSchoolOption() {
        return menuItems[selectedIndex].AssociatedSchool;
    }
}
