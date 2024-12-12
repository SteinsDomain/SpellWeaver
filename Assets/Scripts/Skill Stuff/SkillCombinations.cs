using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillCombinations {
    public SkillManager.Element element;
    public SkillManager.School school;
    public SkillData skill;

    public SkillCombinations(SkillManager.Element element, SkillManager.School school, SkillData spell) {
        this.element = element;
        this.school = school;
        this.skill = spell;
    }
}



[System.Serializable]
public class ElementEffectMapping {
    public SkillManager.Element element;
    public GameObject effectPrefab;
}



[System.Serializable]
public class SchoolEffectMapping {
    public SkillManager.School school;
    public GameObject effectPrefab;
}
