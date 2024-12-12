using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierSkill : Skill {

    private static GameObject currentBarrier;
    private bool isRepressed = false;

    public override void CastPressed() {
        var barrierSkill = skillData as BarrierSkillData;
        if (barrierSkill == null) return;

        // Prevent casting if a non-hold barrier is already active
        if (!CanCast(barrierSkill)) {
            Debug.Log("Attempted to cast a non-hold barrier when one is already active.");
            return;
        }

        SetupBarrier(barrierSkill);
    }

    public override void CastHeld() {
        var barrierSkill = skillData as BarrierSkillData;
        if (barrierSkill == null  || !barrierSkill.requiresHold) return;

        if (currentBarrier == null) {
            SetupBarrier(barrierSkill);
        }
    }

    public override void CastReleased() {
        var barrierSkill = skillData as BarrierSkillData;
        if (barrierSkill == null || !barrierSkill.requiresHold) return;

        DestroyCurrentBarrier();
    }

    private void SetupBarrier(BarrierSkillData barrierSkill) {
        if (TryToCast()) {
            CreateBarrier(barrierSkill);
            if (barrierSkill.drainsMana) {
                StartManaDrain();
            }
        }
    }

    private void CreateBarrier(BarrierSkillData barrierSkill) {

        if (currentBarrier != null && !barrierSkill.isStationary) {
            Debug.Log("Non-stationary barrier already active, cannot create another.");
            return;
        }

        Transform barrierPlacement = barrierSkill.centersOnCaster ? manaManager.transform : castPoint;
        currentBarrier = Instantiate(barrierSkill.barrierPrefab, barrierPlacement.position, Quaternion.identity);

        SetupBarrierTransform(currentBarrier.transform, barrierSkill);
        SetupBarrierProperties(currentBarrier, barrierSkill);
    }

    private Transform DetermineBarrierPlacement(BarrierSkillData barrierSkill) {
        return barrierSkill.centersOnCaster ? manaManager.transform : castPoint;
    }

    private void SetupBarrierProperties(GameObject barrier, BarrierSkillData barrierSkill) {
        BarrierBehaviour behaviour = barrier.AddComponent<BarrierBehaviour>();
        behaviour.Initialize(
            barrierSkill.barrierDuration,
            () => BarrierDestroyed(barrier),
            barrierSkill.isBreakable,
            barrierSkill.barrierHealth);
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

    private void SetupBarrierTransform(Transform barrierTransform, BarrierSkillData barrierSkill) {
        float casterFacingDirection = castPoint.parent.localScale.x; // Positive for facing right, negative for left
        barrierTransform.localScale = new Vector3(Mathf.Sign(casterFacingDirection), 1, 1);

        if (!barrierSkill.isStationary) {
            barrierTransform.SetParent(DetermineBarrierPlacement(barrierSkill));
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

        if (skillData.requiresConcentration) {
            manaManager.GetComponent<SkillManager>().IsConcentrating = false;
        }

        Destroy(barrier);
    }

    private bool CanCast(BarrierSkillData barrierSkill) {
        // Prevent casting if a non-hold barrier is active and requires exclusive control
        return barrierSkill.requiresHold || currentBarrier == null;
    }
}