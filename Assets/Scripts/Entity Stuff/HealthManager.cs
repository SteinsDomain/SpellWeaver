using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public StatsSO stats;
    [Min(0)] public int currentHP;
    private float healthRegenTimer = 0.0f;

    public delegate void HealthDepletedAction();
    public event HealthDepletedAction OnHealthDepleted;


    void Awake() {
        if (stats != null) {
            currentHP = stats.maxHP;
        }
    }
    void Update() {
        if (stats != null) {
            RegenerateHealthOverTime();
        }
    }
    public void TakeDamage(int damage) {
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);
        if (currentHP <= 0) {
            currentHP = 0;
            OnHealthDepleted?.Invoke();
            Die();
        }
    }
    public void RegenerateHealthOverTime() {
        healthRegenTimer += Time.deltaTime;
        if (healthRegenTimer >= 1.0f) { //Only add every 1 second
            int healthToAdd = stats.hpRegenRate;
            currentHP += healthToAdd;
            currentHP = Mathf.Min(currentHP, stats.maxHP);
            healthRegenTimer -= 1.0f;
        }
    }
    public void RegenerateHealth(int amount) {
        currentHP += amount;
        currentHP = Mathf.Min(currentHP, stats.maxHP);
    }
    private void Die() {
        Destroy(gameObject);
    }
}
