using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeManager : MonoBehaviour
{
    #region Melee Variables
    public MeleeAttackData[] meleeCombo;
    public Transform attackPoint;
    private int comboCount = 0;
    private float comboTimer = 0f;
    private GameObject activeMeleeAttack;
    private MeleeAttackData currentAttackData;
    private float attackProgress = 0f;

    private float attackTimer = 0f;
    private float cooldownTimer = 0f;

    private bool isCurrentAttackInCooldown = false;
    private bool isComboInCooldown = false;
    #endregion

    private void Update() {
        UpdateComboTimer();
        UpdateMeleeAttack();
    }
    public void TryMelee() {
        if (!isComboInCooldown) {
            if (comboCount < meleeCombo.Length && isCurrentAttackInCooldown) {
                comboCount++;
                StartMeleeAttack(comboCount);
            }
            else if (!isCurrentAttackInCooldown) {
                comboCount = 1;
                StartMeleeAttack(comboCount);
            }
        }
    }

    private void StartMeleeAttack(int comboStep) {
        currentAttackData = meleeCombo[comboStep - 1];
        attackTimer = currentAttackData.startUpTime + currentAttackData.hitDuration;
        attackProgress = 0f;
        isCurrentAttackInCooldown = false;

        // Destroy previous active melee attack if any
        if (activeMeleeAttack != null) {
            Destroy(activeMeleeAttack);
        }

        // Instantiate the new melee attack prefab
        activeMeleeAttack = Instantiate(currentAttackData.meleePrefab, attackPoint.position, attackPoint.rotation, attackPoint);

        // Initialize position and rotation
        activeMeleeAttack.transform.localPosition = currentAttackData.swingStartPoint;
        activeMeleeAttack.transform.localRotation = Quaternion.Euler(currentAttackData.swingStartRotation);

        if (attackPoint.parent != null) {
            if (attackPoint.parent.CompareTag("Player")) {
                activeMeleeAttack.layer = LayerMask.NameToLayer("Player Melee");
                Debug.Log("Assigned to Player Projectiles layer.");
            }
            else if (attackPoint.parent.CompareTag("Enemy")) {
                activeMeleeAttack.layer = LayerMask.NameToLayer("Enemy Melee");
                Debug.Log("Assigned to Enemy Projectiles layer.");
            }
            else {
                Debug.LogError("Origin parent is neither Player nor Enemy. Using default layer.");
                activeMeleeAttack.layer = LayerMask.NameToLayer("Default");
            }
        }

        var behaviour = activeMeleeAttack.AddComponent<MeleeBehaviour>();
        behaviour.attackData = currentAttackData;
        behaviour.SetOriginLayer(attackPoint.parent.gameObject.layer);
    }

    void UpdateMeleeAttack() {
        if (activeMeleeAttack != null) {
            attackProgress += Time.deltaTime;

            // Calculate relative start and end positions, considering the player's facing direction
            Vector3 startPos = currentAttackData.swingStartPoint;
            Vector3 endPos = currentAttackData.swingEndPoint;
            Vector3 swingStartRotation = currentAttackData.swingStartRotation;
            Vector3 swingEndRotation = currentAttackData.swingEndRotation;

            if (attackProgress < currentAttackData.startUpTime) {
                // Stay at the start position during startup time
                activeMeleeAttack.transform.localPosition = startPos;
                activeMeleeAttack.transform.localRotation = Quaternion.Euler(currentAttackData.swingStartRotation);
            }
            else if (attackProgress < currentAttackData.startUpTime + currentAttackData.hitDuration) {
                // Swing the attack
                float swingProgress = (attackProgress - currentAttackData.startUpTime) / currentAttackData.hitDuration;

                Vector3 parabolicPos = CalculateParabolicPath(swingProgress, startPos, endPos, currentAttackData.arcHeight);
                activeMeleeAttack.transform.localPosition = parabolicPos;

                // Smoothly interpolate rotation
                float easeProgress = EaseInOutSine(swingProgress);
                activeMeleeAttack.transform.localRotation = Quaternion.Lerp(
                    Quaternion.Euler(swingStartRotation),
                    Quaternion.Euler(swingEndRotation),
                    easeProgress
                );
            }
            else {
                // Attack has reached the end position
                activeMeleeAttack.transform.localPosition = endPos;
                activeMeleeAttack.transform.localRotation = Quaternion.Euler(currentAttackData.swingEndRotation);

                // Start the cooldown timer if it's not already started
                if (!isCurrentAttackInCooldown) {
                    isCurrentAttackInCooldown = true;
                    cooldownTimer = currentAttackData.coolDown;
                }

                // Destroy the active melee attack after the cooldown timer
                if (cooldownTimer <= 0) {
                    Destroy(activeMeleeAttack);
                    activeMeleeAttack = null;

                    // If it's the last attack in the combo, start the combo cooldown
                    if (comboCount >= meleeCombo.Length) {
                        isComboInCooldown = true;
                    }
                }
            }
        }
    }

    void UpdateComboTimer() {
        if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
        }
        else if (isCurrentAttackInCooldown) {
            // Handle the attack cooldown
            if (cooldownTimer > 0) {
                cooldownTimer -= Time.deltaTime;
            }
            else {
                // Attack cooldown finished, reset state
                isCurrentAttackInCooldown = false;

                // If it was the last attack in the combo, handle the combo cooldown
                if (isComboInCooldown) {
                    comboCount = 0;
                    isComboInCooldown = false;
                }
            }
        }
        else {
            comboCount = 0;
        }
    }

    Vector3 CalculateParabolicPath(float t, Vector3 startPos, Vector3 endPos, float arcHeight) {
        // Calculate the midpoint and the direction from start to end
        Vector3 direction = endPos - startPos;
        Vector3 midPoint = startPos + direction * 0.5f;

        // Calculate a vector perpendicular to the direction
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

        // Adjust the control point by adding the arcHeight along the perpendicular direction
        Vector3 controlPoint = midPoint + perpendicular * arcHeight;

        // Quadratic Bezier formula
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 parabolicPos = (uu * startPos) + (2 * u * t * controlPoint) + (tt * endPos);
        return parabolicPos;
    }

    float EaseInOutSine(float t) {
        return 0.5f * (1 - Mathf.Cos(Mathf.PI * t));
    }
}
