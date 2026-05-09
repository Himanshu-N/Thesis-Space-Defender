using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    public static DataLogger Instance;
    private string filePath;
    private bool hasInitializedHeader = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeLogger(string levelName)
    {
        string participantID = "Guest";
        if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
        {
            participantID = ParticipantManager.Instance.currentProfile.participantID;
        }

        string folderPath = Path.Combine(Application.persistentDataPath, participantID);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = participantID + "_" + levelName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        filePath = Path.Combine(folderPath, fileName);

        Debug.Log($"<color=cyan>DATA LOGGER:</color> Saving to folder {folderPath}");
    }

    // ==========================================
    // ASSESSMENT LEVEL LOGGING
    // ==========================================
    public void LogAssessmentHeader(float startSpawnRate, float startSpeed, float adaptPercent)
    {
        if (hasInitializedHeader) return;
        string initialRow = $"--- INITIAL VALUES ---,Start Spawn Rate:,{startSpawnRate:F2},Start Speed:,{startSpeed:F2},Adaptation:,{adaptPercent * 100}%\n\n";
        string header = "Wave,StartTime,EndTime,TrueDuration(s),SpawnRate,ActualSpeed,RocksSpawned,RocksDestroyed,ActualWaveScore,Performance%,Decision\n";

        File.WriteAllText(filePath, initialRow + header);
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
    // CHANGED: Added the two progressive variables to the header
    public void LogDifficultyHeader(float startSpawnRate, float startSpeed, float spawnDecrease, float hardSpeedIncrease)
    {
        if (hasInitializedHeader) return;

        string initialRow = $"--- LEVEL PARAMETERS ---,Start Spawn Rate:,{startSpawnRate:F2},Start Speed:,{startSpeed:F2},Wave Spawn Decrease:,{spawnDecrease * 100}%,Hard Wave Speed Increase:,{hardSpeedIncrease * 100}%\n\n";
        string header = "Wave,StartTime,EndTime,TrueDuration(s),SpawnRate,ActualSpeed,RocksSpawned,RocksDestroyed,ActualWaveScore,Performance%\n";

        File.WriteAllText(filePath, initialRow + header);
        hasInitializedHeader = true;
    }

    public void LogDifficultyWave(int wave, string startTime, string endTime, float duration, float spawnRate, float actualSpeed, int rocksSpawned, int rocksDestroyed, int actualScore, float performance)
    {
        string dataRow = $"{wave},{startTime},{endTime},{duration:F2},{spawnRate:F2},{actualSpeed:F2},{rocksSpawned},{rocksDestroyed},{actualScore},{performance:F2}%\n";
        File.AppendAllText(filePath, dataRow);
    }
}