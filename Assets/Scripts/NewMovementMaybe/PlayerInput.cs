using UnityEngine;

[RequireComponent(typeof(PlayerVelocity))]
public class PlayerInput : MonoBehaviour {

    private GameInput gameInput;
    private PlayerVelocity playerVelocity;

    void Start() {
        playerVelocity = GetComponent<PlayerVelocity>();
        if (gameInput == null) gameInput = FindAnyObjectByType<GameInput>();
    }

    void Update() {
        Vector2 directionalInput = gameInput.GetMovementDirection();
        playerVelocity.SetDirectionalInput(directionalInput);

        if (gameInput.JumpPressed()) {
            playerVelocity.OnJumpInputDown();
        }
        if (gameInput.JumpReleased()) {
            playerVelocity.OnJumpInputUp();
        }
        if (gameInput.GetMovementDirection().y < 0) {
            playerVelocity.OnFallInputDown();
        }
    }
}
