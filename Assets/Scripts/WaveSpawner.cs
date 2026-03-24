using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject asteroidPrefab;

    [Header("Spawn Area Setup")]
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 0f);

    [Header("Wave Settings")]
    public int totalWaves = 4;
    private int currentWave = 0;

    [Header("Fixed Timings")]
    public float waveDuration = 25f;
    public float breakDuration = 10f;
    public float spawnRate = 1.0f;

    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(WaveCycleRoutine());
    }

    IEnumerator WaveCycleRoutine()
    {
        // 0. Wait for player to pull the trigger
        while (GameManager.Instance == null || !GameManager.Instance.isLevelActive)
        {
            yield return null;
        }

        // --- FIX 1: Wait 2 seconds before violently starting Wave 1 ---
        yield return new WaitForSeconds(2f);

        // Main Loop
        while (GameManager.Instance != null && GameManager.Instance.isLevelActive && currentWave < totalWaves)
        {
            currentWave++;
            GameManager.Instance.UpdateWaveUI(currentWave, totalWaves);

            // 1. ANNOUNCE WAVE
            GameManager.Instance.ShowAnnouncer("WAVE " + currentWave + " INCOMING");
            yield return new WaitForSeconds(2f);
            GameManager.Instance.HideAnnouncer();

            // 2. SPAWNING PHASE (Timer counts down on Dashboard)
            float phaseTimer = waveDuration;
            isSpawning = true;
            StartCoroutine(SpawnAsteroidsRoutine());

            // Prevent timer from going negative by making sure it stops exactly at 0
            while (phaseTimer > 0 && GameManager.Instance.isLevelActive)
            {
                phaseTimer -= Time.deltaTime;
                if (phaseTimer < 0) phaseTimer = 0;
                GameManager.Instance.UpdateDashboardTimer(phaseTimer);
                yield return null;
            }
            isSpawning = false; // Stop creating new rocks

            // 3. CLEANUP PHASE 
            // --- FIX 2: Set dashboard to dashes during cleanup & breaks ---
            GameManager.Instance.ShowDashboardDashes();

            // Wait silently until exactly 0 objects with the "Enemy" tag exist
            while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0 && GameManager.Instance.isLevelActive)
            {
                yield return new WaitForSeconds(0.5f);
            }

            // --- FIX 3: Announce survival AFTER rocks are destroyed ---
            if (GameManager.Instance.isLevelActive)
            {
                GameManager.Instance.ShowAnnouncer("WAVE SURVIVED!");
                yield return new WaitForSeconds(1.5f);
            }

            // 4. BREAK PHASE (If it's not the final wave)
            if (currentWave < totalWaves && GameManager.Instance.isLevelActive)
            {
                // --- FIX 4: Clean integer countdown loop with Audio ticks ---
                int countdown = Mathf.CeilToInt(breakDuration);

                while (countdown > 0 && GameManager.Instance.isLevelActive)
                {
                    GameManager.Instance.ShowAnnouncer("NEXT WAVE IN:\n" + countdown);
                    GameManager.Instance.PlayCountdownTick(); // Plays the beep

                    yield return new WaitForSeconds(1f); // Wait exactly 1 second
                    countdown--;
                }
                GameManager.Instance.HideAnnouncer();
            }
        }

        // 5. VICTORY SEQUENCE
        if (GameManager.Instance.currentHealth > 0 && GameManager.Instance.isLevelActive)
        {
            GameManager.Instance.ShowDashboardDashes();
            GameManager.Instance.ShowAnnouncer("ALL WAVES CLEARED!");
            yield return new WaitForSeconds(3f);
            GameManager.Instance.LevelComplete(true);
        }
    }

    IEnumerator SpawnAsteroidsRoutine()
    {
        while (isSpawning)
        {
            SpawnSingleAsteroid();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    void SpawnSingleAsteroid()
    {
        if (asteroidPrefab == null) return;
        Vector3 randomPos = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        Vector3 finalSpawnPos = transform.position + randomPos;
        Instantiate(asteroidPrefab, finalSpawnPos, Random.rotation);
    }
}