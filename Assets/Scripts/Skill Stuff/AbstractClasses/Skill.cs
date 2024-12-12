using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill : MonoBehaviour {
    protected Transform castPoint;
    protected ManaManager manaManager;
    protected SkillData skillData;
    protected float nextAllowedCastTime = 0f;
    private Coroutine manaDrainCoroutine;
    private bool isManaDraining;

    
    public virtual bool CanAim => skillData.canAim;

    public abstract void CastPressed();
    public abstract void CastHeld();
    public abstract void CastReleased();

    protected virtual bool TryToCast() {
        if (Time.time >= nextAllowedCastTime && manaManager.currentMP >= skillData.mpCost) {
            manaManager.UseMana(skillData.mpCost, skillData.mpRegenCooldown);
            float cooldownPeriod = 1f / Mathf.Max(skillData.castsPerSecond, 0.01f);
            nextAllowedCastTime = Time.time + cooldownPeriod;
            if (skillData.requiresConcentration) {
                manaManager.GetComponent<SkillManager>().IsConcentrating = true;
            }
            return true;
        }

        Debug.Log("Cannot cast spell due to insufficient mana or cooldown.");
        return false;
    }
    public virtual void ResetCooldown() {
        nextAllowedCastTime = Time.time;  // Allow immediate casting
    }
    protected void StartManaDrain() {
        if (!isManaDraining) {
            Debug.Log("Starting Mana Drain Coroutine.");
            isManaDraining = true;
            manaDrainCoroutine = manaManager.StartCoroutine(ManaDrain());
        }
        else {
            Debug.Log("Mana Drain Coroutine already running.");
        }
    }
    protected IEnumerator ManaDrain() {

        Debug.Log("Mana Drain Coroutine started.");

        while (isManaDraining) {
            if (manaManager.currentMP > skillData.mpCost) {
                manaManager.UseMana(skillData.mpCost, skillData.mpRegenCooldown);
                Debug.Log("Mana drained.");
                yield return new WaitForSeconds(1f / skillData.manaDrainRate);
            }
            else {
                Debug.Log("Not enough mana to maintain the barrier.");
                CastReleased();
                StopManaDrain();  // Stop mana drain as mana is insufficient
                break;
            }
        }

        Debug.Log("Mana Drain Coroutine finished.");

        if (!isManaDraining) {
            manaManager.StartCoroutine(manaManager.RegenerationDelay(skillData.mpRegenCooldown));
        }
    }
    protected void StopManaDrain() {
        if (isManaDraining && manaDrainCoroutine != null) {
            manaManager.StopCoroutine(manaDrainCoroutine);
            manaDrainCoroutine = null;
            isManaDraining = false;
            Debug.Log("Mana Drain Coroutine stopped.");
            manaManager.StartCoroutine(manaManager.RegenerationDelay(skillData.mpRegenCooldown));  // Ensure mana regeneration starts after drain stops
        }
    }

    public static Skill CreateSkill(SkillData spellData, Transform castPoint, ManaManager manaManager) {
        if (spellData is ProjectileSkillData) {
            return castPoint.gameObject.AddComponent<ProjectileSkill>().Initialize(castPoint, manaManager, spellData);
        }
        if (spellData is BarrierSkillData) {
            return castPoint.gameObject.AddComponent<BarrierSkill>().Initialize(castPoint, manaManager, spellData);
        }

        throw new ArgumentException("Unknown SpellData type");
    }

    public virtual Skill Initialize(Transform castPoint, ManaManager manaManager, SkillData spellData) {
        this.castPoint = castPoint;
        this.manaManager = manaManager;
        this.skillData = spellData;
        return this;
    }
}