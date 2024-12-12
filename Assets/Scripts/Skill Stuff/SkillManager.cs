using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  // Add this line to use LINQ methods like Select
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;
using static ProjectileSkillData;
using System.Threading;

public class SkillManager : MonoBehaviour {

    public Transform castPoint;
    private ManaManager manaManager;

    [SerializeField] private StandardSkillCombosSO standardSkillCombinations;  // Reference to the standard spell combinations
    [SerializeField] private List<SkillCombinations> customSkillCombinations;
    [SerializeField] private List<ElementEffectMapping> elementEffects;
    [SerializeField] private List<SchoolEffectMapping> schoolEffects;
    public Dictionary<(Element, School), Skill> skillInstances;
    public Skill currentSkillInstance;

    public enum Element { Arcane, Fire, Ice, Thunder, Earth, Water }
    public enum School { Projectile, Barrier }


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
        FindSkillComponents();
        PopulateCustomCombinations();
        InitializeSkillMap();
        EnsureValidCurrentSelections();  // Ensure currentElement and currentSchool are valid to start
        UpdateCurrentSkill();
    }
    private void FindSkillComponents() {
        castPoint = transform.Find("CastPoint");
        if (castPoint == null) {
            Debug.LogError("SkillManager error: No child GameObject named 'Cast Point' found. Please ensure there is a GameObject named 'Cast Point' as a child of this component.");
        }
        TryGetComponent<ManaManager>(out manaManager);
        if (manaManager == null) {
            Debug.LogError("SkillManager error: No ManaManager component found on this GameObject or any of its parents.");
        }
    }

    #region Skill Switching for Player
    private void PopulateCustomCombinations() {
        // Create a dictionary for quick lookup of existing custom combinations
        var customCombinationLookup = customSkillCombinations.ToDictionary(combo => (combo.element, combo.school));

        // Iterate through the standard combinations
        foreach (var standardCombo in standardSkillCombinations.skillCombinations) {
            var key = (standardCombo.element, standardCombo.school);

            // If the custom combinations do not already contain this standard combination, add it
            if (!customCombinationLookup.ContainsKey(key)) {
                // Create a new instance of SpellCombinations to avoid referencing the same object
                var newCombo = new SkillCombinations(standardCombo.element, standardCombo.school, standardCombo.skill);
                customSkillCombinations.Add(newCombo);
            }
        }
    }
    private void InitializeSkillMap() {

        //Cleanup for old spells, mostly for testing in case changed in inspector
        Skill[] existingSkills = castPoint.GetComponents<Skill>();
        foreach (Skill skill in existingSkills) {
            skillInstances.Clear();
            Destroy(skill);
        }
        skillInstances = new Dictionary<(Element, School), Skill>();

        foreach (var combo in customSkillCombinations) {
            Skill skillInstance = Skill.CreateSkill(combo.skill, castPoint, manaManager);
            skillInstances[(combo.element, combo.school)] = skillInstance;
        }
    }
    private void EnsureValidCurrentSelections() {

        if (availableElements.Count == 0) {
            currentElement = default; // Handle empty list scenario
        }
        else if (!availableElements.Contains(currentElement)) {
            currentElement = availableElements.First();
        }

        if (availableSchools.Count == 0) {
            currentSchool = default; // Handle empty list scenario
        }
        else if (!availableSchools.Contains(currentSchool)) {
            currentSchool = availableSchools.First();
        }
    }
    public void HandleElementSelect() {
        PopulateCustomCombinations(); //called again in case changed in inspector
        InitializeSkillMap(); //called again in case changed in inspector

        CycleElements();
        UpdateCurrentSkill();
    }
    public void HandleSchoolSelect() {
        PopulateCustomCombinations(); //called again in case changed in inspector
        InitializeSkillMap(); //called again in case changed in inspector

        CycleSchools();
        UpdateCurrentSkill();
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
            if (customSkillCombinations.Any(sc => sc.element == newElement)) {
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
            if (customSkillCombinations.Any(sc => sc.school == newSchool)) {
                if (currentSchool != newSchool) {
                    currentSchool = newSchool;
                    PlaySchoolEffect(currentSchool);
                }
                return;
            }
        }
        currentSchool = originalSchool;  // Reset to original if no valid combination is found
    }
    private void UpdateCurrentSkill() {
        if (availableElements.Count == 0 || availableSchools.Count == 0) {
            currentSkillInstance = null;
            return;
        }
        if (skillInstances.TryGetValue((currentElement, currentSchool), out Skill skill)) {
            currentSkillInstance = skill;
        }
        else {
            Debug.Log("No skill assigned for this element/form combination!");
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
            UpdateCurrentSkill();
        }
    }
    public void AddAvailableSchool(School newSchoolChoice) {
        if (!availableSchools.Contains(newSchoolChoice)) {
            availableSchools.Add(newSchoolChoice);
            currentSchool = newSchoolChoice;
            UpdateCurrentSkill();
        }
    }
    #endregion

    #region Casting Methods
    public void CastPressed() {
        currentSkillInstance?.CastPressed();
    }
    public void CastHeld() {
        currentSkillInstance?.CastHeld();
    }
    public void CastReleased() {
        currentSkillInstance?.CastReleased();
    }
    #endregion
}