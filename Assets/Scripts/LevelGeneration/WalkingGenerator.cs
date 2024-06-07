using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkingGenerator : MonoBehaviour {
    public Tilemap tilemap; // Reference to the Tilemap
    public RuleTile obstacleTile; // Reference to the RuleTile

    private Vector3Int walkerPosition; // Position of the walker in grid coordinates
    public Vector2Int walkerSize = new Vector2Int(2, 2); // Size of the walker (2x2)
    private HashSet<Vector3Int> clearedPositions = new HashSet<Vector3Int>(); // Track cleared positions
    private HashSet<Vector3Int> visitedPositions = new HashSet<Vector3Int>(); // Track visited positions

    public float stepDelay = 0.5f; // Time delay between steps
    public float directionChangeInterval = 2f; // Time interval for changing direction
    private float directionChangeTimer = 0f;

    private Vector3Int currentDirection; // Current movement direction
    private Coroutine walkCoroutine; // Reference to the walking coroutine

    void Start() {
        // Initialize walker position (example start position)
        walkerPosition = new Vector3Int(0, 0, 0);

        // Initialize direction to move right initially
        currentDirection = new Vector3Int(walkerSize.x, 0, 0);

        // Mark the initial position as visited
        visitedPositions.Add(walkerPosition);

        // Call the function to start the pattern movement
        walkCoroutine = StartCoroutine(WalkPattern());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            StopAndResetWalker();
        }
    }

    IEnumerator WalkPattern() {
        while (true) {
            PlaceTilesAroundWalker();

            // Update direction change timer
            directionChangeTimer += stepDelay;

            // Change direction if the timer exceeds the interval
            if (directionChangeTimer >= directionChangeInterval) {
                ChangeDirection();
                directionChangeTimer = 0f;
            }

            // Move the walker in the current direction
            walkerPosition += currentDirection;
            visitedPositions.Add(walkerPosition);

            // Wait for a short time before the next step (to visualize the pattern)
            yield return new WaitForSeconds(stepDelay);
        }
    }

    void PlaceTilesAroundWalker() {
        // Define the area around the walker
        for (int x = -1; x <= walkerSize.x; x++) {
            for (int y = -1; y <= walkerSize.y; y++) {
                Vector3Int tilePosition = new Vector3Int(walkerPosition.x + x, walkerPosition.y + y, 0);

                // Place obstacle tiles around the walker
                if (x == -1 || x == walkerSize.x || y == -1 || y == walkerSize.y) {
                    if (!clearedPositions.Contains(tilePosition)) {
                        tilemap.SetTile(tilePosition, obstacleTile);
                    }
                }
                else {
                    // Clear the tiles within the walker's 2x2 space
                    tilemap.SetTile(tilePosition, null);
                    clearedPositions.Add(tilePosition);
                }
            }
        }
    }

    void ChangeDirection() {
        // Possible directions (right, left, up, down)
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(walkerSize.x, 0, 0),   // Move right
            new Vector3Int(-walkerSize.x, 0, 0),  // Move left
            new Vector3Int(0, walkerSize.y, 0),   // Move up
            new Vector3Int(0, -walkerSize.y, 0)   // Move down
        };

        // Try to find a valid direction that hasn't been visited recently
        List<Vector3Int> validDirections = new List<Vector3Int>();
        foreach (var direction in directions) {
            Vector3Int newPosition = walkerPosition + direction;
            if (!visitedPositions.Contains(newPosition)) {
                validDirections.Add(direction);
            }
        }

        // If there are valid directions, choose one randomly
        if (validDirections.Count > 0) {
            currentDirection = validDirections[Random.Range(0, validDirections.Count)];
        }
        else {
            // If all directions have been visited, choose any direction (avoid getting stuck)
            currentDirection = directions[Random.Range(0, directions.Length)];
        }
    }

    void StopAndResetWalker() {
        // Stop the walking coroutine
        if (walkCoroutine != null) {
            StopCoroutine(walkCoroutine);
        }

        // Clear the tilemap
        tilemap.ClearAllTiles();

        // Reset the walker position
        walkerPosition = new Vector3Int(0, 0, 0);

        // Clear the cleared positions and visited positions
        clearedPositions.Clear();
        visitedPositions.Clear();

        // Mark the initial position as visited
        visitedPositions.Add(walkerPosition);

        // Restart the pattern movement
        walkCoroutine = StartCoroutine(WalkPattern());
    }
}