using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour {

    private PlayerInputActions playerInputActions;

    private void Awake() {
        playerInputActions = new PlayerInputActions(); // Instantiates the player input actions.
        playerInputActions.Player.Enable(); // Enables the Player action map, allowing it to start receiving input.
    }

    public Vector2 GetMovementDirection() {
         return playerInputActions.Player.Move.ReadValue<Vector2>();
    }
    public float GetAimDirection(){
        Vector2 aimDirection = playerInputActions.Player.Move.ReadValue<Vector2>();
        if(aimDirection.y > 0) {
            return 1;
        } else if(aimDirection.y < 0) {
            return -1;
        } else {
            return 0;
        }
    }
    public bool JumpPressed() {
        return playerInputActions.Player.Jump.WasPressedThisFrame();
    }
    public bool JumpHeld() {
        return playerInputActions.Player.Jump.IsPressed();
    }
    public bool JumpReleased() {
        return playerInputActions.Player.Jump.WasReleasedThisFrame();
    }
    public bool DashPressed() {
        return playerInputActions.Player.Dash.WasPressedThisFrame();
    }
    public bool AttackPressed() {
        return playerInputActions.Player.Attack.WasPressedThisFrame();
    }
    public bool InteractPressed() {
        return playerInputActions.Player.Interact.WasPressedThisFrame();
    }
    public bool CastPressed() {
        return playerInputActions.Player.Cast.WasPressedThisFrame();
    }
    public bool CastHeld() {
        return playerInputActions.Player.Cast.IsPressed();
    }
    public bool CastReleased() {
        return playerInputActions.Player.Cast.WasReleasedThisFrame();
    }
    public Vector2 GetElementMenuDirection() {
        return playerInputActions.Player.ElementMenuNavigation.ReadValue<Vector2>();
    }
    public bool ElementMenuPressed() {
        return playerInputActions.Player.ElementMenu.WasPressedThisFrame();
    }
    public bool ElementMenuHeld() {
        return playerInputActions.Player.ElementMenu.IsPressed();
    }
    public bool ElementMenuReleased() {
        return playerInputActions.Player.ElementMenu.WasReleasedThisFrame();
    }
    public Vector2 GetSchoolMenuDirection() {
        return playerInputActions.Player.SchoolMenuNavigation.ReadValue<Vector2>();
    }
    public bool SchoolMenuPressed() {
        return playerInputActions.Player.SchoolMenu.WasPressedThisFrame();
    }
    public bool SchoolMenuHeld() {
        return playerInputActions.Player.SchoolMenu.IsPressed();
    }
    public bool SchoolMenuReleased() {
        return playerInputActions.Player.SchoolMenu.WasReleasedThisFrame();
    }
}
