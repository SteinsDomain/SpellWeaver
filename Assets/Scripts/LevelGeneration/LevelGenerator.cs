using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    public int maxRoomsPerFloor = 3;  // Maximum number of rooms per floor
    public int minimumRoomsPerFloor = 1;  // Minimum number of rooms per floor
    public int numberOfFloors = 3;
    public GameObject roomGenerator;  // Prefab of the room containing RoomGenerator script

    private Tilemap tilemap;
    private List<int> previousFloorRoomPositions;  // X positions of rooms on the previous floor
    private int currentYOffset;

    private void Start() {
        InitializeLevelGenerator();
        GenerateLevel();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetLevel();
        }
    }

    private void InitializeLevelGenerator() {
        tilemap = FindObjectOfType<Tilemap>();
        if (tilemap == null) {
            Debug.LogError("No Tilemap found in the scene.");
            return;
        }
        previousFloorRoomPositions = new List<int> { 0 };
        currentYOffset = 0;
    }

    private void GenerateLevel() {
        for (int floor = 0; floor < numberOfFloors; floor++) {
            GenerateFloor(floor);
        }
    }

    private void GenerateFloor(int floor) {
        List<RoomGenerator> currentFloorRooms = new List<RoomGenerator>();
        int roomsThisFloor = Random.Range(minimumRoomsPerFloor, maxRoomsPerFloor + 1);
        int currentXOffset = floor == 0 ? 0 : GetRandomStartingPosition();

        for (int i = 0; i < roomsThisFloor; i++) {
            RoomGenerator roomGenerator = CreateRoom(currentXOffset, currentYOffset);
            currentFloorRooms.Add(roomGenerator);
            currentXOffset += roomGenerator.roomWidth;
        }

        UpdateForNextFloor(currentFloorRooms);
    }

    private RoomGenerator CreateRoom(int xPosition, int yPosition) {
        Vector3 roomPosition = new Vector3(xPosition, yPosition, 0);
        GameObject roomGeneratorInstance = Instantiate(roomGenerator, roomPosition, Quaternion.identity, transform);
        RoomGenerator roomGen = roomGeneratorInstance.GetComponent<RoomGenerator>();
        roomGen.tilemap = tilemap;
        roomGen.GenerateRoom(xPosition, yPosition);
        previousFloorRoomPositions.Add(xPosition);
        return roomGen;
    }

    private int GetRandomStartingPosition() {
        return previousFloorRoomPositions[Random.Range(0, previousFloorRoomPositions.Count)];
    }

    private void UpdateForNextFloor(List<RoomGenerator> currentFloorRooms) {
        if (currentFloorRooms.Count > 0) {
            currentYOffset += currentFloorRooms[0].roomHeight;
        }
    }

    private void ResetLevel() {
        ClearLevel();
        GenerateLevel();
    }

    private void ClearLevel() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        if (tilemap != null) {
            tilemap.ClearAllTiles();
        }
    }
}
