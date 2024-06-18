using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance;

    public enum MovementControls { Platformer, TopDown, Runner, Enemy }
    public MovementControls movementControls;


    public RoomGenerator roomGenerator;
    public int minWidth = 5;
    public int maxWidth = 15;
    public int minHeight = 5;
    public int maxHeight = 10;
    public int wallThickness = 1;

    public int maxRooms = 3;
    private int currentRoomCount = 0;
    private Vector3Int currentRoomPosition = new Vector3Int(0, 0, 0);

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
        roomGenerator = FindObjectOfType<RoomGenerator>();
        InitializeLevel();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            InitializeLevel();
        }
    }

    private void InitializeLevel() {
        roomGenerator.ClearTiles();
        currentRoomCount = 0;
        currentRoomPosition = new Vector3Int(0, 0, 0);
        GenerateNextRoom();
    }

    private void GenerateNextRoom() {
        if (currentRoomCount >= maxRooms)
            return;

        roomGenerator.InitializeRoomSettings(minWidth, maxWidth, minHeight, maxHeight, wallThickness);
        roomGenerator.GenerateRoom(currentRoomPosition);

        if (currentRoomCount > 0 && currentRoomCount < maxRooms - 1) {
            roomGenerator.CreateDoor(currentRoomPosition);
        }

        if (currentRoomCount > 0) {
            currentRoomPosition += new Vector3Int(roomGenerator.roomWidth + roomGenerator.wallThickness * 2, 0, 0);
        }

        currentRoomCount++;
        GenerateNextRoom();
    }

    private Vector3Int GetNextRoomPosition(Vector3Int doorPosition) {
        // Position the next room to the right of the current one
        return new Vector3Int(doorPosition.x + roomGenerator.wallThickness, currentRoomPosition.y, 0);
    }

    public void IncreaseScore(int amount) {
        score += amount;
        // Adjust difficulty based on score or other criteria
    }
}