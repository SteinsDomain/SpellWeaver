using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy : MonoBehaviour
{


    private enum State { Idle, Wander, Chase, Attack}
    private State currentState = State.Idle;

    public StatsSO stats;
    private HealthManager healthManager;
    private SpellManager spellManager;
    private MovementManager movementManager;
    private CollisionManager collisionManager;

    public bool isFacingRight = true;
    private Vector2 moveDirection = Vector2.zero;
    private float horizontalSpeed = 0f;
    private float verticalSpeed = 0f;
    private float smoothTime;
    public bool isGrounded;

    public bool shouldMove = true;

    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float idlePauseDuration = 2.0f;
    [SerializeField] private float wanderRange = 4f;
    [SerializeField] private float safeDistance = 2f;  // Minimum distance to keep away from player

    private float lastAttackTime = 0f;
    private Vector3 wanderTarget;
    private bool isMovingToTarget = false;
    private float idleEndTime = 0f;
    private Vector3 initialPosition;
    private Transform player;


    void Awake() {
        healthManager = GetComponent<HealthManager>();
        if (healthManager != null ) {
            healthManager.OnHealthDepleted += HandleDeath;
        }
        collisionManager = GetComponent<CollisionManager>();
        spellManager = GetComponent<SpellManager>();
        initialPosition = transform.position;
        player = GameObject.FindWithTag("Player")?.transform;
        TryGetComponent<MovementManager>(out movementManager);
    }

    void Update() {
        movementManager.CheckGrounded(Player.MovementControls.Enemy);
        movementManager.HandleFalling(false);
        UpdateStateMachine();
        Move();
        movementManager.UpdatePosition();
    }

    private void UpdateStateMachine() {
        
        if (player != null) {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            switch (currentState) {

                case State.Idle:
                Debug.Log("State: Idle");
                HandleIdleState(distanceToPlayer);
                break;

                case State.Wander:
                Debug.Log("State: Wander");
                HandleWanderState(distanceToPlayer);
                break;

                case State.Chase:
                Debug.Log("State: Chase");
                HandleChaseState(distanceToPlayer);
                break;

                case State.Attack:
                Debug.Log("State: Attack");
                HandleAttackState(distanceToPlayer);
                break;
            }

        }
        
    }
    private void HandleIdleState(float distanceToPlayer) {
        if (distanceToPlayer <= chaseRange) {
            currentState = State.Chase;
            shouldMove = true;
            Debug.Log("Idle: Switching to Chase state");
        }
        else if (Time.time >= idleEndTime) {
            // Switch to the Wander state after the idle time ends
            currentState = State.Wander;
            shouldMove = true;  // Re-enable movement after idle
            isMovingToTarget = false;  // Reset the wander target status
            Debug.Log("Idle: Switching to Wander state");
        }
    }
    private void HandleWanderState(float distanceToPlayer) {

        if (distanceToPlayer <= chaseRange) {
            currentState = State.Chase;
            shouldMove = true;
            Debug.Log("Wander: Switching to Chase state");
        }

        else if (Mathf.Abs(transform.position.x - initialPosition.x) > wanderRange) {
            // If outside wander range, return to the initial position
            moveDirection = initialPosition.x > transform.position.x ? Vector2.right : Vector2.left;
            shouldMove = true;
            isMovingToTarget = false;  // Not wandering but returning to the center
            Debug.Log("Wander: Returning to initial position");
        }

        else if (isMovingToTarget) {
            // Check if the enemy reached the wander target
            if (Mathf.Abs(transform.position.x - wanderTarget.x) < 0.1f) {
                currentState = State.Idle;  // Reached target, switch to idle
                shouldMove = false;
                idleEndTime = Time.time + idlePauseDuration;  // Set idle duration
                Debug.Log("Wander: Reached target");
            }
        }

        else {
            // Choose a new random target position to wander towards within the allowed range
            float targetOffset = UnityEngine.Random.Range(-wanderRange, wanderRange);
            wanderTarget = new Vector3(initialPosition.x + targetOffset, transform.position.y, transform.position.z);
            wanderTarget.x = Mathf.Clamp(wanderTarget.x, initialPosition.x - wanderRange, initialPosition.x + wanderRange);

            // Set the movement direction based on the new target's position
            moveDirection = wanderTarget.x > transform.position.x ? Vector2.right : Vector2.left;
            isMovingToTarget = true;
            shouldMove = true;
            Debug.Log($"Wander: Moving to new target at {wanderTarget.x}");
        }

    }
    private void HandleChaseState(float distanceToPlayer) {
        // Transition to attack state if within attack range
        if (distanceToPlayer <= attackRange) {
            currentState = State.Attack;
            lastAttackTime = Time.time;
            shouldMove = false;
            Debug.Log("Chase: Switching to Attack state");
        }
        // Return to idle if the player is out of chase range
        else if (distanceToPlayer > chaseRange) {
            currentState = State.Idle;
            Debug.Log("Chase: Switching to Idle state");
        }
        // Maintain safe distance while approaching attack range
        else {
            float xDirection = (player.position.x > transform.position.x ? 1 : -1);
            if (Mathf.Abs(transform.position.x - player.position.x) < safeDistance) {
                xDirection = -xDirection;
            }
            moveDirection = new Vector2(xDirection, 0);
            shouldMove = true;
        }
    }
    private void HandleAttackState(float distanceToPlayer) {

        // Stay stationary while attacking
        shouldMove = false;
        moveDirection = Vector2.zero;
        horizontalSpeed = 0f; // Stop horizontal movement

        // Return to chase if the player is out of attack range
        if (distanceToPlayer > attackRange) {
            currentState = State.Chase;
            shouldMove = true;
            Debug.Log("Attack: Switching to Chase state");
        }
        // Fire a spell based on cooldown while remaining stationary
        else {
            if (Time.time - lastAttackTime >= attackCooldown) {
                spellManager.CastPressed();
                lastAttackTime = Time.time;
                Debug.Log("Attack: Casting spell");
            }
        }
    }
    public Vector3 GetPlayerPosition() {
        return GameObject.FindWithTag("Player").transform.position;
    }
    public void SetMoveDirection(Vector3 direction) {
        // Update the moveDirection based on the AI's decision
        moveDirection = direction;
    }
    private void Move() {
        if (shouldMove && moveDirection != Vector2.zero) {
            
            movementManager.UpdateHorizontalMovement(moveDirection);
            

            // Optionally handle flipping the enemy sprite based on the direction
            if (moveDirection.x > 0 && !movementManager.isFacingRight || moveDirection.x < 0 && movementManager.isFacingRight) {
                movementManager.FlipPlayerSprite(moveDirection);
            }
        } 
        else {
            horizontalSpeed = 0f;
        }
    }
    public void TakeKnockback(float knockbackForce, Vector2 knockbackDirection) {
        // Normalize the knockback direction and multiply by the knockback force
        Vector2 knockback = knockbackDirection.normalized * knockbackForce;
        horizontalSpeed += knockback.x;
        verticalSpeed += knockback.y;
        movementManager.UpdateHorizontalMovement(new Vector2(horizontalSpeed, verticalSpeed));
        Debug.Log($"Enemy: Taking knockback with force {knockbackForce} in direction {knockbackDirection}.");
    }
    void HandleDeath() {
        GameManager.Instance.IncreaseScore(1);
        Destroy(gameObject);
        Debug.Log("Enemy has died.");
    }
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(initialPosition, wanderRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
