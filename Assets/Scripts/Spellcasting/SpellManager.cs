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
    public RadialMenu schoolMenu;
    public RadialMenu elementMenu;

    [SerializeField] private List<SpellCombinations> spellCombinations;
    [SerializeField] private List<ElementEffectMapping> elementEffects;
    [SerializeField] private List<SchoolEffectMapping> schoolEffects;
    private Dictionary<(Element, School), Spell> spellInstances;
    public Spell currentSpellInstance;

    public enum Element { Arcane, Fire, Ice, Thunder, Earth }
    public enum School { Evoke, Conjure }
    public Element currentElement = Element.Arcane;
    public School currentSchool = School.Evoke;

    private bool isConcentrating;
    public bool IsConcentrating {
        get { return isConcentrating; }
        set { isConcentrating = value; } 
    }
    
    void Awake() {
        FindSpellcastingComponents();
        InitializeSpellMap();
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
    private void InitializeSpellMap() {
        spellInstances = new Dictionary<(Element, School), Spell>();

        foreach (var combo in spellCombinations) {

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
    public void HandleElementSelect() {
        CycleElements();
        UpdateCurrentSpell();
        InitializeSpellMap();
    }
    public void HandleSchoolSelect() {
        CycleSchools();
        UpdateCurrentSpell();
        InitializeSpellMap();
    }
    private void CycleElements() {
        Element originalElement = currentElement;
        bool validElementFound = false;
        int attempts = 0;
        do {
            currentElement = (Element)(((int)currentElement + 1) % Enum.GetNames(typeof(Element)).Length);
            validElementFound = spellCombinations.Any(sc => sc.element == currentElement);
            if (validElementFound) {
                if (currentElement != originalElement) {
                    PlayElementEffect(currentElement);
                }
                break;
            }
            attempts++;
        }
        while (currentElement != originalElement && attempts < Enum.GetNames(typeof(Element)).Length);
        if (!validElementFound) {
            currentElement = originalElement;  // Reset to original if no valid combination is found
        }
    }
    private void CycleSchools() {
        School originalSchool = currentSchool;
        bool validSchoolFound = false;
        int attempts = 0;
        do {
            currentSchool = (School)(((int)currentSchool + 1) % Enum.GetNames(typeof(School)).Length);
            validSchoolFound = spellCombinations.Any(sc => sc.school == currentSchool);
            if (validSchoolFound) {
                if (currentSchool != originalSchool) {
                    PlaySchoolEffect(currentSchool);
                }
                break;
            }
            attempts++;
        }
        while (currentSchool != originalSchool && attempts < Enum.GetNames(typeof(School)).Length);
        if (!validSchoolFound) {
            currentSchool = originalSchool;  // Reset to original if no valid combination is found
        }
    }
    private void UpdateCurrentSpell() {
        if (spellInstances.TryGetValue((currentElement, currentSchool), out Spell spell)) {
            currentSpellInstance = spell;
        }
        else {
            //currentSpell = currentSpell;
            Debug.LogError("No spell assigned for this element/form combination!");
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
    #region Unused Radial Menu Stuff
    private void OpenElementMenu() {
        var elementItems = spellCombinations.Select(sc => new RadialMenuItem { AssociatedElement = sc.element }).Distinct();
        elementMenu.Initialize(elementItems);
        elementMenu.gameObject.SetActive(true);
    }
    private void OpenSchoolMenu() {
        var schoolItems = spellCombinations.Select(sc => new RadialMenuItem { AssociatedSchool = sc.school }).Distinct();
        schoolMenu.Initialize(schoolItems);
        schoolMenu.gameObject.SetActive(true);
    }
    public void UpdateElementMenuDirection(Vector2 direction) {
        elementMenu.UpdateSelection(direction);
    }
    public void UpdateSchoolMenuDirection(Vector2 direction) {
        schoolMenu.UpdateSelection(direction);
    }
    public void HandleElementMenuClose() {
        Element selectedElement = elementMenu.SelectElementOption();
        if (selectedElement != currentElement) {
            currentElement = selectedElement;
            PlayElementEffect(currentElement);
            UpdateCurrentSpell();
            InitializeSpellMap();
            elementMenu.gameObject.SetActive(false);
        }
    }
    public void HandleSchoolMenuClose() {
        School selectedSchool = schoolMenu.SelectSchoolOption();
        if (selectedSchool != currentSchool) {
            currentSchool = selectedSchool;
            PlaySchoolEffect(currentSchool);
            UpdateCurrentSpell();
            InitializeSpellMap();
            schoolMenu.gameObject.SetActive(false);
        }
    } 
    #endregion
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