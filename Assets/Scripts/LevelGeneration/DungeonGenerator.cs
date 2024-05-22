using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs;  // Array to store room prefabs
    public int numberOfRooms = 5;      // Number of rooms to generate
    public Vector2Int levelSize = new Vector2Int(5, 1);  // Grid size of the level
    public Tilemap mainTilemap;

    void Start() {
        GenerateLevel();
    }

    void GenerateLevel() {
        for (int i = 0; i < numberOfRooms; i++) {
            Vector3Int position = new Vector3Int(i * 10, 0, 0);  // Calculate position for each room as a Tilemap position
            GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            AddRoomToTilemap(roomPrefab, position);
        }
    }

    void AddRoomToTilemap(GameObject roomPrefab, Vector3Int position) {
        // Get the Tilemap from the prefab
        Tilemap roomTilemap = roomPrefab.GetComponentInChildren<Tilemap>();

        if (roomTilemap == null) {
            Debug.LogError("No Tilemap found in room prefab.");
            return;
        }

        // Copy each tile from the room's Tilemap to the main Tilemap
        foreach (var positionInRoom in roomTilemap.cellBounds.allPositionsWithin) {
            Vector3Int tilePosition = positionInRoom + position;
            TileBase tile = roomTilemap.GetTile(positionInRoom);
            if (tile != null) {
                mainTilemap.SetTile(tilePosition, tile);
            }
        }
    }
}
