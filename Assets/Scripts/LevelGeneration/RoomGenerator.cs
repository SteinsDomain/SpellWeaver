using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour {

    [Header("Level Dimensions")]
    public int roomWidth = 100;  // Width of the level
    public int roomHeight = 50; // Height of the level

    [Header("Tilemap and Tile Settings")]
    public Tilemap tilemap;  // Reference to your Tilemap
    public RuleTile groundTile;  // Reference to your RuleTile

    [Header("Ceiling Settings")]
    public bool hasCeiling = true;  // Whether to generate a ceiling
    public int ceilingHeight = 8;  // Distance from ground level to ceiling

    [Header("End Cap Settings")]
    public bool capEnds = true;  // Whether to cap the ends

    [Header("Ground Level Settings")]
    public int startGroundLevel = 20;  // Fixed starting ground level
    public float groundVariation = 8f;  // Maximum variation up or down for the ground level
    public float smoothness = 0.1f;  // Smoothness factor for Perlin noise

    private bool[,] layout;
    private int[] groundLevels;
    private float seed;

    private void Start() {
        if (Application.isPlaying) {
            // Prevent generating a room on Start during gameplay
            return;
        }
        GenerateRoom(0, 0);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            ClearLevel();
            GenerateRoom(0, 0);
        }
    }

    public void GenerateRoom(int xOffset, int yOffset) {
        InitializeRoom();
        GenerateGroundLevels();
        FillGroundTiles();

        if (hasCeiling) {
            AddCeiling();
        }
        

        // Cap the ends if enabled
        if (capEnds) {
            CapEnds();
        }

        ApplyLayoutToTilemap(layout, xOffset, yOffset);
    }

    private void InitializeRoom() {
        // Update seed for Perlin noise to create variation
        seed = Random.Range(0f, 100f);

        layout = new bool[roomWidth, roomHeight];
        groundLevels = new int[roomWidth];

        groundLevels[0] = startGroundLevel;
        groundLevels[roomWidth - 1] = startGroundLevel;
    }
    private void GenerateGroundLevels() {
        for (int x = 1; x < roomWidth - 1; x++) {
            // Use Perlin noise to generate smooth variation in ground level
            float noiseValue = Mathf.PerlinNoise(x * smoothness, seed);
            int minGroundLevel = Mathf.Clamp(startGroundLevel - (int)groundVariation, 0, roomHeight - 1);
            int maxGroundLevel = Mathf.Clamp(startGroundLevel + (int)groundVariation, 0, roomHeight - 1);
            groundLevels[x] = (int)Mathf.Lerp(minGroundLevel, maxGroundLevel, noiseValue);
        }
    }
    private void FillGroundTiles() {
        for (int x = 0; x < roomWidth; x++) {
            int groundLevel = groundLevels[x];
            for (int y = 0; y <= groundLevel; y++) {
                layout[x, y] = true;
            }
        }
    }
    private void AddCeiling() {
        for (int x = 0; x < roomWidth; x++) {
            int groundLevel = groundLevels[x];
            int ceilingY = Mathf.Clamp(groundLevel + ceilingHeight, 0, roomHeight - 1);
            for (int y = ceilingY; y < roomHeight; y++) {
                layout[x, y] = true;
            }
        }
    }
    private void CapEnds() {
        for (int y = 0; y < roomHeight; y++) {
            layout[0, y] = true;  // Left end
            layout[roomWidth - 1, y] = true;  // Right end
        }
    }

    private void ApplyLayoutToTilemap(bool[,] layout, int xOffset, int yOffset) {
        for (int x = 0; x < layout.GetLength(0); x++) {
            for (int y = 0; y < layout.GetLength(1); y++) {
                Vector3Int tilePosition = new Vector3Int(x + xOffset, y + yOffset, 0);
                if (layout[x, y]) {
                    tilemap.SetTile(tilePosition, groundTile);
                }
                else {
                    tilemap.SetTile(tilePosition, null);  // Clear tile
                }
            }
        }
    }

    private void ClearLevel() {
        tilemap.ClearAllTiles();
    }

    public int GetEndGroundLevel() {
        return groundLevels[roomWidth - 1];
    }

    private void OnDrawGizmos() {
        // Draw the boundaries of the level
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(roomWidth / 2, roomHeight / 2, 0), new Vector3(roomWidth, roomHeight, 0));

        // Draw the starting ground point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(0, startGroundLevel, 0), 0.5f);

        // Draw the ending ground point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(roomWidth - 1, startGroundLevel, 0), 0.5f);
    }
}
