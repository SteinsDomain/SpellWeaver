using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBehaviour : MonoBehaviour
{
    public MeleeAttackData attackData;
    private int originLayer;
    private PolygonCollider2D polygonCollider;

    private void Awake() {
        // Get the PolygonCollider2D component
        polygonCollider = GetComponent<PolygonCollider2D>();

        if (polygonCollider == null) {
            Debug.LogError("PolygonCollider2D component not found on the melee attack prefab.");
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"MeleeBehaviour: Collision detected with {collision.gameObject.name}.");

        if (ShouldAffectTarget(collision.gameObject))
        {
            Debug.Log($"MeleeBehaviour: Processing hit with {collision.gameObject.name}.");
            HealthManager healthManager = collision.gameObject.GetComponent<HealthManager>();
            if (healthManager != null)
            {
                healthManager.TakeDamage(attackData.damage);
                Debug.Log($"MeleeBehaviour: Applied {attackData.damage} damage to {collision.gameObject.name}.");
                if (polygonCollider != null) {
                    polygonCollider.enabled = false;
                }
            }

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null) {
                enemy.TakeKnockback(attackData.knockbackForce, attackData.knockbackDirection);
            }

        }

    }
    public void SetOriginLayer(int layer)
    {
        originLayer = layer;
    }
    private bool ShouldAffectTarget(GameObject target)
    {
        return target.layer != originLayer;
    }
}
