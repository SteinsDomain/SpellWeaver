using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StandardSkillCombinations", menuName = "Scriptable Objects/Skills/Standard Skill Combinations")]
public class StandardSkillCombosSO : ScriptableObject {
    public List<SkillCombinations> skillCombinations;
}
