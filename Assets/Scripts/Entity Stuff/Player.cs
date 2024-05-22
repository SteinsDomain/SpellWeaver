using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Player : MonoBehaviour {

    [SerializeField] private SpellManager spellManager;
    [SerializeField] private CollisionManager collisionManager;
    [SerializeField] private GameInput gameInput;
    public StatsSO stats;

    public enum MovementControls {Platformer, TopDown}
    public MovementControls movementControls;

    public bool isGrounded; 
    public bool isFacingRight = true;

    #region The Internal Stuff
    private Vector2 moveDirection;
    private float horizontalSpeed = 0f;
    private float verticalSpeed = 0f;
    private float smoothTime;
    private int airJumpsLeft;
    private bool isWallJumping;
    private bool isAimingUp;
    private bool isAimingDown;
    private bool CanWallJump => CheckForWall() != 0; // If CheckForWall returns anything but 0, a wall jump is possible
    private bool ignoreInput = false;
    #endregion

    //NewStuff
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
    #region Interaction Variables
    public LayerMask interactableLayer;
    public float interactRadius = 1.0f;
    public Transform interactPoint;
    #endregion

    void Awake() {
        if (spellManager == null) spellManager = gameObject.AddComponent<SpellManager>();
        if (collisionManager == null) collisionManager = gameObject.AddComponent<CollisionManager>();
        if(gameInput == null) gameInput = FindAnyObjectByType<GameInput>();
    }
    void Start() {
        ResetJump();
    }
    void Update() {
        switch (movementControls) {
            case MovementControls.Platformer:
            PlatformerMovement();
            HandleAimingUp();
            break;

            case MovementControls.TopDown:
            TopDownMovement();
            HandleAiming360();
            break;
        }

        UpdatePosition();

        HandleMelee();
        UpdateComboTimer();
        UpdateMeleeAttack();

        HandleSpellSelect();
        HandleSpellCasting();

        HandleInteractions();
    }

    //New Stuff
    #region Melee
    void HandleMelee() {
        if (gameInput.AttackPressed() && !isComboInCooldown) {
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

    void StartMeleeAttack(int comboStep) {
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

        if (attackPoint.parent != null)
        {
            if (attackPoint.parent.CompareTag("Player"))
            {
                activeMeleeAttack.layer = LayerMask.NameToLayer("Player Melee");
                Debug.Log("Assigned to Player Projectiles layer.");
            }
            else if (attackPoint.parent.CompareTag("Enemy"))
            {
                activeMeleeAttack.layer = LayerMask.NameToLayer("Enemy Melee");
                Debug.Log("Assigned to Enemy Projectiles layer.");
            }
            else
            {
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
    #endregion
    #region Interactions
    private void HandleInteractions() {
        if (gameInput.InteractPressed()) {
            CheckForInteractable();
        }
    }

    private void CheckForInteractable() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(interactPoint.position, interactRadius, interactableLayer);
        foreach (Collider2D hitCollider in hitColliders) {
            if (hitCollider.TryGetComponent(out IInteractable interactable)){
                interactable.Interact();
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(interactPoint.position, interactRadius);
    } 
    #endregion

    #region Collision and Gravity
    private void CheckGrounded() {
        bool wasGrounded = isGrounded;
        isGrounded = collisionManager.CheckIfGrounded(transform);
        if (isGrounded && !wasGrounded) {
            OnLanding();
        }
        if (movementControls == MovementControls.TopDown) {
            isGrounded = false;
        }
    }
    private void OnLanding() {
        ResetJump();
    }
    private void HandleFalling() {
        bool isWallSliding = CheckForWall() != 0 && !isGrounded && verticalSpeed < 0;
        if (!isGrounded) {
            if (isWallSliding) {
                ApplyGravity(stats.wallSlideGravityMod);
                verticalSpeed = Mathf.Max(verticalSpeed, -stats.maxWallSlideSpeed);
            }
            else {
                ApplyGravity();
            }
        }
        else {
            //verticalSpeed = Mathf.Max(0, verticalSpeed); // Prevents verticalSpeed from going negative while grounded, wasnt working properly?
        }
        if (!isWallSliding) {
            if (verticalSpeed < 0 || (!gameInput.JumpHeld() && verticalSpeed > 0)) {
                verticalSpeed -= stats.jumpDecay * Time.deltaTime; // Apply decay when falling or jump is not held
            }
        }
    }
    private void ApplyGravity(float modifier = 1f) {
        verticalSpeed -= (stats.gravity * modifier) * Time.deltaTime;
    }
    private int CheckForWall() {
        return collisionManager.CheckForWall();
    }
    #endregion

    #region Movement
    private void PlatformerMovement() {
        CheckGrounded();
        HandleFalling();
        Jump();
        GetMovementInput();
        UpdateHorizontalMovement();
        FlipPlayerSprite();
    }
    private void TopDownMovement() {
        CheckGrounded();
        GetMovementInput();
        UpdateHorizontalMovement();
        UpdateVerticalMovement();
        FlipPlayerSprite();
    }

    private void GetMovementInput() {
        switch (movementControls) {
            case MovementControls.Platformer:
                moveDirection = gameInput.GetMovementDirection();
            break;

            case MovementControls.TopDown:
                moveDirection = gameInput.GetElementMenuDirection();
            break;
        }
    }
    private void UpdateHorizontalMovement() {
        if (!spellManager.IsConcentrating && !ignoreInput) {
            float targetSpeed = moveDirection.x * (isGrounded ? stats.groundSpeed : stats.airSpeed);
            float currentSpeed = horizontalSpeed;
            // Determine the appropriate time factor based on whether the player is accelerating or decelerating
            float timeFactor = (moveDirection.x != 0) ?
                               (isGrounded ? stats.groundAccelerationTime : stats.airAccelerationTime) :
                               (isGrounded ? stats.groundDecelerationTime : stats.airDecelerationTime);
            // Interpolate towards the target speed using the calculated time factor
            horizontalSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / timeFactor);
            // Clamp the horizontal speed to ensure it does not exceed maximum speeds in either direction
            float maxSpeed = isGrounded ? stats.groundSpeed : stats.airSpeed;
            horizontalSpeed = Mathf.Clamp(horizontalSpeed, -maxSpeed, maxSpeed);
        }

        if (spellManager.IsConcentrating) {
            //maintain current momentum and direction, ignoring new input.
            moveDirection.x = Mathf.Sign(horizontalSpeed);
            if (isGrounded) {
                // Apply deceleration if grounded while concentrating as no input is effectively given
                horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, 0f, stats.groundSpeed * Time.deltaTime / stats.groundDecelerationTime);
            }
        }

        if (isGrounded && moveDirection.x == 0) {
            horizontalSpeed = Mathf.MoveTowards(horizontalSpeed, 0f, stats.groundSpeed * Time.deltaTime / stats.groundDecelerationTime);
        }
    }
    private void UpdateVerticalMovement() {
        if (!spellManager.IsConcentrating && !ignoreInput) {
            float targetSpeed = moveDirection.y * (isGrounded ? stats.groundSpeed : stats.airSpeed);
            float currentSpeed = verticalSpeed;
            // Determine the appropriate time factor based on whether the player is accelerating or decelerating
            float timeFactor = (moveDirection.y != 0) ?
                               (isGrounded ? stats.groundAccelerationTime : stats.airAccelerationTime) :
                               (isGrounded ? stats.groundDecelerationTime : stats.airDecelerationTime);
            // Interpolate towards the target speed using the calculated time factor
            verticalSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / timeFactor);
            // Clamp the vertical speed to ensure it does not exceed maximum speeds in either direction
            float maxSpeed = isGrounded ? stats.groundSpeed : stats.airSpeed;
            verticalSpeed = Mathf.Clamp(verticalSpeed, -maxSpeed, maxSpeed);
        }

        if (spellManager.IsConcentrating) {
            //maintain current momentum and direction, ignoring new input.
            moveDirection.y = Mathf.Sign(verticalSpeed);
            if (isGrounded) {
                // Apply deceleration if grounded while concentrating as no input is effectively given
                verticalSpeed = Mathf.MoveTowards(verticalSpeed, 0f, stats.groundSpeed * Time.deltaTime / stats.groundDecelerationTime);
            }
        }

        if (isGrounded && moveDirection.x == 0) {
            verticalSpeed = Mathf.MoveTowards(verticalSpeed, 0f, stats.groundSpeed * Time.deltaTime / stats.groundDecelerationTime);
        }
    }
    private void UpdatePosition() {
        horizontalSpeed = collisionManager.CheckForHorizontalCollision(horizontalSpeed, transform);
        transform.position += new Vector3(horizontalSpeed * Time.deltaTime, 0, 0);
        verticalSpeed = collisionManager.CheckForVerticalCollision(verticalSpeed, transform);
        transform.position += new Vector3(0, verticalSpeed * Time.deltaTime, 0);
    }
    #endregion

    #region Jump Section
    private void Jump() {
        if (spellManager.IsConcentrating) return;

        if (gameInput.JumpPressed()) {
            TryJump();
        }
    }
    private void TryJump() {
        if (CanWallJump && !isGrounded) {
            WallJump();
        }
        else if (isGrounded) {
            verticalSpeed = MathF.Sqrt(2 * stats.gravity * stats.groundJumpHeight);
        }
        else if (!isGrounded) {
            if (airJumpsLeft > 0) {
                verticalSpeed = MathF.Sqrt(2 * stats.gravity * stats.airJumpHeight);
                airJumpsLeft--;
            }
            
        }
    }
    private void WallJump() {
        int wallDirection = CheckForWall();

        if (wallDirection != 0) {
            Vector2 jumpDirection = new Vector2(stats.wallJumpDirection.x * -wallDirection, stats.wallJumpDirection.y);
            jumpDirection.Normalize();

            verticalSpeed = jumpDirection.y * stats.wallJumpForce;
            horizontalSpeed = jumpDirection.x * stats.wallJumpForce;

            isWallJumping = true;
            ignoreInput = true;
            Invoke(nameof(ResetWallJumpState), stats.wallJumpDuration);
            Invoke(nameof(ResetInputOverride), stats.inputOverrideDuration);
        }
    }
    private void ResetWallJumpState() {
        isWallJumping = false;
    }
    private void ResetInputOverride() {
        ignoreInput = false;
    }
    private void ResetJump() {
        airJumpsLeft = stats.maxAirJumps;
    }
    #endregion

    #region SpellCasting Section
    private void HandleAiming360() {
        /* Not Working As Intended 
        Vector2 aimDirection = gameInput.GetSchoolMenuDirection();
        if (aimDirection.sqrMagnitude > 0.01f) {  // Ensure the direction is significant to avoid jittering

            // Rotate the cast point to face the direction
            transform.rotation = Quaternion.LookRotation(Vector3.forward, aimDirection);
        }
        */
    }
    
    private void HandleAimingUp() {
        if (spellManager.currentSpellInstance?.CanAim == true) {
            float aimDirection = gameInput.GetAimDirection();  // Gets -1, 0, 1
            int rotationDirection = isFacingRight ? 1 : -1;

            if (aimDirection == 1 && !isAimingUp) {
                spellManager.castPoint.RotateAround(transform.position, Vector3.forward, 30 * rotationDirection);
                isAimingUp = true;  // Set the flag to true after rotating
                isAimingDown = false;
            }
            else if (aimDirection == 0 && isAimingUp) {
                spellManager.castPoint.RotateAround(transform.position, Vector3.forward, -30 * rotationDirection);
                isAimingUp = false;  // Reset the flag when the input is not upwards
                isAimingDown = false;
            }
        }
    }
    private void HandleSpellSelect() {
        if (gameInput.ElementMenuPressed()) {
            spellManager.HandleElementSelect();
        }
        if (gameInput.SchoolMenuPressed()) {
            spellManager.HandleSchoolSelect();
        }

        if (gameInput.ElementMenuHeld()) {
        }
        if (gameInput.SchoolMenuHeld()) {
        }

        if (gameInput.ElementMenuReleased()) {
        }
        if (gameInput.SchoolMenuReleased()) {
        }
    }
    public void HandleSpellCasting() {
        if (gameInput.CastPressed()) {
            spellManager.CastPressed();
        }

        if (gameInput.CastHeld()) {
            spellManager.CastHeld();
        }

        if (gameInput.CastReleased()) {
            spellManager.CastReleased();
        }
    }
    #endregion

    #region Animation Stuff
    private void FlipPlayerSprite() {
        if (!isGrounded) {
            int wallDirection = CheckForWall();
            if (wallDirection != 0) { //if we're touching a wall on either side while airborne
                isFacingRight = wallDirection == -1; //Face away from the wall
            }
            else { //Otherwise turn freely in the air
                FlipBasedOnMovement();
            }
        }
        else {//and while we're on the ground
            FlipBasedOnMovement();
        }
        transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
    }
    private void FlipBasedOnMovement() {
        if (spellManager.IsConcentrating) {
            transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
        }
        else {
            if (moveDirection.x > 0) {
                isFacingRight = true;
                //castPoint.localScale = new Vector3(1, 1, 1);  // Face right
            }
            else if (moveDirection.x < 0) {
                isFacingRight = false;
                //castPoint.localScale = new Vector3(-1, 1, 1);  // Face left
            }
            transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
        }
    }
    #endregion
}