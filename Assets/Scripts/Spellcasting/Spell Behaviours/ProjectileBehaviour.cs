using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ProjectileBehaviour : MonoBehaviour {

    public ProjectileSpellData spellData;
    private Vector3 startPosition;
    private int originLayer;
    private CollisionManager collisionManager;
    public Transform castPoint;
    private float travelDistance => Vector3.Distance(startPosition, transform.position);

    private void Start() {
        startPosition = transform.position;
        collisionManager = gameObject.AddComponent<CollisionManager>();
        if (collisionManager == null) {
            Debug.LogError("ProjectileBehaviour: Failed to add a CollisionManager to " + gameObject.name);
        }
        Debug.Log("ProjectileBehaviour: Projectile started at position " + startPosition.ToString());
    }

    private void Update() {
        HandleMovement();
        CheckOutOfRange();
    }

    private void HandleMovement() {
        float moveSpeed = Time.deltaTime * spellData.projectileSpeed * (transform.localScale.x > 0 ? 1 : -1);
        float adjustedHorizontalSpeed = collisionManager?.CheckForHorizontalCollision(moveSpeed, transform) ?? moveSpeed;
        transform.Translate(new Vector3(adjustedHorizontalSpeed, 0, 0), Space.Self);
    }
    private void CheckOutOfRange() {
        if (travelDistance > spellData.maxProjectileRange) {
            Debug.Log("ProjectileBehaviour: Projectile exceeded max range. Destroying.");
            Destroy(gameObject);
        }
    }
    public void InitiateExplosionCountdown(float delay) {
        StartCoroutine(ExplosionCountdown(delay));
        Debug.Log("ProjectileBehaviour: Explosion countdown started.");
    }
    private IEnumerator ExplosionCountdown(float delay) {
        yield return new WaitForSeconds(delay);
        Debug.Log("ProjectileBehaviour: Countdown completed. Exploding now.");
        Explode();
    }
    public void Explode() {
        Debug.Log("ProjectileBehaviour: Explosion triggered.");
        DoAreaDamage();
        if (spellData.explosionPrefab != null) {
            Instantiate(spellData.explosionPrefab, transform.position, Quaternion.identity);
            Debug.Log("ProjectileBehaviour: Explosion effect instantiated.");
        }
        Destroy(gameObject);
    }
    private void DoAreaDamage() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spellData.explosionSize);
        Debug.Log($"ProjectileBehaviour: Checking area damage for {hits.Length} potential targets.");
        foreach (var hit in hits) {
            if (ShouldAffectTarget(hit.gameObject)) {
                Debug.Log($"ProjectileBehaviour: Target {hit.gameObject.name} affected by explosion.");
                HealthManager healthManager = hit.GetComponent<HealthManager>();
                if (healthManager != null) {
                    healthManager.TakeDamage(spellData.explosionDamage);
                    Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;
                    ApplyExplosionKnockback(hit.transform, spellData.explosionKnockbackForce, knockbackDirection);
                    Debug.Log($"ProjectileBehaviour: Applied {spellData.explosionDamage} damage and knockback to {hit.gameObject.name}.");
                }
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision) { //Change to OnTriggerEnter to get rid of Rigidbodies Completely?

        Debug.Log($"ProjectileBehaviour: Collision detected with {collision.gameObject.name}.");

        if (spellData.isExplosive) {
            Explode();
            return;
        }

        if (ShouldAffectTarget(collision.gameObject)) {
            Debug.Log($"ProjectileBehaviour: Processing impact with {collision.gameObject.name}.");
            HealthManager healthManager = collision.gameObject.GetComponent<HealthManager>();
            if (healthManager != null) {
                healthManager.TakeDamage(spellData.projectileDamage);
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                ApplyProjectileKnockback(collision.transform, spellData.projectileKnockback, knockbackDirection);
                Debug.Log($"ProjectileBehaviour: Applied {spellData.projectileDamage} damage and knockback to {collision.gameObject.name}.");
            }

            if (spellData.hitEffect != null) {
                Instantiate(spellData.hitEffect, collision.contacts[0].point, Quaternion.identity);
                Debug.Log("ProjectileBehaviour: Hit effect instantiated.");
            }

            Destroy(gameObject);
            Debug.Log("ProjectileBehaviour: Projectile destroyed after impact.");
        }
    }
    public void SetOriginLayer(int layer) {
        originLayer = layer;
    }
    private bool ShouldAffectTarget(GameObject target) {
        return target.layer != originLayer;
    }
    private void ApplyProjectileKnockback(Transform hit, float projectileKnockback, Vector2 knockbackDirection) {
        Vector3 direction = new Vector3(knockbackDirection.x, knockbackDirection.y, 0).normalized;
        hit.position += direction * projectileKnockback * Time.deltaTime;
    }
    private void ApplyExplosionKnockback(Transform hit, float explosionKnockbackForce, Vector2 knockbackDirection) {
        Vector3 direction = new Vector3(knockbackDirection.x, knockbackDirection.y, 0).normalized;
        hit.position += direction * explosionKnockbackForce * Time.deltaTime;
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        // Draw a wire sphere at the transform's position with radius equal to the explosionSize
        Gizmos.DrawWireSphere(transform.position, spellData.explosionSize);
    }
}
