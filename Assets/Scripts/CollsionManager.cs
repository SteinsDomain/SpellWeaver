using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float verticalCastBuffer = 0.01f;
    [SerializeField] private float horizontalCastBuffer = 0.01f;
    [SerializeField] private float collisionAdjustmentBuffer = 0.01f;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float slopeCheckDistance = 0.1f;
    [SerializeField] private float maxSlopeAngle = 45f;

    private Collider2D colliderComponent;

    private void Awake() {
        collisionLayer = LayerMask.GetMask("Ground");
        TryGetComponent<Collider2D>(out colliderComponent);
        if (colliderComponent == null) {
            Debug.LogError("CollisionManager: No Collider2D component found on " + gameObject.name);
        }
    }

    public float CheckForVerticalCollision(float verticalSpeed, Transform transform) {
        if (colliderComponent == null) return verticalSpeed;

        float direction = Mathf.Sign(verticalSpeed);
        Vector2 castDirection = Vector2.up * direction;
        float castDistance = Mathf.Abs(verticalSpeed * Time.deltaTime) + verticalCastBuffer;
        RaycastHit2D hit = CastCollider(transform, castDirection, castDistance);

        if (hit.collider != null) {
            verticalSpeed = (hit.distance - collisionAdjustmentBuffer) * direction / Time.deltaTime;
        }

        return verticalSpeed;

    }

    public float CheckForHorizontalCollision(float horizontalSpeed, Transform transform) {
        if (colliderComponent == null) return horizontalSpeed;

        float direction = Mathf.Sign(horizontalSpeed);
        Vector2 castDirection = Vector2.right * direction;
        float castDistance = Mathf.Abs(horizontalSpeed * Time.deltaTime) + horizontalCastBuffer;
        RaycastHit2D hit = CastCollider(transform, castDirection, castDistance);

        if (hit.collider != null) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeAngle <= maxSlopeAngle) {
                // Adjust movement along the slope
                Vector2 slopeDirection = new Vector2(hit.normal.y, -hit.normal.x);
                slopeDirection *= direction;

                float adjustedSpeed = horizontalSpeed / Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
                transform.Translate(slopeDirection.normalized * adjustedSpeed * Time.deltaTime);
                horizontalSpeed = 0; // Stop horizontal movement as it's handled by the slope adjustment
            }
            else {
                horizontalSpeed = (hit.distance - collisionAdjustmentBuffer) * direction / Time.deltaTime;
            }
        }
        return horizontalSpeed;
    }

    private RaycastHit2D CastCollider(Transform transform, Vector2 direction, float distance) {
        switch (colliderComponent) {
            case BoxCollider2D box:
            return Physics2D.BoxCast(transform.position, box.size, 0, direction, distance, collisionLayer);
            case CircleCollider2D circle:
            return Physics2D.CircleCast(transform.position, circle.radius, direction, distance, collisionLayer);
            case CapsuleCollider2D capsule:
            return Physics2D.CapsuleCast(transform.position, capsule.size, capsule.direction, 0, direction, distance, collisionLayer);
            case PolygonCollider2D polygon:
            return Physics2D.BoxCast(transform.position, polygon.bounds.size, 0, direction, distance, collisionLayer);
            default:
            Debug.LogError("Unsupported collider type: " + colliderComponent.GetType());
            return default;
        }
    }

    public bool CheckIfGrounded(Transform transform, float checkDistance = 0.1f) {
        if (colliderComponent == null) return false;

        //Vector2 origin = new Vector2(transform.position.x, transform.position.y - colliderComponent.bounds.extents.y);
        RaycastHit2D hit = CastCollider(transform, Vector2.down, checkDistance);

        return hit.collider != null;
    }

    public int CheckForWall() {
        if (colliderComponent == null) {
            Debug.LogError("No Collider2D component found on " + gameObject.name);
            return 0;
        }

        Vector2 direction;
        RaycastHit2D hit;

        // Apply switch-case for different collider types
        switch (colliderComponent) {
            case BoxCollider2D box:
            direction = Vector2.left;
            hit = Physics2D.BoxCast(transform.position, box.size, 0f, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return -1;

            direction = Vector2.right;
            hit = Physics2D.BoxCast(transform.position, box.size, 0f, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return 1;
            break;

            case CircleCollider2D circle:
            direction = Vector2.left;
            hit = Physics2D.CircleCast(transform.position, circle.radius, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return -1;

            direction = Vector2.right;
            hit = Physics2D.CircleCast(transform.position, circle.radius, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return 1;
            break;

            case CapsuleCollider2D capsule:
            direction = Vector2.left;
            hit = Physics2D.CapsuleCast(transform.position, capsule.size, capsule.direction, 0, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return -1;

            direction = Vector2.right;
            hit = Physics2D.CapsuleCast(transform.position, capsule.size, capsule.direction, 0, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return 1;
            break;

            case PolygonCollider2D polygon:
            direction = Vector2.left;
            hit = Physics2D.BoxCast(transform.position, polygon.bounds.size, 0f, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return -1;

            direction = Vector2.right;
            hit = Physics2D.BoxCast(transform.position, polygon.bounds.size, 0f, direction, wallCheckDistance, collisionLayer);
            if (hit.collider != null) return 1;
            break;

            default:
            Debug.LogError("Unsupported collider type: " + colliderComponent.GetType());
            return 0;
        }

        return 0; // No wall detected
    }

}
