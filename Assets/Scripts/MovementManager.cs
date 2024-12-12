using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static Player;
using UnityEngine.EventSystems;

public class MovementManager : MonoBehaviour
{
    public StatsSO stats;
    private CollisionManager collisionManager;
    public bool isGrounded;
    public bool isFacingRight = true;
    private float horizontalSpeed = 0f;
    private float verticalSpeed = 0f;
    private float smoothTime;
    private int airJumpsLeft;
    private bool isWallJumping;
    private bool ignoreInput = false;
    private bool CanWallJump => CheckForWall() != 0; // If CheckFo/rWall returns anything but 0, a wall jump is possible 

    private Rigidbody2D rb;

    private void Awake() {
        TryGetComponent<CollisionManager>(out collisionManager);
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (Input.GetKey(KeyCode.R)) {

            ApplyRecoil(1f);
        }
    }

    #region Collision and Gravity
    public void CheckGrounded() {
        bool wasGrounded = isGrounded;
        isGrounded = collisionManager.CheckIfGrounded(transform);
        if (isGrounded && !wasGrounded) {
            OnLanding();
        }
        if (GameManager.Instance.movementControls == GameManager.MovementControls.TopDown){
            isGrounded = false;
        }
    }
    private void OnLanding() {
        ResetJump();
    }
    public void HandleFalling(bool jumpHeld) {
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
            verticalSpeed = Mathf.Max(0, verticalSpeed); // Prevents verticalSpeed from going negative while grounded, wasnt working properly?
        }
        if (!isWallSliding) {
            if (verticalSpeed < 0 || (!jumpHeld && verticalSpeed > 0)) {
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
    public void UpdateHorizontalMovement(Vector2 moveDirection, bool isConcentrating = false) {
        if (!isConcentrating && !ignoreInput) {
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

        if (isConcentrating) {
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
    public void UpdateVerticalMovement(Vector2 moveDirection, bool isConcentrating = false) {
        if (!isConcentrating && !ignoreInput) {
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

        if (isConcentrating) {
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

    public void UpdatePosition()
    {
        //horizontalSpeed = collisionManager.CheckForHorizontalCollision(horizontalSpeed, transform);
        //verticalSpeed = collisionManager.CheckForVerticalCollision(verticalSpeed, transform);

        //transform.position += new Vector3(horizontalSpeed * Time.deltaTime, verticalSpeed * Time.deltaTime, 0);
        // Apply the calculated speeds to the Rigidbody2D
        Vector2 velocity = rb.velocity;
        velocity.x = horizontalSpeed;
        velocity.y = verticalSpeed;
        rb.velocity = velocity;
    }
    #endregion

    #region Jump Section
    public void TryJump() {
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

    public void FlipPlayerSprite(Vector2 moveDirection, bool isConcentrating = false) {
        if (!isGrounded) {
            int wallDirection = CheckForWall();
            if (wallDirection != 0) { //if we're touching a wall on either side while airborne
                isFacingRight = wallDirection == -1; //Face away from the wall
            }
            else { //Otherwise turn freely in the air
                FlipBasedOnMovement(moveDirection, isConcentrating);
            }
        }
        else {//and while we're on the ground
            FlipBasedOnMovement(moveDirection, isConcentrating);
        }
        SetRotation(isFacingRight);
    }
    private void FlipBasedOnMovement(Vector2 moveDirection, bool isConcentrating) {
        if (isConcentrating) {
            SetRotationBasedOnCastPoint();
        }
        else {
            if (moveDirection.x != 0) { // Check if there is horizontal movement
        
                isFacingRight = moveDirection.x > 0;
            }
            SetRotation(isFacingRight);
        }
    }

    private void SetRotationBasedOnCastPoint() {
        // Check the direction the castPoint is facing
        isFacingRight = GetComponent<SkillManager>().castPoint.right.x > 0;

        // Set the rotation based on the castPoint's direction
        SetRotation(isFacingRight);
    }

    private void SetRotation(bool faceRight) {
        float yRotation = faceRight ? 0 : 180;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        // If you need to adjust castPoint as well
        // castPoint.localRotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void ApplyRecoil(float recoilAmount) {
        // Apply recoil force to the rigidbody
        Vector2 recoilDirection = -GetComponent<SkillManager>().castPoint.right;
        rb.AddForce(recoilDirection * recoilAmount);
    }
}
