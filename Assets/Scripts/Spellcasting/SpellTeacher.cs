using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTeacher : MonoBehaviour, IInteractable
{
    // Lists of elements and schools to add when interacted with
    [SerializeField] private List<SpellManager.Element> elementsToAdd;
    [SerializeField] private List<SpellManager.School> schoolsToAdd;

    public void Interact(GameObject interactor) {
        // Find the SpellManager component on the player
        SpellManager spellManager = interactor.GetComponent<SpellManager>();
        if (spellManager != null) {
            // Add each specified element to the SpellManager's available elements
            foreach (var element in elementsToAdd) {
                spellManager.AddAvailableElement(element);
            }

            // Add each specified school to the SpellManager's available schools
            foreach (var school in schoolsToAdd) {
                spellManager.AddAvailableSchool(school);
            }

            Debug.Log("Elements and schools added to available lists.");
        }
        else {
            Debug.LogError("No SpellManager found on the player.");
        }
    }
}
