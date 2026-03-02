using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;
    public float spawnRate = 2f;

    [Header("Spawn Area")]
    // Since you are looking at -Z, we spawn them deep at -Z (e.g. -60)
    public float spawnDistanceZ = -60f;
    public float spreadX = 20f; // How wide left/right they appear
    public float spreadY = 10f; // How high up/down they appear

    void Start()
    {
        InvokeRepeating("SpawnAsteroid", 1f, spawnRate);
    }

    void SpawnAsteroid()
    {
        if (asteroidPrefab == null) return;

        // 1. Calculate a random position IN FRONT of the player
        // We use a fixed Z (depth) and random X/Y to make a "wall" of incoming rocks
        Vector3 spawnPos = new Vector3(
            Random.Range(-spreadX, spreadX), // Random Left/Right
            Random.Range(-spreadY, spreadY), // Random Up/Down
            spawnDistanceZ                   // Fixed distance in front
        );

        // 2. Spawn the asteroid
        Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
    }
}