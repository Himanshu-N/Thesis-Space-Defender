using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject[] asteroidPrefabs; // Added 's' and [] brackets to make it a list!

    [Header("Spawn Area Setup")]
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 0f);

    [Header("Wave Settings")]
    public int totalWaves = 3;
    private int currentWave = 0;

    [Header("Fixed Timings")]
    public float waveDuration = 120f;
    public float breakDuration = 10f;

    [Header("Difficulty Scaling")]
    public float initialSpawnRate = 1.2f; // Seconds between spawns on Wave 1
    public float spawnRateDecreasePerWave = 0.3f; // Subtract this each wave (makes them spawn faster)
    public float rockSpeedIncreasePerWave = 0.4f; // Adds speed to rocks each wave

    private float currentSpawnRate;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(WaveCycleRoutine());
    }

    IEnumerator WaveCycleRoutine()
    {
        while (GameManager.Instance == null || !GameManager.Instance.isLevelActive) yield return null;
        yield return new WaitForSeconds(2f);

        while (GameManager.Instance != null && GameManager.Instance.isLevelActive && currentWave < totalWaves)
        {
            currentWave++;
            GameManager.Instance.UpdateWaveUI(currentWave, totalWaves);

            currentSpawnRate = initialSpawnRate - ((currentWave - 1) * spawnRateDecreasePerWave);
            if (currentSpawnRate < 0.2f) currentSpawnRate = 0.2f;

            GameManager.Instance.currentRockSpeedMultiplier = 1.0f + ((currentWave - 1) * rockSpeedIncreasePerWave);

            // 1. ANNOUNCE WAVE
            GameManager.Instance.ShowAnnouncer("WAVE " + currentWave + " INCOMING");
            yield return new WaitForSeconds(2f);
            GameManager.Instance.HideAnnouncer();

            // 2. SPAWNING PHASE 
            GameManager.Instance.SetTimerSubText("Rocks\nGenerating");
            float phaseTimer = waveDuration;
            isSpawning = true;
            StartCoroutine(SpawnAsteroidsRoutine());

            while (phaseTimer > 0 && GameManager.Instance.isLevelActive)
            {
                phaseTimer -= Time.deltaTime;
                if (phaseTimer < 0) phaseTimer = 0;
                GameManager.Instance.UpdateDashboardTimer(phaseTimer);
                yield return null;
            }
            isSpawning = false;

            // 3. CLEANUP PHASE 
            GameManager.Instance.SetTimerSubText("Debris\nCleanup");
            float maxCleanupTime = 15f;

            while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0 && GameManager.Instance.isLevelActive && maxCleanupTime > 0)
            {
                maxCleanupTime -= Time.deltaTime;
                if (maxCleanupTime < 0) maxCleanupTime = 0;

                GameManager.Instance.UpdateDashboardTimer(maxCleanupTime);
                yield return null;
            }

            // FORCE CLEANUP
            GameObject[] missedRocks = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject rock in missedRocks) Destroy(rock);

            if (GameManager.Instance.isLevelActive)
            {
                GameManager.Instance.ShowAnnouncer("WAVE SURVIVED!");
                yield return new WaitForSeconds(1.5f);
            }

            // 4. BREAK PHASE 
            if (currentWave < totalWaves && GameManager.Instance.isLevelActive)
            {
                // --- CHANGED: Dashboard locked to Dashes and "Rocks Generating" ---
                GameManager.Instance.SetTimerSubText("Rocks\nGenerating");
                GameManager.Instance.ShowDashboardDashes();

                int countdown = Mathf.CeilToInt(breakDuration);

                while (countdown > 0 && GameManager.Instance.isLevelActive)
                {
                    // The countdown now ONLY happens on the center announcer
                    GameManager.Instance.ShowAnnouncer("NEXT WAVE IN:\n" + countdown);
                    GameManager.Instance.PlayCountdownTick();
                    yield return new WaitForSeconds(1f);
                    countdown--;
                }
                GameManager.Instance.HideAnnouncer();
            }
        }

        // 5. VICTORY (No health check, you always survive)
        if (GameManager.Instance.isLevelActive)
        {
            GameManager.Instance.SetTimerSubText("");
            GameManager.Instance.ShowDashboardDashes();

            GameManager.Instance.ShowAnnouncer("ALL WAVES CLEARED!");
            yield return new WaitForSeconds(3f);
            GameManager.Instance.LevelComplete();
        }
    }
    IEnumerator SpawnAsteroidsRoutine()
    {
        while (isSpawning)
        {
            SpawnSingleAsteroid();
            yield return new WaitForSeconds(currentSpawnRate); // Uses the new scaling rate
        }
    }

    void SpawnSingleAsteroid()
    {
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0) return;

        // Pick a random rock from your list!
        int randomIndex = Random.Range(0, asteroidPrefabs.Length);
        GameObject prefabToSpawn = asteroidPrefabs[randomIndex];

        Vector3 randomPos = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        Vector3 finalSpawnPos = transform.position + randomPos;
        Instantiate(prefabToSpawn, finalSpawnPos, Random.rotation);
    }
}