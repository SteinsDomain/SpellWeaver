using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int score;
    public int difficultyLevel;
    public bool isGameRunning;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        StartGame();
    }

    public void StartGame() {
        score = 0;
        difficultyLevel = 1;
        isGameRunning = true;
        // Initialize other components or managers if necessary
    }

    public void EndGame() {
        isGameRunning = false;
        // Handle end game logic, like showing end screen, saving score, etc.
    }

    public void RestartGame() {
        // Reset necessary variables and restart the game
        StartGame();
    }

    public void IncreaseScore(int amount) {
        score += amount;
        // Adjust difficulty based on score or other criteria
    }
}