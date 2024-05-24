using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  // Add this line to use LINQ methods like Select
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;
using static ProjectileSpellData;
using System.Threading;

public class SpellManager : MonoBehaviour {

    public Transform castPoint;
    public ManaManager manaManager;

    [SerializeField] private StandardSpellCombosSO standardSpellCombinations;  // Reference to the standard spell combinations
    [SerializeField] private List<SpellCombinations> customSpellCombinations;
    [SerializeField] private List<ElementEffectMapping> elementEffects;
    [SerializeField] private List<SchoolEffectMapping> schoolEffects;
    public Dictionary<(Element, School), Spell> spellInstances;
    public Spell currentSpellInstance;

    public enum Element { Arcane, Fire, Ice, Thunder, Earth }
    public enum School { Evoke, Conjure }
    [SerializeField] private List<Element> availableElements;
    [SerializeField] private List<School> availableSchools;
    public Element currentElement;
    public School currentSchool;

    private bool isConcentrating;
    public bool IsConcentrating {
        get { return isConcentrating; }
        set { isConcentrating = value; } 
    }
    
    void Awake() {
        FindSpellcastingComponents();
        PopulateCustomCombinations();
        InitializeSpellMap();
        EnsureValidCurrentSelections();  // Ensure currentElement and currentSchool are valid to start
        UpdateCurrentSpell();
    }
    private void FindSpellcastingComponents() {
        castPoint = transform.Find("CastPoint");
        if (castPoint == null) {
            Debug.LogError("SpellManager error: No child GameObject named 'Cast Point' found. Please ensure there is a GameObject named 'Cast Point' as a child of this component.");
        }
        manaManager = GetComponentInParent<ManaManager>();
        if (manaManager == null) {
            Debug.LogError("SpellManager error: No ManaManager component found on this GameObject or any of its parents.");
        }
    }

    #region Spell Switching for Player

    private void PopulateCustomCombinations() {
        // Create a dictionary for quick lookup of existing custom combinations
        var customCombinationLookup = customSpellCombinations.ToDictionary(combo => (combo.element, combo.school));

        // Iterate through the standard combinations
        foreach (var standardCombo in standardSpellCombinations.spellCombinations) {
            var key = (standardCombo.element, standardCombo.school);

            // If the custom combinations do not already contain this standard combination, add it
            if (!customCombinationLookup.ContainsKey(key)) {
                customSpellCombinations.Add(standardCombo);
            }
        }
    }
    private void InitializeSpellMap() {

        spellInstances = new Dictionary<(Element, School), Spell>();

        foreach (var combo in customSpellCombinations) {
            Spell spellInstance;
            if (combo.spell is ProjectileSpellData) {
                spellInstance = new ProjectileSpell(castPoint, manaManager, combo.spell);
            }
            else if (combo.spell is BarrierSpellData) {
                spellInstance = new BarrierSpell(castPoint, manaManager, combo.spell);
            }
            else {
                continue;
            }
            spellInstances[(combo.element, combo.school)] = spellInstance;
        }


    }
    private void EnsureValidCurrentSelections() {

        if (availableElements.Count == 0) {
            currentElement = default(Element); // Handle empty list scenario
        }
        else if (!availableElements.Contains(currentElement)) {
            currentElement = availableElements.First();
        }

        if (availableSchools.Count == 0) {
            currentSchool = default(School); // Handle empty list scenario
        }
        else if (!availableSchools.Contains(currentSchool)) {
            currentSchool = availableSchools.First();
        }
    }
    public void HandleElementSelect() {
        CycleElements();
        UpdateCurrentSpell();
        //InitializeSpellMap();
    }
    public void HandleSchoolSelect() {
        CycleSchools();
        UpdateCurrentSpell();
        //InitializeSpellMap();
    }
    private void CycleElements() {
        if (availableElements.Count == 0) return;

        Element originalElement = currentElement;
        int originalIndex = availableElements.IndexOf(originalElement);


        // Ensure currentElement is valid
        if (originalIndex == -1) {
            currentElement = availableElements[0];
            originalIndex = 0;
        }


        for (int i = 1; i <= availableElements.Count; i++) {
            int newIndex = (originalIndex + i) % availableElements.Count;
            Element newElement = availableElements[newIndex];
            if (customSpellCombinations.Any(sc => sc.element == newElement)) {
                if (currentElement != newElement) {
                    currentElement = newElement;
                    PlayElementEffect(currentElement);
                }
                return;
            }
        }
        currentElement = originalElement;  // Reset to original if no valid combination is found
    }
    private void CycleSchools() {
        if (availableSchools.Count == 0) return;

        School originalSchool = currentSchool;
        int originalIndex = availableSchools.IndexOf(originalSchool);

        // Ensure currentSchool is valid
        if (originalIndex == -1) {
            currentSchool = availableSchools[0];
            originalIndex = 0;
        }

        for (int i = 1; i <= availableSchools.Count; i++) {
            int newIndex = (originalIndex + i) % availableSchools.Count;
            School newSchool = availableSchools[newIndex];
            if (customSpellCombinations.Any(sc => sc.school == newSchool)) {
                if (currentSchool != newSchool) {
                    currentSchool = newSchool;
                    PlaySchoolEffect(currentSchool);
                }
                return;
            }
        }
        currentSchool = originalSchool;  // Reset to original if no valid combination is found
    }
    private void UpdateCurrentSpell() {

        if (availableElements.Count == 0 || availableSchools.Count == 0) {
            currentSpellInstance = null;
            return;
        }

        if (spellInstances.TryGetValue((currentElement, currentSchool), out Spell spell)) {
            currentSpellInstance = spell;
        }
        else {
            Debug.Log("No spell assigned for this element/form combination!");
        }
    }
    private void PlayElementEffect(Element element) {
        var mapping = elementEffects.FirstOrDefault(e => e.element == element);
        if (mapping != null && mapping.effectPrefab != null) {
            ParticleSystem effect = Instantiate(mapping.effectPrefab, castPoint.position, Quaternion.identity, castPoint).GetComponent<ParticleSystem>();
            AdjustEffectRotation(effect.transform);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
    }
    private void PlaySchoolEffect(School school) {
        var mapping = schoolEffects.FirstOrDefault(f => f.school == school);
        if (mapping != null && mapping.effectPrefab != null) {
            ParticleSystem effect = Instantiate(mapping.effectPrefab, castPoint.position, Quaternion.identity, castPoint).GetComponent<ParticleSystem>();
            AdjustEffectRotation(effect.transform);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
    }
    private void AdjustEffectRotation(Transform effectTransform) {
        float casterFacingDirection = Mathf.Sign(castPoint.parent.localScale.x);
        effectTransform.localScale = new Vector3(casterFacingDirection, 1, 1);
    }

    #endregion

    #region Adding Elements and Schools
    public void AddAvailableElement(Element newElementChoice) {
        if (!availableElements.Contains(newElementChoice)) {
            availableElements.Add(newElementChoice);
            currentElement = newElementChoice;
            UpdateCurrentSpell();
        }
    }

    public void AddAvailableSchool(School newSchoolChoice) {
        if (!availableSchools.Contains(newSchoolChoice)) {
            availableSchools.Add(newSchoolChoice);
            currentSchool = newSchoolChoice;
            UpdateCurrentSpell();
        }
    }
    #endregion

    #region Casting Methods
    public void CastPressed() {
        currentSpellInstance?.CastPressed();
    }
    public void CastHeld() {
        currentSpellInstance?.CastHeld();
    }
    public void CastReleased() {
        currentSpellInstance?.CastReleased();
    }
    #endregion
}