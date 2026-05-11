using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    public static DataLogger Instance;
    private string filePath;
    private bool hasInitializedHeader = false;

    // Track these to inject them into the headers
    private string currentParticipantID;
    private string currentLevelName;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeLogger(string levelName)
    {
        currentParticipantID = "Guest";
        if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
        {
            currentParticipantID = ParticipantManager.Instance.currentProfile.participantID;
        }
        // --- NEW: Tell the Participant Manager what level we are playing! ---
        if (ParticipantManager.Instance != null) ParticipantManager.Instance.lastPlayedLevel = levelName;

        currentLevelName = levelName;

        string folderPath = Path.Combine(Application.persistentDataPath, currentParticipantID);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = currentParticipantID + "_" + currentLevelName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        filePath = Path.Combine(folderPath, fileName);
    }

    // ==========================================
    // ASSESSMENT LEVEL LOGGING
    // ==========================================
    public void LogAssessmentHeader(float startSpawnRate, float startSpeed, float adaptPercent)
    {
        if (hasInitializedHeader) return;

        // --- CHANGED: Added Participant and Level to the top row ---
        string topRow = $"Participant:,{currentParticipantID},Level:,{currentLevelName}\n";
        string initialRow = $"--- INITIAL VALUES ---,Start Spawn Rate:,{startSpawnRate:F2},Start Speed:,{startSpeed:F2},Adaptation:,{adaptPercent * 100}%\n\n";
        string header = "Wave,StartTime,EndTime,TrueDuration(s),SpawnRate,ActualSpeed,RocksSpawned,RocksDestroyed,ActualWaveScore,Performance%,Decision\n";

        File.WriteAllText(filePath, topRow + initialRow + header);
        hasInitializedHeader = true;
    }

    public void LogAssessmentWave(int wave, string startTime, string endTime, float duration, float spawnRate, float actualSpeed, int rocksSpawned, int rocksDestroyed, int actualScore, float performance, string decision)
    {
        string dataRow = $"{wave},{startTime},{endTime},{duration:F2},{spawnRate:F2},{actualSpeed:F2},{rocksSpawned},{rocksDestroyed},{actualScore},{performance:F2}%,{decision}\n";
        File.AppendAllText(filePath, dataRow);
    }

    // ==========================================
    // EASY/MEDIUM/HARD LOGGING
    // ==========================================
    public void LogDifficultyHeader(float startSpawnRate, float startSpeed, float spawnDecrease, float hardSpeedIncrease)
    {
        if (hasInitializedHeader) return;

        // --- CHANGED: Added Participant and Level to the top row ---
        string topRow = $"Participant:,{currentParticipantID},Level:,{currentLevelName}\n";
        string initialRow = $"--- LEVEL PARAMETERS ---,Start Spawn Rate:,{startSpawnRate:F2},Start Speed:,{startSpeed:F2},Wave Spawn Decrease:,{spawnDecrease * 100}%,Hard Wave Speed Increase:,{hardSpeedIncrease * 100}%\n\n";
        string header = "Wave,StartTime,EndTime,TrueDuration(s),SpawnRate,ActualSpeed,RocksSpawned,RocksDestroyed,ActualWaveScore,Performance%\n";

        File.WriteAllText(filePath, topRow + initialRow + header);
        hasInitializedHeader = true;
    }

    public void LogDifficultyWave(int wave, string startTime, string endTime, float duration, float spawnRate, float actualSpeed, int rocksSpawned, int rocksDestroyed, int actualScore, float performance)
    {
        string dataRow = $"{wave},{startTime},{endTime},{duration:F2},{spawnRate:F2},{actualSpeed:F2},{rocksSpawned},{rocksDestroyed},{actualScore},{performance:F2}%\n";
        File.AppendAllText(filePath, dataRow);
    }
}