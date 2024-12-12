using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  // Add this line to use LINQ methods like Select
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;
using static ProjectileSkillData;
using System.Threading;

public class SimpleSkillList : MonoBehaviour {


    public Transform castPoint;
    private ManaManager manaManager;
    public SkillData currentSkillData; // Directly assignable spell data in the inspector
    public Skill currentSkillInstance;

    void Awake() {
        FindSkillComponents();
        InitializeCurrentSkill();
    }

    private void FindSkillComponents() {
        castPoint = transform.Find("CastPoint");
        if (castPoint == null) {
            Debug.LogError("SkillManager error: No child GameObject named 'CastPoint' found. Please ensure there is a GameObject named 'CastPoint' as a child of this component.");
        }
        TryGetComponent<ManaManager>(out manaManager);
        if (manaManager == null) {
            Debug.LogError("SkillManager error: No ManaManager component found on this GameObject or any of its parents.");
        }
    }

    private void InitializeCurrentSkill() {
        if (currentSkillData == null) {
            Debug.LogError("SkillManager error: No SkillData assigned. Please assign a SkillData in the inspector.");
            return;
        }

        if (currentSkillData !=null) {
            currentSkillInstance = Skill.CreateSkill(currentSkillData, castPoint, manaManager);
        }
    }

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