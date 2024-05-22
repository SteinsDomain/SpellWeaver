using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ManaManager : MonoBehaviour
{
    public StatsSO stats;

    [Min(0)] public int currentMP;
    public UnityEvent<int> OnManaChanged;

    private Coroutine regenCoroutine;
    private bool isRegenerating = false;

    void Awake() {
        currentMP = stats.maxMP;
        OnManaChanged?.Invoke(currentMP);
        StartRegeneration();
    }

    public void UseMana(int amount, float mpRegenCooldown) {
        currentMP = Mathf.Max(0, currentMP - amount);
        OnManaChanged?.Invoke(currentMP);
        Debug.Log($"Player used {amount} mana, current MP: {currentMP}, Regen cooldown: {mpRegenCooldown} seconds.");

        PauseRegeneration();
        StartCoroutine(RegenerationDelay(mpRegenCooldown));
    }
    public void RegenerateMana(int amount) {
        currentMP = Mathf.Min(stats.maxMP, currentMP + amount);
        OnManaChanged?.Invoke(currentMP);
        Debug.Log("Player regenerated " + amount + " mana, current MP: " + currentMP);
    }
    private void StartRegeneration() {
        if (!isRegenerating) {
            isRegenerating = true;
            regenCoroutine = StartCoroutine(RegenerateManaOverTime());
        }
    }
    public IEnumerator RegenerateManaOverTime() {
        while (isRegenerating) {
            yield return new WaitForSeconds(stats.mpRegenRate);
            // Only regenerate if current MP is less than max MP
            if (currentMP < stats.maxMP) {
                RegenerateMana(stats.mpRegenAmount);
            }
            else {
                Debug.Log("Mana is full, stopping regeneration.");
                PauseRegeneration(); // Stop the regeneration coroutine
                break;
            }
        }
    }
    private void PauseRegeneration() {
        if (isRegenerating && regenCoroutine != null) {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
            isRegenerating = false;
        }
    }
    public IEnumerator RegenerationDelay(float delay) {
        Debug.Log($"Starting regeneration delay for {delay} seconds.");
        yield return new WaitForSeconds(delay);  
        StartRegeneration();
    }
}
