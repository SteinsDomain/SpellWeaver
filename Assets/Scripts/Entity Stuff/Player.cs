using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cinemachine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Player : MonoBehaviour {

    private HealthManager healthManager;
    private ManaManager manaManager;
    private MeleeManager meleeManager;
    private SpellManager spellManager;
    private CollisionManager collisionManager;
    private GameInput gameInput;
    public StatsSO stats;


    public SimpleSpellList mySpellList;

    public enum MovementControls {Platformer, TopDown, Runner}
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
    private bool CanWallJump => collisionManager.CheckForWall() != 0; // If CheckForWall returns anything but 0, a wall jump is possible
    private bool ignoreInput = false;
    #endregion

    #region Interaction Variables
    public LayerMask interactableLayer;
    public float interactRadius = 1.0f;
    public Transform interactPoint;
    #endregion

    void Awake() {
        InitializeComponents();

        var theCam = FindAnyObjectByType<CinemachineVirtualCamera>();
        theCam.Follow = this.transform;
        if (gameInput == null) gameInput = FindAnyObjectByType<GameInput>();
    }
    void Update() {
        switch (movementControls) {
            case MovementControls.Platformer:
            HandlePlatformerMovement();
            HandleAimingUp();
            break;

            case MovementControls.TopDown:
            HandleTopDownMovement();
            HandleAiming360();
            break;

            case MovementControls.Runner:
            HandleRunnerMovement();
            HandleAimingUp();
            break;
        }
        if (meleeManager != null) {
            HandleMelee();
        }
        if (spellManager != null) {
            HandleSpellSelect();
            HandleSpellCasting();
        }
        if (mySpellList != null) {
            HandleSimpleCasting();
        }
        HandleInteractions();
        UpdatePosition();
    }
    private void InitializeComponents() {
        TryGetComponent<HealthManager>(out healthManager);
        if (healthManager != null) {
            healthManager.stats = stats;
            healthManager.OnHealthDepleted += HandleDeath;
        }
        TryGetComponent<ManaManager>(out manaManager);
        if (manaManager != null) {
            manaManager.stats = stats;
        }
        TryGetComponent<MeleeManager>(out meleeManager);
        TryGetComponent<SpellManager>(out spellManager);
        TryGetComponent<CollisionManager>(out collisionManager);
        TryGetComponent<SimpleSpellList>(out mySpellList);
    }

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
        bool isWallSliding = collisionManager.CheckForWall() != 0 && !isGrounded && verticalSpeed < 0;
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
    #endregion

    #region Movement
    private void HandlePlatformerMovement() {
        CheckGrounded();
        HandleFalling();
        Jump();
        GetMovementInput();
        UpdateHorizontalMovement();
        FlipPlayerSprite();
    }
    private void HandleTopDownMovement() {
        CheckGrounded();
        GetMovementInput();
        UpdateHorizontalMovement();
        UpdateVerticalMovement();
        FlipPlayerSprite();
    }
    private void HandleRunnerMovement() {
        CheckGrounded();
        HandleFalling();
        Jump();
        moveDirection = Vector2.right;
        UpdateHorizontalMovement();
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
        verticalSpeed = collisionManager.CheckForVerticalCollision(verticalSpeed, transform);
        horizontalSpeed = collisionManager.CheckForHorizontalCollision(horizontalSpeed, transform);
        transform.position += new Vector3(horizontalSpeed * Time.deltaTime, verticalSpeed * Time.deltaTime, 0);
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
        int wallDirection = collisionManager.CheckForWall();

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
                interactable.Interact(gameObject);
            }
        }
    }
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(interactPoint.position, interactRadius);
    } 
    #endregion

    #region Melee Section
    void HandleMelee() {
        if (gameInput.AttackPressed()) {
            meleeManager.TryMelee();
        }
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
            int wallDirection = collisionManager.CheckForWall();
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

    private void HandleSimpleCasting() {
        if (gameInput.CastPressed()) {
            mySpellList.CastPressed();
        }

        if (gameInput.CastHeld()) {
            mySpellList.CastHeld();
        }

        if (gameInput.CastReleased()) {
            mySpellList.CastReleased();
        }
    }

    private void HandleDeath() {
        Destroy(gameObject);
    }
}