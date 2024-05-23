using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StandardSpellCombinations", menuName = "Scriptable Objects/Spells/Standard Spell Combinations")]
public class StandardSpellCombosSO : ScriptableObject {
    public List<SpellCombinations> spellCombinations;
}
