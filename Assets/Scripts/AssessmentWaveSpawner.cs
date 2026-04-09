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

    void Start()
    {
        currentSpawnInterval = startingSpawnInterval;
        currentRockSpeed = startingRockSpeed;
        StartCoroutine(WaveCycleRoutine());
    }

    IEnumerator WaveCycleRoutine()
    {
        while (GameManager.Instance == null || !GameManager.Instance.isLevelActive) yield return null;

        // --- NEW: Print the Initial Values to Excel exactly once ---
        if (AssessmentLogger.Instance != null)
        {
            AssessmentLogger.Instance.LogInitialValues(startingSpawnInterval, startingRockSpeed, adaptationPercentage);
        }

        yield return new WaitForSeconds(2f);

        while (GameManager.Instance != null && GameManager.Instance.isLevelActive && currentWave < totalWaves)
        {
            currentWave++;
            GameManager.Instance.UpdateWaveUI(currentWave, totalWaves);

            rocksSpawnedThisWave = 0;
            GameManager.Instance.ResetWaveScore();
            GameManager.Instance.currentRockSpeed = currentRockSpeed;

            // ANNOUNCE WAVE
            GameManager.Instance.ShowAnnouncer("ASSESSMENT WAVE " + currentWave);
            yield return new WaitForSeconds(2f);
            GameManager.Instance.HideAnnouncer();

            // --- TIME TRACKING STARTS HERE ---
            string waveStartTimeStamp = System.DateTime.Now.ToString("HH:mm:ss");
            float internalStartTime = Time.time;

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

            // --- TIME TRACKING ENDS HERE ---
            string waveEndTimeStamp = System.DateTime.Now.ToString("HH:mm:ss");
            float trueDuration = Time.time - internalStartTime;

            // PERFORMANCE CALCULATION & ADAPTATION 
            int maxPossibleScore = rocksSpawnedThisWave * GameManager.Instance.scoreRewardPerRock;
            int actualScore = GameManager.Instance.currentWaveScore;

            float performancePercent = maxPossibleScore > 0 ? ((float)actualScore / maxPossibleScore) * 100f : 0f;
            string decision = "Maintained";

            if (performancePercent < 60f)
            {
                decision = "Eased (-5%)";
                currentRockSpeed *= (1f - adaptationPercentage);
                currentSpawnInterval *= (1f + adaptationPercentage);
            }
            else if (performancePercent > 80f)
            {
                decision = "Cranked (+5%)";
                currentRockSpeed *= (1f + adaptationPercentage);
                currentSpawnInterval *= (1f - adaptationPercentage);
            }

            // --- LOG TO EXCEL ---
            if (AssessmentLogger.Instance != null)
            {
                AssessmentLogger.Instance.LogWaveData(
                    currentWave, waveStartTimeStamp, waveEndTimeStamp, trueDuration,
                    currentSpawnInterval, currentRockSpeed,
                    rocksSpawnedThisWave, maxPossibleScore, actualScore, performancePercent, decision
                );
            }

            // BREAK PHASE (Not counted in the duration)
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

        // ASSESSMENT COMPLETE
        if (GameManager.Instance.isLevelActive)
        {
            GameManager.Instance.SetTimerSubText("");
            GameManager.Instance.ShowDashboardDashes();

            GameManager.Instance.ShowAnnouncer("ASSESSMENT COMPLETE\n<size=50%>DATA LOGGED SUCESSFULLY</size>");
            yield return new WaitForSeconds(4f);
            GameManager.Instance.LevelComplete();
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
}