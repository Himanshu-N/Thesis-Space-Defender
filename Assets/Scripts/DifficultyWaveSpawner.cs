using System.Collections;
using UnityEngine;

public class DifficultyWaveSpawner : MonoBehaviour
{
    public enum DifficultyMode { Easy, Medium, Hard }

    [Header("Level Settings")]
    public DifficultyMode currentDifficulty;

    [Tooltip("Add as many prefabs as you want. The game will pick one randomly each time!")]
    public GameObject[] asteroidPrefabs;

    [Header("Spawn Area Setup")]
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 0f);

    [Header("Wave Progression")]
    public int totalWaves = 5; // You can make these levels shorter or longer than the assessment
    public float waveDuration = 60f;
    public float breakDuration = 10f;
    public float cleanupDuration = 15f;

    [Header("Testing Fallbacks (If No Profile Found)")]
    public float fallbackSpeed = 25f;
    public float fallbackSpawnRate = 2f;

    // Private tracking variables
    private int currentWave = 0;
    private float finalCalculatedSpawnRate;
    private float finalCalculatedSpeed;
    private bool isSpawning = false;

    void Start()
    {
        CalculatePersonalizedDifficulty();
        StartCoroutine(WaveCycleRoutine());
    }

    void CalculatePersonalizedDifficulty()
    {
        float baseSpeed = fallbackSpeed;
        float baseSpawn = fallbackSpawnRate;

        // 1. Check if the participant has a saved baseline
        if (ParticipantManager.Instance != null &&
            ParticipantManager.Instance.currentProfile != null &&
            ParticipantManager.Instance.currentProfile.hasCompletedAssessment)
        {
            baseSpeed = ParticipantManager.Instance.currentProfile.baselineSpeed;
            baseSpawn = ParticipantManager.Instance.currentProfile.baselineSpawnRate;
            Debug.Log($"<color=cyan>Loaded Baseline for {ParticipantManager.Instance.currentProfile.participantID}:</color> Speed {baseSpeed:F2}, Spawn {baseSpawn:F2}");
        }
        else
        {
            Debug.LogWarning("No Assessment Baseline found! Using fallback Inspector values.");
        }

        // 2. Apply the +/- 15% Difficulty Multipliers
        switch (currentDifficulty)
        {
            case DifficultyMode.Easy:
                finalCalculatedSpeed = baseSpeed * 0.85f;      // 15% slower
                finalCalculatedSpawnRate = baseSpawn * 1.15f;  // 15% more time between rocks
                break;

            case DifficultyMode.Medium:
                finalCalculatedSpeed = baseSpeed;              // Exactly the baseline
                finalCalculatedSpawnRate = baseSpawn;          // Exactly the baseline
                break;

            case DifficultyMode.Hard:
                finalCalculatedSpeed = baseSpeed * 1.15f;      // 15% faster
                finalCalculatedSpawnRate = baseSpawn * 0.85f;  // 15% less time between rocks
                break;
        }

        Debug.Log($"<color=green>Level Set to {currentDifficulty}:</color> Final Speed {finalCalculatedSpeed:F2}, Final Spawn {finalCalculatedSpawnRate:F2}");
    }

    IEnumerator WaveCycleRoutine()
    {
        while (GameManager.Instance == null || !GameManager.Instance.isLevelActive) yield return null;
        yield return new WaitForSeconds(2f);

        while (GameManager.Instance != null && GameManager.Instance.isLevelActive && currentWave < totalWaves)
        {
            currentWave++;
            GameManager.Instance.UpdateWaveUI(currentWave, totalWaves);

            GameManager.Instance.ResetWaveScore();

            // Push the calculated speed to the GameManager so rocks can read it!
            GameManager.Instance.currentRockSpeed = finalCalculatedSpeed;

            GameManager.Instance.ShowAnnouncer(currentDifficulty.ToString().ToUpper() + " LEVEL - WAVE " + currentWave);
            yield return new WaitForSeconds(2f);
            GameManager.Instance.HideAnnouncer();

            // SPAWNING PHASE
            GameManager.Instance.SetTimerSubText("Rocks Generating");
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

            // CLEANUP PHASE
            GameManager.Instance.SetTimerSubText("Debris Cleanup");
            float cleanupTimer = cleanupDuration;

            while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0 && GameManager.Instance.isLevelActive && cleanupTimer > 0)
            {
                cleanupTimer -= Time.deltaTime;
                if (cleanupTimer < 0) cleanupTimer = 0;
                GameManager.Instance.UpdateDashboardTimer(cleanupTimer);
                yield return null;
            }

            GameObject[] missedRocks = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject rock in missedRocks) Destroy(rock);

            // BREAK PHASE
            if (currentWave < totalWaves && GameManager.Instance.isLevelActive)
            {
                GameManager.Instance.SetTimerSubText("Calculating Next Wave...");
                GameManager.Instance.ShowDashboardDashes();

                int countdown = Mathf.CeilToInt(breakDuration);
                while (countdown > 0 && GameManager.Instance.isLevelActive)
                {
                    GameManager.Instance.ShowAnnouncer("NEXT WAVE IN:\n" + countdown);
                    GameManager.Instance.PlayCountdownTick();
                    yield return new WaitForSeconds(1f);
                    countdown--;
                }
                GameManager.Instance.HideAnnouncer();
            }
        }

        // LEVEL COMPLETE
        if (GameManager.Instance.isLevelActive)
        {
            // Mark this specific difficulty as completed in the profile!
            if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
            {
                if (currentDifficulty == DifficultyMode.Easy) ParticipantManager.Instance.currentProfile.hasCompletedEasy = true;
                if (currentDifficulty == DifficultyMode.Medium) ParticipantManager.Instance.currentProfile.hasCompletedMedium = true;
                if (currentDifficulty == DifficultyMode.Hard) ParticipantManager.Instance.currentProfile.hasCompletedHard = true;
                ParticipantManager.Instance.SaveProfile();
            }

            GameManager.Instance.SetTimerSubText("");
            GameManager.Instance.ShowDashboardDashes();
            GameManager.Instance.ShowAnnouncer("LEVEL COMPLETE\n<size=50%>GREAT JOB</size>");
            yield return new WaitForSeconds(4f);
            GameManager.Instance.LevelComplete();
        }
    }

    IEnumerator SpawnAsteroidsRoutine()
    {
        while (isSpawning)
        {
            SpawnSingleAsteroid();
            yield return new WaitForSeconds(finalCalculatedSpawnRate);
        }
    }

    void SpawnSingleAsteroid()
    {
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0) return;

        // --- NEW: Picks a random prefab from your list! ---
        int randomIndex = Random.Range(0, asteroidPrefabs.Length);
        GameObject selectedPrefab = asteroidPrefabs[randomIndex];

        Vector3 randomPos = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        Vector3 finalSpawnPos = transform.position + randomPos;
        Instantiate(selectedPrefab, finalSpawnPos, Random.rotation);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // Green box for gameplay levels
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}