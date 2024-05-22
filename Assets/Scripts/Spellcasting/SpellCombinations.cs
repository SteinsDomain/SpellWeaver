using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellCombinations {
    public SpellManager.Element element;
    public SpellManager.School school;
    public SpellData spell;

    public SpellCombinations(SpellManager.Element element, SpellManager.School school, SpellData spell) {
        this.element = element;
        this.school = school;
        this.spell = spell;
    }
}



[System.Serializable]
public class ElementEffectMapping {
    public SpellManager.Element element;
    public GameObject effectPrefab;
}



[System.Serializable]
public class SchoolEffectMapping {
    public SpellManager.School school;
    public GameObject effectPrefab;
}
