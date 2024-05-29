using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierSpell : Spell {

    private static GameObject currentBarrier;
    private bool isRepressed = false;

    public override void CastPressed() {
        var barrierSpell = spellData as BarrierSpellData;
        if (barrierSpell == null) return;

        // Prevent casting if a non-hold barrier is already active
        if (!CanCast(barrierSpell)) {
            Debug.Log("Attempted to cast a non-hold barrier when one is already active.");
            return;
        }

        SetupBarrier(barrierSpell);
    }

    public override void CastHeld() {
        var barrierSpell = spellData as BarrierSpellData;
        if (barrierSpell == null  || !barrierSpell.requiresHold) return;

        if (currentBarrier == null) {
            SetupBarrier(barrierSpell);
        }
    }

    public override void CastReleased() {
        var barrierSpell = spellData as BarrierSpellData;
        if (barrierSpell == null || !barrierSpell.requiresHold) return;

        DestroyCurrentBarrier();
    }

    private void SetupBarrier(BarrierSpellData barrierSpell) {
        if (TryToCast()) {
            CreateBarrier(barrierSpell);
            if (barrierSpell.drainsMana) {
                StartManaDrain();
            }
        }
    }

    private void CreateBarrier(BarrierSpellData barrierSpell) {

        if (currentBarrier != null && !barrierSpell.isStationary) {
            Debug.Log("Non-stationary barrier already active, cannot create another.");
            return;
        }

        Transform barrierPlacement = barrierSpell.centersOnCaster ? manaManager.transform : castPoint;
        currentBarrier = Instantiate(barrierSpell.barrierPrefab, barrierPlacement.position, Quaternion.identity);

        SetupBarrierTransform(currentBarrier.transform, barrierSpell);
        SetupBarrierProperties(currentBarrier, barrierSpell);
    }

    private Transform DetermineBarrierPlacement(BarrierSpellData barrierSpell) {
        return barrierSpell.centersOnCaster ? manaManager.transform : castPoint;
    }

    private void SetupBarrierProperties(GameObject barrier, BarrierSpellData barrierSpell) {
        BarrierBehaviour behaviour = barrier.AddComponent<BarrierBehaviour>();
        behaviour.Initialize(
            barrierSpell.barrierDuration,
            () => BarrierDestroyed(barrier),
            barrierSpell.isBreakable,
            barrierSpell.barrierHealth);
    }

    private void AssignBarrierLayer(Transform barrierTransform) {
        if (castPoint.parent != null) {
            string layerName = castPoint.parent.CompareTag("Player") ? "Player Barriers" : "Enemy Barriers";
           barrierTransform.gameObject.layer = LayerMask.NameToLayer(layerName);
            
        }
        else {
            Debug.LogError("Barrier's origin parent is neither Player nor Enemy. Defaulting to a neutral layer.");
            barrierTransform.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    private void SetupBarrierTransform(Transform barrierTransform, BarrierSpellData barrierSpell) {
        float casterFacingDirection = castPoint.parent.localScale.x; // Positive for facing right, negative for left
        barrierTransform.localScale = new Vector3(Mathf.Sign(casterFacingDirection), 1, 1);

        if (!barrierSpell.isStationary) {
            barrierTransform.SetParent(DetermineBarrierPlacement(barrierSpell));
        }
        AssignBarrierLayer(barrierTransform);

    }

    private void DestroyCurrentBarrier() {
        if (currentBarrier == null) return;

        BarrierBehaviour behaviour = currentBarrier.GetComponent<BarrierBehaviour>();
        if (behaviour) {
            behaviour.InitiateDestruction(); // Assumes a method that handles the destruction process
        }
        StopManaDrain(); // Ensure mana drain is stopped when barrier is manually released
        currentBarrier = null;
    }

    private void BarrierDestroyed(GameObject barrier) {
        if (currentBarrier == barrier) {
            StopManaDrain();
            currentBarrier = null;
            Debug.Log("Barrier destroyed due to condition.");
        }

        if (spellData.requiresConcentration) {
            manaManager.GetComponent<SpellManager>().IsConcentrating = false;
        }

        Destroy(barrier);
    }

    private bool CanCast(BarrierSpellData barrierSpell) {
        // Prevent casting if a non-hold barrier is active and requires exclusive control
        return barrierSpell.requiresHold || currentBarrier == null;
    }
}