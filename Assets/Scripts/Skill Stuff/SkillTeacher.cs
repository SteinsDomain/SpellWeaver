using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTeacher : MonoBehaviour, IInteractable
{
    // Lists of elements and schools to add when interacted with
    [SerializeField] private List<SkillManager.Element> elementsToAdd;
    [SerializeField] private List<SkillManager.School> schoolsToAdd;

    public void Interact(GameObject interactor) {
        // Find the SkillManager component on the player
        SkillManager skillManager = interactor.GetComponent<SkillManager>();
        if (skillManager != null) {
            // Add each specified element to the SpellManager's available elements
            foreach (var element in elementsToAdd) {
                skillManager.AddAvailableElement(element);
            }

            // Add each specified school to the SpellManager's available schools
            foreach (var school in schoolsToAdd) {
                skillManager.AddAvailableSchool(school);
            }

            Debug.Log("Elements and schools added to available lists.");
            Destroy(gameObject);
        }
        else {
            Debug.LogError("No SkillManager found on the player.");
        }
    }
}
