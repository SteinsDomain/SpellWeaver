using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour {
    public Tilemap tilemap;
    public RuleTile groundTile;

    public int minWidth = 5;
    public int maxWidth = 15;
    public int minHeight = 5;
    public int maxHeight = 10;
    public int wallThickness = 1;

    public int roomWidth;
    public int roomHeight;

    public void InitializeRoomSettings(int minW, int maxW, int minH, int maxH, int thickness) {
        minWidth = minW;
        maxWidth = maxW;
        minHeight = minH;
        maxHeight = maxH;
        wallThickness = thickness;
        GenerateRandomRoomSize();
    }

    private void GenerateRandomRoomSize() {
        roomWidth = Random.Range(minWidth, maxWidth + 1);
        roomHeight = Random.Range(minHeight, maxHeight + 1);
    }

    public void ClearTiles() {
        tilemap.ClearAllTiles();
    }

    public void GenerateRoom(Vector3Int startPosition) {
        // Calculate the outer bounds of the room including walls
        int outerWidth = roomWidth + 2 * wallThickness;
        int outerHeight = roomHeight + 2 * wallThickness;

        // Generate walls
        for (int x = 0; x < outerWidth; x++) {
            for (int y = 0; y < outerHeight; y++) {
                // Check if the current position is within the walls
                bool isWall = x < wallThickness || x >= outerWidth - wallThickness || y < wallThickness || y >= outerHeight - wallThickness;
                Vector3Int tilePosition = startPosition + new Vector3Int(x, y, 0);

                if (isWall) {
                    // Set wall tile
                    tilemap.SetTile(tilePosition, groundTile);
                }
                else {
                    // Ensure the floor tiles are empty
                    tilemap.SetTile(tilePosition, null);
                }
            }
        }
    }

    public void CreateDoor(Vector3Int roomStartPosition, int doorHeight = 3) {
        int doorYStart = wallThickness; // Ensure the door starts at the ground level
        Vector3Int doorPosition = new Vector3Int(roomWidth + wallThickness - 1, doorYStart, 0);

        // Dig out the door in the current room's right wall
        for (int i = 0; i < doorHeight; i++) {
            tilemap.SetTile(roomStartPosition + doorPosition + new Vector3Int(0, i, 0), null);
            tilemap.SetTile(roomStartPosition + doorPosition + new Vector3Int(1, i, 0), null);
            tilemap.SetTile(roomStartPosition + doorPosition + new Vector3Int(2, i, 0), null);
        }
    }
}