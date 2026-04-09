using System.IO;
using UnityEngine;

public class AssessmentLogger : MonoBehaviour
{
    public static AssessmentLogger Instance;
    private string filePath;
    private bool hasInitializedHeader = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        string fileName = "Assessment_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        Debug.Log("Assessment Logger Started. Saving to: " + filePath);
    }

    // NEW: Logs the initial parameters before the standard table headers
    public void LogInitialValues(float startSpawnRate, float startSpeed, float adaptPercent)
    {
        if (hasInitializedHeader) return;

        string initialRow = $"--- INITIAL VALUES ---,Start Spawn Rate:,{startSpawnRate:F2},Start Speed:,{startSpeed:F2},Adaptation:,{adaptPercent * 100}%\n\n";
        string header = "Wave,StartTime,EndTime,TrueDuration(s),SpawnRate,ActualSpeed,RocksSpawned,MaxPossibleScore,ActualWaveScore,Performance%,Decision\n";

        File.WriteAllText(filePath, initialRow + header);
        hasInitializedHeader = true;
    }

    // CHANGED: Now accepts startTime, endTime, and true duration
    public void LogWaveData(int wave, string startTime, string endTime, float duration, float spawnRate, float actualSpeed, int rocksSpawned, int maxScore, int actualScore, float performance, string decision)
    {
        string dataRow = $"{wave},{startTime},{endTime},{duration:F2},{spawnRate:F2},{actualSpeed:F2},{rocksSpawned},{maxScore},{actualScore},{performance:F2}%,{decision}\n";
        File.AppendAllText(filePath, dataRow);

        Debug.Log("<color=cyan>WAVE " + wave + " LOGGED:</color> " + dataRow);
    }
}