using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy : MonoBehaviour
{
    //Private Managers found on Awake 
    private HealthManager healthManager;
    public SkillManager skillManager;
    private MovementManager movementManager;
    private CollisionManager collisionManager;

    // Public fields and serialized fields for inspector tweaking
    public StatsSO stats;

    [SerializeField] public float attackRange = 5f;
    [SerializeField] public float chaseRange = 10f;
    [SerializeField] public float attackCooldown = 1.5f;
    [SerializeField] public float idlePauseDuration = 2.0f;
    [SerializeField] public float wanderRange = 4f;
    [SerializeField] public float safeDistance = 2f;  // Minimum distance to keep away from player

    public bool isFacingRight = true;
    public bool shouldMove = true;
    public bool isGrounded;

    #region Internal Stuff
    public Vector2 moveDirection = Vector2.zero;
    public float horizontalSpeed = 0f;
    public float verticalSpeed = 0f;
    public float smoothTime;
    public float lastAttackTime = 0f;
    public Vector3 wanderTarget;
    public bool isMovingToTarget = false;
    public float idleEndTime = 0f;
    public Vector3 initialPosition;
    public Transform player;
    #endregion

    public EnemyBaseStateSO currentState;
    public EnemyBaseStateSO idleState;
    public EnemyBaseStateSO wanderState;
    public EnemyBaseStateSO chaseState;
    public EnemyBaseStateSO attackState;

    // Initialize components and set initial position
    void Awake() {
        TryGetComponent<HealthManager>(out healthManager); 
        if (healthManager != null ) {
            healthManager.OnHealthDepleted += HandleDeath;
        }
        TryGetComponent<CollisionManager>(out collisionManager);
        TryGetComponent<SkillManager>(out skillManager); 
        TryGetComponent<MovementManager>(out movementManager);

        initialPosition = transform.position;
        player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Start()
    {
        TransitionToState(idleState);
    }

    void Update() {
        movementManager.CheckGrounded();
        movementManager.HandleFalling(false);

        //UpdateStateMachine();
        currentState.UpdateState(this);


        MoveToPoint();

        movementManager.UpdatePosition();
    }

    public void TransitionToState(EnemyBaseStateSO newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    public void SetMoveDirection(Vector3 direction) {
        // Update the moveDirection based on the AI's decision
        moveDirection = direction;
    }

    private void MoveToPoint() {
        if (!shouldMove) { return; }

        movementManager.UpdateHorizontalMovement(moveDirection);

        // Handle flipping the enemy sprite based on the direction
        if (moveDirection.x > 0 && !movementManager.isFacingRight || moveDirection.x < 0 && movementManager.isFacingRight)
        {
            movementManager.FlipPlayerSprite(moveDirection);
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
