using System.Collections;
using UnityEngine;

public class AssessmentWaveSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject asteroidPrefab;

    [Header("Spawn Area Setup")]
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 0f);

    [Header("Assessment Settings")]
    public int totalWaves = 10;
    public float waveDuration = 45f;
    public float breakDuration = 10f;
    public float cleanupDuration = 15f;

    [Header("Initial Difficulty Parameters")]
    public float startingSpawnInterval = 1.0f;
    public float startingRockSpeed = 50f;
    public float adaptationPercentage = 0.05f;

    private int currentWave = 0;
    private float currentSpawnInterval;
    private float currentRockSpeed;
    private int rocksSpawnedThisWave = 0;
    private bool isSpawning = false;

    // Track final wave stats for baseline calculation
    private float finalWavePerformance = 0f;
    private float finalWaveSpeed = 0f;
    private float finalWaveSpawn = 0f;

    void Start()
    {
        // 1. Initialize the Logger and create the folder
        if (DataLogger.Instance != null) DataLogger.Instance.InitializeLogger("Assessment");

        currentSpawnInterval = startingSpawnInterval;
        currentRockSpeed = startingRockSpeed;
        StartCoroutine(WaveCycleRoutine());
    }

    IEnumerator WaveCycleRoutine()
    {
        while (GameManager.Instance == null || !GameManager.Instance.isLevelActive) yield return null;

        // 2. Log the Assessment Header
        if (DataLogger.Instance != null)
        {
            DataLogger.Instance.LogAssessmentHeader(startingSpawnInterval, startingRockSpeed, adaptationPercentage);
        }

        yield return new WaitForSeconds(2f);

        while (GameManager.Instance != null && GameManager.Instance.isLevelActive && currentWave < totalWaves)
        {
            currentWave++;
            GameManager.Instance.UpdateWaveUI(currentWave, totalWaves);

            rocksSpawnedThisWave = 0;
            GameManager.Instance.ResetWaveScore();
            GameManager.Instance.currentRockSpeed = currentRockSpeed;

            // Capture the exact stats the player is playing THIS wave at
            finalWaveSpeed = currentRockSpeed;
            finalWaveSpawn = currentSpawnInterval;

            GameManager.Instance.ShowAnnouncer("ASSESSMENT WAVE " + currentWave);
            yield return new WaitForSeconds(2f);
            GameManager.Instance.HideAnnouncer();

            string waveStartTimeStamp = System.DateTime.Now.ToString("HH:mm:ss");
            float internalStartTime = Time.time;

            // SPAWNING PHASE
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

            // CLEANUP PHASE
            GameManager.Instance.SetTimerSubText("Debris\nCleanup");
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

            string waveEndTimeStamp = System.DateTime.Now.ToString("HH:mm:ss");
            float trueDuration = Time.time - internalStartTime;

            // --- PERFORMANCE CALCULATION ---
            int rocksDestroyed = GameManager.Instance.rocksDestroyedThisWave;
            int actualScore = GameManager.Instance.currentWaveScore;

            float performancePercent = rocksSpawnedThisWave > 0 ? ((float)rocksDestroyed / rocksSpawnedThisWave) * 100f : 0f;

            // Save this to calculate the baseline at the end of the game
            finalWavePerformance = performancePercent;

            string decision = "Maintained";
            int displayPercent = Mathf.RoundToInt(adaptationPercentage * 100f);

            if (performancePercent < 60f)
            {
                decision = $"Decreased (-{displayPercent}%)";
                currentRockSpeed *= (1f - adaptationPercentage);
                currentSpawnInterval *= (1f + adaptationPercentage);
            }
            else if (performancePercent > 80f)
            {
                decision = $"Increased (+{displayPercent}%)";
                currentRockSpeed *= (1f + adaptationPercentage);
                currentSpawnInterval *= (1f - adaptationPercentage);
            }

            // --- LOG TO EXCEL ---
            if (DataLogger.Instance != null)
            {
                DataLogger.Instance.LogAssessmentWave(
                    currentWave, waveStartTimeStamp, waveEndTimeStamp, trueDuration,
                    finalWaveSpawn, finalWaveSpeed,
                    rocksSpawnedThisWave, rocksDestroyed, actualScore, performancePercent, decision
                );
            }

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

        // --- ASSESSMENT COMPLETE: CALCULATE AND SAVE BASELINE ---
        if (GameManager.Instance.isLevelActive)
        {
            CalculateAndSaveBaseline();

            GameManager.Instance.SetTimerSubText("");
            GameManager.Instance.ShowDashboardDashes();
            GameManager.Instance.ShowAnnouncer("ASSESSMENT COMPLETE\n<size=50%>BASELINE SAVED TO PROFILE</size>");
            yield return new WaitForSeconds(4f);
            GameManager.Instance.LevelComplete();
        }
    }

    void CalculateAndSaveBaseline()
    {
        float calculatedBaselineSpeed = finalWaveSpeed;
        float calculatedBaselineSpawn = finalWaveSpawn;

        // Apply the strict 10% rule based on Wave 10's performance
        if (finalWavePerformance < 60f)
        {
            calculatedBaselineSpeed *= 0.90f; // 10% slower
            calculatedBaselineSpawn *= 1.10f; // 10% more time between rocks
        }
        else if (finalWavePerformance > 80f)
        {
            calculatedBaselineSpeed *= 1.10f; // 10% faster
            calculatedBaselineSpawn *= 0.90f; // 10% less time between rocks
        }
        // If between 60% and 80%, it remains exactly what it was in Wave 10 (Maintained)

        // Save it permanently to the Participant Manager
        if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
        {
            ParticipantManager.Instance.currentProfile.baselineSpeed = calculatedBaselineSpeed;
            ParticipantManager.Instance.currentProfile.baselineSpawnRate = calculatedBaselineSpawn;
            ParticipantManager.Instance.currentProfile.hasCompletedAssessment = true;

            // Write it to the JSON file
            ParticipantManager.Instance.SaveProfile();

            Debug.Log($"<color=green>BASELINE SAVED:</color> Speed: {calculatedBaselineSpeed:F2}, Spawn: {calculatedBaselineSpawn:F2}");
        }
    }

    IEnumerator SpawnAsteroidsRoutine()
    {
        while (isSpawning)
        {
            SpawnSingleAsteroid();
            rocksSpawnedThisWave++;
            yield return new WaitForSeconds(currentSpawnInterval);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}