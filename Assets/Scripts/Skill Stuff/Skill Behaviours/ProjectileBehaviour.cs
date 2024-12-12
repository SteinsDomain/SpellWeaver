using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ProjectileBehaviour : MonoBehaviour {

    public ProjectileSkillData skillData;
    private Vector3 startPosition;
    private int originLayer;
    private CollisionManager collisionManager;
   public Transform castPoint;
    private float TravelDistance => Vector3.Distance(startPosition, transform.position);

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
        float moveSpeed = skillData.projectileSpeed * Time.deltaTime * (transform.localScale.x > 0 ? 1 : -1);
        Vector2 moveDirection = transform.right * moveSpeed;
        HandleCollision(moveDirection);
    }

    private void HandleCollision(Vector2 moveDirection)
    {
        Vector2 currentPosition = transform.position;
        Vector2 nextPosition = currentPosition + moveDirection;

        RaycastHit2D hit = Physics2D.Raycast(currentPosition, moveDirection, moveDirection.magnitude, LayerMask.GetMask("Ground", "Enemy", "Player"));
        Debug.DrawRay(currentPosition, moveDirection, Color.red);

        if (hit.collider != null)
        {
            Debug.Log("ProjectileBehaviour: Collision detected with " + hit.collider.name);
            transform.position = hit.point;
            OnCollision(hit.collider);
        }
        else
        {
            transform.position = nextPosition;
        }
    }

    private void OnCollision(Collider2D collision)
    {
        if (skillData.isExplosive)
        {
            Explode();
            return;
        }

        if (skillData.hitEffect != null) {
            Instantiate(skillData.hitEffect, transform.position, Quaternion.identity);
            Debug.Log("ProjectileBehaviour: Hit effect instantiated.");
        }

        if (ShouldAffectTarget(collision.gameObject))
        {
            Debug.Log("ProjectileBehaviour: Processing impact with " + collision.gameObject.name);
            HealthManager healthManager = collision.gameObject.GetComponent<HealthManager>();
            if (healthManager != null)
            {
                healthManager.TakeDamage(skillData.projectileDamage, skillData.hitStunScale, skillData.hitStunDuration);
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                ApplyProjectileKnockback(collision.transform, skillData.projectileKnockback, knockbackDirection);
                Debug.Log("ProjectileBehaviour: Applied " + skillData.projectileDamage + " damage and knockback to " + collision.gameObject.name);
            }

            Destroy(gameObject);
            Debug.Log("ProjectileBehaviour: Projectile destroyed after impact.");
        }
    }

    private void CheckOutOfRange() {
        if (TravelDistance > skillData.maxProjectileRange) {
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
        if (skillData.explosionPrefab != null) {
            Instantiate(skillData.explosionPrefab, transform.position, Quaternion.identity);
            Debug.Log("ProjectileBehaviour: Explosion effect instantiated.");
        }
        Destroy(gameObject);
    }
    private void DoAreaDamage() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skillData.explosionSize);
        Debug.Log($"ProjectileBehaviour: Checking area damage for {hits.Length} potential targets.");
        foreach (var hit in hits) {
            if (ShouldAffectTarget(hit.gameObject)) {
                Debug.Log($"ProjectileBehaviour: Target {hit.gameObject.name} affected by explosion.");
                HealthManager healthManager = hit.GetComponent<HealthManager>();
                if (healthManager != null) {
                    healthManager.TakeDamage(skillData.explosionDamage, skillData.hitStunScale, skillData.hitStunDuration); Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;
                    ApplyExplosionKnockback(hit.transform, skillData.explosionKnockbackForce, knockbackDirection);
                    Debug.Log($"ProjectileBehaviour: Applied {skillData.explosionDamage} damage and knockback to {hit.gameObject.name}.");
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        OnCollision(collision);
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
        Gizmos.DrawWireSphere(transform.position, skillData.explosionSize);
    }
}
