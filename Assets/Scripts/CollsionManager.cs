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

    private Collider2D colliderComponent;

    private void Awake() {
        collisionLayer = LayerMask.GetMask("Ground");
        colliderComponent = GetComponent<Collider2D>();
        if (colliderComponent == null) {
            Debug.LogError("CollisionManager: No Collider2D component found on " + gameObject.name);
        }
    }

    public float CheckForVerticalCollision(float verticalSpeed, Transform transform) {
        if (colliderComponent == null) return verticalSpeed;

        float direction = Mathf.Sign(verticalSpeed);
        Vector2 castDirection = Vector2.up * direction;
        float castDistance = Mathf.Abs(verticalSpeed * Time.deltaTime) + verticalCastBuffer;
        RaycastHit2D hit = default;

        switch (colliderComponent) {
            case BoxCollider2D box:
            hit = Physics2D.BoxCast(transform.position, box.size, 0, castDirection, castDistance, collisionLayer);
            break;
            case CircleCollider2D circle:
            hit = Physics2D.CircleCast(transform.position, circle.radius, castDirection, castDistance, collisionLayer);
            break;
            case CapsuleCollider2D capsule:
            hit = Physics2D.CapsuleCast(transform.position, capsule.size, capsule.direction, 0, castDirection, castDistance, collisionLayer);
            break;
            case PolygonCollider2D polygon:
            hit = Physics2D.BoxCast(transform.position, polygon.bounds.size, 0, castDirection, castDistance, collisionLayer);
            break;
            default:
            Debug.LogError("Unsupported collider type: " + colliderComponent.GetType());
            return verticalSpeed;
        }

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
        RaycastHit2D hit = default;

        switch (colliderComponent) {
            case BoxCollider2D box:
            hit = Physics2D.BoxCast(transform.position, box.size, 0, castDirection, castDistance, collisionLayer);
            break;
            case CircleCollider2D circle:
            hit = Physics2D.CircleCast(transform.position, circle.radius, castDirection, castDistance, collisionLayer);
            break;
            case CapsuleCollider2D capsule:
            hit = Physics2D.CapsuleCast(transform.position, capsule.size, capsule.direction, 0, castDirection, castDistance, collisionLayer);
            break;
            case PolygonCollider2D polygon:
            hit = Physics2D.BoxCast(transform.position, polygon.bounds.size, 0, castDirection, castDistance, collisionLayer);
            break;
            default:
            Debug.LogError("Unsupported collider type: " + colliderComponent.GetType());
            return horizontalSpeed;
        }

        if (hit.collider != null) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeAngle <= 45) { // Consider adjusting the threshold based on your game's needs
                                    // Allow movement along the slope, adjust horizontal speed based on the slope angle
                horizontalSpeed = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * horizontalSpeed;
            }
            else {
                horizontalSpeed = (hit.distance - collisionAdjustmentBuffer) * direction / Time.deltaTime;
            }
        }
        return horizontalSpeed;
    }

    public bool CheckIfGrounded(Transform transform, float checkDistance = 0.1f) {
        if (colliderComponent == null) return false;

        Vector2 origin = new Vector2(transform.position.x, transform.position.y - colliderComponent.bounds.extents.y);
        RaycastHit2D hit = default;

        switch (colliderComponent) {
            case BoxCollider2D box:
            hit = Physics2D.BoxCast(origin, new Vector2(box.size.x, 0.1f), 0, Vector2.down, checkDistance, collisionLayer);
            break;
            case CircleCollider2D circle:
            hit = Physics2D.CircleCast(origin, circle.radius, Vector2.down, checkDistance, collisionLayer);
            break;
            case CapsuleCollider2D capsule:
            hit = Physics2D.CapsuleCast(origin, new Vector2(capsule.size.x, 0.1f), capsule.direction, 0, Vector2.down, checkDistance, collisionLayer);
            break;
            case PolygonCollider2D polygon:
            hit = Physics2D.BoxCast(origin, new Vector2(polygon.bounds.size.x, 0.1f), 0, Vector2.down, checkDistance, collisionLayer);
            break;
            default:
            Debug.LogError("Unsupported collider type for grounded check: " + colliderComponent.GetType());
            return false;
        }

        return hit.collider != null;
    }

    public int CheckForWall() {
        if (colliderComponent == null) {
            Debug.LogError("No Collider2D component found on " + gameObject.name);
            return 0;
        }

        Vector2 direction;
        RaycastHit2D hit = default;

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
