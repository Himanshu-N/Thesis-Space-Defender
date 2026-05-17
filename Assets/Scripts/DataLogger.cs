using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    public static DataLogger Instance;
    private string filePath;
    private bool hasInitializedHeader = false;

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
            ParticipantManager.Instance.lastPlayedLevel = levelName;
        }

        currentLevelName = levelName;

        string baseDir = Path.Combine(Application.persistentDataPath, "data");
        string levelFolder = Path.Combine(baseDir, currentParticipantID, currentLevelName);

        if (!Directory.Exists(levelFolder))
        {
            Directory.CreateDirectory(levelFolder);
        }

        filePath = Path.Combine(levelFolder, "gameplay.csv");

        if (File.Exists(filePath)) File.Delete(filePath);
    }

    // ==========================================
    // ASSESSMENT LEVEL LOGGING
    // ==========================================
    public void LogAssessmentHeader(float startSpawnRate, float startSpeed, float adaptPercent)
    {
        if (hasInitializedHeader) return;
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
    // BASELINE RESTING LEVEL LOGGING
    // ==========================================
    public void LogBaselineTimes(string instructionsStart, string crossStart, string crossEnd)
    {
        string topRow = $"Participant:,{currentParticipantID},Level:,{currentLevelName}\n";
        string header = "Event,Timestamp\n";
        string data = $"Instructions_Start,{instructionsStart}\nCross_Visible_Start,{crossStart}\nBaseline_End,{crossEnd}\n";

        File.WriteAllText(filePath, topRow + header + data);
        Debug.Log("<color=green>Baseline timestamps saved perfectly.</color>");
    }

    // ==========================================
    // EASY/MEDIUM/HARD LOGGING (CLEANED)
    // ==========================================
    public void LogDifficultyHeader(float startSpawnRate, float startSpeed)
    {
        if (hasInitializedHeader) return;
        string topRow = $"Participant:,{currentParticipantID},Level:,{currentLevelName}\n";
        string initialRow = $"--- LEVEL PARAMETERS ---,Start Spawn Rate:,{startSpawnRate:F2},Start Speed:,{startSpeed:F2}\n\n";
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