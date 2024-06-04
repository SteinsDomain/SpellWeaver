using UnityEngine;

public class Spawner : MonoBehaviour {
    public EnemySquad[] enemySquads;
    public Transform spawnPoint;
    public float spawnInterval = 2f;
    private float timeSinceLastSpawn;

    private void Update() {
        if (GameManager.Instance.isGameRunning) {
            timeSinceLastSpawn += Time.deltaTime;

            if (timeSinceLastSpawn >= spawnInterval) {
                SpawnSquad();
                timeSinceLastSpawn = 0f;
            }
        }
    }

    private void SpawnSquad() {
        // Select a random squad to spawn
        EnemySquad squadToSpawn = enemySquads[Random.Range(0, enemySquads.Length)];

        // Instantiate each enemy in the squad
        foreach (GameObject enemyPrefab in squadToSpawn.enemies) {
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    public void IncreaseSpawnRate() {
        // Logic to increase spawn rate as difficulty increases
        spawnInterval = Mathf.Max(0.5f, spawnInterval - 0.1f);
    }
}