using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class BarrierBehaviour : MonoBehaviour {

    public BarrierSpellData spellData;
    private float lifetime;
    private System.Action onDestroy;
    private HealthManager healthManager;
    private ManaManager manaManager;

    public void Initialize(float duration, System.Action callback, bool canBreak, int barrierHealth) {
        lifetime = duration;
        onDestroy = callback;

        if (canBreak) {
            healthManager = gameObject.AddComponent<HealthManager>();
            healthManager.currentHP = barrierHealth;
            healthManager.OnHealthDepleted += HandleBarrierDestroyed;
        }
        StartCoroutine(Countdown());
    }

    public void InitiateDestruction() {
        StartCoroutine(DelayedDestroyCountdown(0)); // Immediately trigger countdown to destruction
    }

    private void HandleBarrierDestroyed() {
        onDestroy?.Invoke();
    }

    private IEnumerator Countdown() {
        yield return new WaitForSeconds(lifetime);

        if (healthManager == null || healthManager.currentHP >= 0) {
            onDestroy?.Invoke();
        }
    }

    private IEnumerator DelayedDestroyCountdown(float delay) {
        yield return new WaitForSeconds(delay);
        onDestroy?.Invoke();
    }
}