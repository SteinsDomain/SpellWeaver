using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spell : MonoBehaviour {
    protected Transform castPoint;
    protected ManaManager manaManager;
    protected SpellData spellData;
    protected float nextAllowedCastTime = 0f;
    private Coroutine manaDrainCoroutine;
    private bool isManaDraining;

    
    public virtual bool CanAim => spellData.canAim;

    public abstract void CastPressed();
    public abstract void CastHeld();
    public abstract void CastReleased();

    protected virtual bool TryToCast() {
        if (Time.time >= nextAllowedCastTime && manaManager.currentMP >= spellData.mpCost) {
            manaManager.UseMana(spellData.mpCost, spellData.mpRegenCooldown);
            float cooldownPeriod = 1f / Mathf.Max(spellData.castsPerSecond, 0.01f);
            nextAllowedCastTime = Time.time + cooldownPeriod;
            if (spellData.requiresConcentration) {
                manaManager.GetComponent<SpellManager>().IsConcentrating = true;
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
            if (manaManager.currentMP > spellData.mpCost) {
                manaManager.UseMana(spellData.mpCost, spellData.mpRegenCooldown);
                Debug.Log("Mana drained.");
                yield return new WaitForSeconds(1f / spellData.manaDrainRate);
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
            manaManager.StartCoroutine(manaManager.RegenerationDelay(spellData.mpRegenCooldown));
        }
    }
    protected void StopManaDrain() {
        if (isManaDraining && manaDrainCoroutine != null) {
            manaManager.StopCoroutine(manaDrainCoroutine);
            manaDrainCoroutine = null;
            isManaDraining = false;
            Debug.Log("Mana Drain Coroutine stopped.");
            manaManager.StartCoroutine(manaManager.RegenerationDelay(spellData.mpRegenCooldown));  // Ensure mana regeneration starts after drain stops
        }
    }

    public static Spell CreateSpell(SpellData spellData, Transform castPoint, ManaManager manaManager) {
        if (spellData is ProjectileSpellData) {
            return castPoint.gameObject.AddComponent<ProjectileSpell>().Initialize(castPoint, manaManager, spellData);
        }
        if (spellData is BarrierSpellData) {
            return castPoint.gameObject.AddComponent<BarrierSpell>().Initialize(castPoint, manaManager, spellData);
        }

        throw new ArgumentException("Unknown SpellData type");
    }

    public virtual Spell Initialize(Transform castPoint, ManaManager manaManager, SpellData spellData) {
        this.castPoint = castPoint;
        this.manaManager = manaManager;
        this.spellData = spellData;
        return this;
    }
}