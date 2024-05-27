using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static Player;
using UnityEngine.EventSystems;

public class MovementManager : MonoBehaviour
{
    private GameInput gameInput;
    public StatsSO stats;
    private CollisionManager collisionManager;
    private SpellManager spellManager;

    public enum MovementControls { Platformer, TopDown, Runner }
    public MovementControls movementControls;

    public bool isGrounded;
    public bool isFacingRight = true;

    private Vector2 moveDirection;
    private float horizontalSpeed = 0f;
    private float verticalSpeed = 0f;
    private float smoothTime;
    private int airJumpsLeft;
    private bool isWallJumping;
    private bool ignoreInput = false;
    private bool CanWallJump => CheckForWall() != 0; // If CheckForWall returns anything but 0, a wall jump is possible 

    private void Awake() {
        TryGetComponent<SpellManager>(out spellManager);
        TryGetComponent<CollisionManager>(out collisionManager);

        if (gameInput == null) gameInput = FindAnyObjectByType<GameInput>();

    }

    private void Update() {

        switch (movementControls) {
            case MovementControls.Platformer:
            PlatformerMovement();
            break;

            case MovementControls.TopDown:
            TopDownMovement();
            break;

            case MovementControls.Runner:
            RunnerMovement();
            break;
        }
        UpdatePosition();
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
    private void RunnerMovement() {
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
}
