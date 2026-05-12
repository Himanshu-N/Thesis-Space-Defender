using System.Collections;
using UnityEngine;

// --- NEW: Custom class to define rock weights ---
[System.Serializable]
public class AsteroidVariant
{
    public GameObject prefab;
    [Tooltip("How many hits/spawns this rock counts as. (Normal = 1, Red = 2)")]
    public int spawnWeightValue = 1;
}

public class DifficultyWaveSpawner : MonoBehaviour
{
    public enum DifficultyMode { Easy, Medium, Hard }

    [Header("Level Settings")]
    public DifficultyMode currentDifficulty;

    // --- CHANGED: Now uses our custom list instead of a basic array ---
    public AsteroidVariant[] asteroidVariants;

    [Header("Spawn Area Setup")]
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 0f);

    [Header("Wave Progression")]
    public int totalWaves = 5;
    public float waveDuration = 60f;
    public float breakDuration = 10f;
    public float cleanupDuration = 15f;

    [Header("Difficulty Tuning (Baseline Offset)")]
    public float easyAdjustment = 0.30f;
    public float hardAdjustment = 0.05f;

    [Header("Intra-Wave Progression")]
    public float waveSpawnDecrease = 0.05f;
    public float hardWaveSpeedIncrease = 0.05f;

    [Header("Testing Fallbacks (If No Profile Found)")]
    public float fallbackSpeed = 25f;
    public float fallbackSpawnRate = 2f;

    private int currentWave = 0;
    private float currentWaveSpawnRate;
    private float currentWaveSpeed;
    private int rocksSpawnedThisWave = 0;
    private bool isSpawning = false;

    void Start()
    {
        if (DataLogger.Instance != null) DataLogger.Instance.InitializeLogger(currentDifficulty.ToString());

        CalculateInitialDifficulty();
        StartCoroutine(WaveCycleRoutine());
    }

    void CalculateInitialDifficulty()
    {
        float baseSpeed = fallbackSpeed;
        float baseSpawn = fallbackSpawnRate;

        if (ParticipantManager.Instance != null &&
            ParticipantManager.Instance.currentProfile != null &&
            ParticipantManager.Instance.currentProfile.hasCompletedAssessment)
        {
            baseSpeed = ParticipantManager.Instance.currentProfile.baselineSpeed;
            baseSpawn = ParticipantManager.Instance.currentProfile.baselineSpawnRate;
        }

        switch (currentDifficulty)
        {
            case DifficultyMode.Easy:
                currentWaveSpeed = baseSpeed * (1f - easyAdjustment);
                currentWaveSpawnRate = baseSpawn * (1f + easyAdjustment);
                break;
            case DifficultyMode.Medium:
                currentWaveSpeed = baseSpeed;
                currentWaveSpawnRate = baseSpawn;
                break;
            case DifficultyMode.Hard:
                currentWaveSpeed = baseSpeed * (1f + hardAdjustment);
                currentWaveSpawnRate = baseSpawn * (1f - hardAdjustment);
                break;
        }

        if (DataLogger.Instance != null)
        {
            DataLogger.Instance.LogDifficultyHeader(currentWaveSpawnRate, currentWaveSpeed, waveSpawnDecrease, hardWaveSpeedIncrease);
        }
    }

    IEnumerator WaveCycleRoutine()
    {
        while (GameManager.Instance == null || !GameManager.Instance.isLevelActive) yield return null;
        yield return new WaitForSeconds(2f);

        while (GameManager.Instance != null && GameManager.Instance.isLevelActive && currentWave < totalWaves)
        {
            currentWave++;
            GameManager.Instance.UpdateWaveUI(currentWave, totalWaves);

            if (currentWave > 1)
            {
                currentWaveSpawnRate *= (1f - waveSpawnDecrease);

                if (currentDifficulty == DifficultyMode.Hard)
                {
                    currentWaveSpeed *= (1f + hardWaveSpeedIncrease);
                }
            }

            rocksSpawnedThisWave = 0;
            GameManager.Instance.ResetWaveScore();
            GameManager.Instance.currentRockSpeed = currentWaveSpeed;

            string levelCodeName = "";
            if (currentDifficulty == DifficultyMode.Easy) levelCodeName = "ORBIT";
            else if (currentDifficulty == DifficultyMode.Medium) levelCodeName = "VELOCITY";
            else if (currentDifficulty == DifficultyMode.Hard) levelCodeName = "LUNAR";

            GameManager.Instance.ShowAnnouncer(levelCodeName + " PROTOCOL\nWAVE " + currentWave);
            yield return new WaitForSeconds(2f);
            GameManager.Instance.HideAnnouncer();

            string waveStartTimeStamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            float internalStartTime = Time.time;

            // SPAWNING
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

            // CLEANUP
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

            string waveEndTimeStamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            float trueDuration = Time.time - internalStartTime;

            // --- PERFORMANCE CALCULATION ---
            int rocksDestroyed = GameManager.Instance.rocksDestroyedThisWave;
            int actualScore = GameManager.Instance.currentWaveScore;
            float performancePercent = rocksSpawnedThisWave > 0 ? ((float)rocksDestroyed / rocksSpawnedThisWave) * 100f : 0f;

            if (DataLogger.Instance != null)
            {
                DataLogger.Instance.LogDifficultyWave(
                    currentWave, waveStartTimeStamp, waveEndTimeStamp, trueDuration,
                    currentWaveSpawnRate, currentWaveSpeed,
                    rocksSpawnedThisWave, rocksDestroyed, actualScore, performancePercent
                );
            }

            // BREAK
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
            if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
            {
                if (currentDifficulty == DifficultyMode.Easy) ParticipantManager.Instance.currentProfile.hasCompletedEasy = true;
                if (currentDifficulty == DifficultyMode.Medium) ParticipantManager.Instance.currentProfile.hasCompletedMedium = true;
                if (currentDifficulty == DifficultyMode.Hard) ParticipantManager.Instance.currentProfile.hasCompletedHard = true;
                ParticipantManager.Instance.SaveProfile();
            }

            GameManager.Instance.SetTimerSubText("");
            GameManager.Instance.ShowDashboardDashes();
            GameManager.Instance.ShowAnnouncer("PROTOCOL COMPLETE\n<size=50%>GREAT JOB</size>");
            yield return new WaitForSeconds(4f);
            GameManager.Instance.LevelComplete();
        }
    }

    IEnumerator SpawnAsteroidsRoutine()
    {
        while (isSpawning)
        {
            // --- CHANGED: Now adds the specific weight of the rock that was spawned ---
            int spawnedWeight = SpawnSingleAsteroid();
            rocksSpawnedThisWave += spawnedWeight;

            yield return new WaitForSeconds(currentWaveSpawnRate);
        }
    }

    // --- CHANGED: Returns an integer (the weight) instead of void ---
    int SpawnSingleAsteroid()
    {
        if (asteroidVariants == null || asteroidVariants.Length == 0) return 0;

        int randomIndex = Random.Range(0, asteroidVariants.Length);
        AsteroidVariant selectedVariant = asteroidVariants[randomIndex];

        Vector3 randomPos = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        Vector3 finalSpawnPos = transform.position + randomPos;
        Instantiate(selectedVariant.prefab, finalSpawnPos, Random.rotation);

        // Tell the routine how much this rock is worth
        return selectedVariant.spawnWeightValue;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}