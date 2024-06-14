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

    private GameInput gameInput;
    public StatsSO stats;

    #region Managers
    private HealthManager healthManager;
    private ManaManager manaManager;
    private MeleeManager meleeManager;
    private SpellManager spellManager;
    private MovementManager movementManager; 
    #endregion

    public enum MovementControls {Platformer, TopDown, Runner, Enemy}
    public MovementControls movementControls;

    #region Interaction Variables
    public LayerMask interactableLayer;
    public float interactRadius = 1.0f;
    public Transform interactPoint;
    #endregion

    #region The Internal Stuff
    private Vector2 moveDirection;
    private Vector2 lastAimDirection;
    private bool isAimingUp;
    private bool isAimingDown;
    #endregion

    void Awake() {
        InitializeComponents();

        var theCam = FindAnyObjectByType<CinemachineVirtualCamera>();
        theCam.Follow = this.transform;
        if (gameInput == null) gameInput = FindAnyObjectByType<GameInput>();
    }
    void Update() {

        if (Input.GetKeyDown(KeyCode.T)) {
            TimeDilationManager.Instance.SetTimeDilation(0.3f, 5f);
        }

        if (Input.GetKeyDown(KeyCode.Y)) {
            TimeDilationManager.Instance.StartTimeDilation(0.3f);
        }

        if (Input.GetKeyUp(KeyCode.Y)) {
            TimeDilationManager.Instance.StopTimeDilation();
        }

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
        HandleInteractions();
        
    }
    private void FixedUpdate() {
        movementManager.UpdatePosition();
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
        TryGetComponent<MovementManager>(out movementManager);
        if (movementManager != null) {
            movementManager.stats = stats;
        }
        TryGetComponent<MeleeManager>(out meleeManager);
        TryGetComponent<SpellManager>(out spellManager);
    }

    #region Movement
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
    public void GetJumpInput() {
        if (spellManager.IsConcentrating) return;

        if (gameInput.JumpPressed()) {
            movementManager.TryJump();
        }
    }

    private void HandlePlatformerMovement() {
        movementManager.CheckGrounded(movementControls);
        movementManager.HandleFalling(gameInput.JumpHeld());
        GetJumpInput();
        GetMovementInput();

        movementManager.UpdateHorizontalMovement(moveDirection, spellManager.IsConcentrating);

        movementManager.FlipPlayerSprite(moveDirection, spellManager.IsConcentrating);
    }
    private void HandleTopDownMovement() {
        movementManager.CheckGrounded(movementControls);
        GetMovementInput();
        movementManager.UpdateHorizontalMovement(moveDirection, spellManager.IsConcentrating);
        movementManager.UpdateVerticalMovement(moveDirection, spellManager.IsConcentrating);

        movementManager.FlipPlayerSprite(lastAimDirection, spellManager.IsConcentrating);

    }
    private void HandleRunnerMovement() {
        movementManager.CheckGrounded(movementControls);
        movementManager.HandleFalling(gameInput.JumpHeld());
        GetJumpInput();
        moveDirection = Vector2.right;
        movementManager.UpdateHorizontalMovement(moveDirection, spellManager.IsConcentrating);
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
        Vector2 aimDirection = gameInput.GetSchoolMenuDirection();  // Gets the direction vector from the stick input

        if (aimDirection != Vector2.zero) {
            lastAimDirection = aimDirection;
        }
        else if (moveDirection != Vector2.zero) {
            lastAimDirection = moveDirection;
        }

        float angle = Mathf.Atan2(lastAimDirection.y, lastAimDirection.x) * Mathf.Rad2Deg;

        // Check if the player is facing right or left and adjust the angle accordingly
        if (!movementManager.isFacingRight) {
            angle = 180 - angle;
        }

        // Calculate the new position of the cast point around the parent
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * 1.5f;
        spellManager.castPoint.localPosition = offset;

        // Rotate the cast point to face the direction of the stick input
        spellManager.castPoint.localRotation = Quaternion.Euler(0, 0, angle);
    }
    private void HandleAimingUp() {
        if (spellManager.currentSpellInstance?.CanAim == true) {
            float aimDirection = gameInput.GetAimDirectionPlatformer();  // Gets -1, 0, 1
            int rotationDirection = movementManager.isFacingRight ? 1 : -1;

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

    private void HandleDeath() {
        Destroy(gameObject);
    }
}