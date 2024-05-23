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
    private SpellManager spellManager;
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
        collisionManager = GetComponent<CollisionManager>();
        spellManager = GetComponent<SpellManager>();
        initialPosition = transform.position;
        player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update() {
        CheckGrounded();
        HandleFalling();
        Move();
        UpdateHorizontalPosition();
        UpdateVerticalPosition();

        UpdateStateMachine();
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

    public void SetMoveDirection(Vector3 direction) {
        // Update the moveDirection based on the AI's decision
        moveDirection = direction;
    }
    private void Move() {
        if (shouldMove && moveDirection != Vector2.zero) {
            
            UpdateHorizontalMovement();
            

            // Optionally handle flipping the enemy sprite based on the direction
            if (moveDirection.x > 0 && !isFacingRight || moveDirection.x < 0 && isFacingRight) {
                Flip();
            }
        } 
        else {
            horizontalSpeed = 0f;
        }
    }
    private void Flip() {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    private void CheckGrounded() {
        isGrounded = collisionManager.CheckIfGrounded(transform);
    }
    private void HandleFalling() {
        if (!isGrounded) {
            ApplyGravity();
        }
        else {
            //verticalSpeed = Mathf.Max(0, verticalSpeed); // Prevents verticalSpeed from going negative while grounded
        }
    }
    private void ApplyGravity(float modifier = 1f) {
        verticalSpeed -= (stats.gravity * modifier) * Time.deltaTime;
    }
    public Vector3 GetPlayerPosition() {
        return GameObject.FindWithTag("Player").transform.position;
    }
    private void UpdateHorizontalMovement() {
            float targetspeed = moveDirection.x * stats.groundSpeed;
            float speedDiff = targetspeed - horizontalSpeed;
            // Accelerate or decelerate towards the target speed.
            smoothTime = (moveDirection.x == 0 && isGrounded) ? stats.groundDecelerationTime : stats.groundAccelerationTime;
            float movementAdjustment = Mathf.Pow(Mathf.Abs(speedDiff) * smoothTime, 2) * Mathf.Sign(speedDiff);
            horizontalSpeed += movementAdjustment * Time.deltaTime;
            // Clamp the horizontal speed to prevent it from exceeding the target speed.
            horizontalSpeed = Mathf.Clamp(horizontalSpeed, -stats.groundSpeed, stats.groundSpeed);
    }
    private void UpdateHorizontalPosition() {
        horizontalSpeed = collisionManager.CheckForHorizontalCollision(horizontalSpeed, transform);
        transform.position += new Vector3(horizontalSpeed * Time.deltaTime, 0, 0);
    }
    private void UpdateVerticalPosition() {
        verticalSpeed = collisionManager.CheckForVerticalCollision(verticalSpeed, transform);
        transform.position += new Vector3(0, verticalSpeed * Time.deltaTime, 0);
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
