using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  // Add this line to use LINQ methods like Select
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;
using static ProjectileSpellData;
using System.Threading;

public class SimpleSpellList : MonoBehaviour {


    public Transform castPoint;
    private ManaManager manaManager;
    public SpellData currentSpellData; // Directly assignable spell data in the inspector
    public Spell currentSpellInstance;

    void Awake() {
        FindSpellcastingComponents();
        InitializeCurrentSpell();
    }

    private void FindSpellcastingComponents() {
        castPoint = transform.Find("CastPoint");
        if (castPoint == null) {
            Debug.LogError("SpellManager error: No child GameObject named 'CastPoint' found. Please ensure there is a GameObject named 'CastPoint' as a child of this component.");
        }
        TryGetComponent<ManaManager>(out manaManager);
        if (manaManager == null) {
            Debug.LogError("SpellManager error: No ManaManager component found on this GameObject or any of its parents.");
        }
    }

    private void InitializeCurrentSpell() {
        if (currentSpellData == null) {
            Debug.LogError("SpellManager error: No SpellData assigned. Please assign a SpellData in the inspector.");
            return;
        }

        if (currentSpellData !=null) {
            currentSpellInstance = Spell.CreateSpell(currentSpellData, castPoint, manaManager);
        }
    }

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