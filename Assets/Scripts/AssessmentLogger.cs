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
    }

    void Start()
    {
        // NEW: Pull the ID from the Participant Manager!
        string participantID = "Guest";
        if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
        {
            participantID = ParticipantManager.Instance.currentProfile.participantID;
        }

        string fileName = participantID + "_Assessment_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        Debug.Log("Assessment Logger Started. Saving to: " + filePath);
    }

    public void LogInitialValues(float startSpawnRate, float startSpeed, float adaptPercent)
    {
        if (hasInitializedHeader) return;

        string initialRow = $"--- INITIAL VALUES ---,Start Spawn Rate:,{startSpawnRate:F2},Start Speed:,{startSpeed:F2},Adaptation:,{adaptPercent * 100}%\n\n";
        string header = "Wave,StartTime,EndTime,TrueDuration(s),SpawnRate,ActualSpeed,RocksSpawned,RocksDestroyed,ActualWaveScore,Performance%,Decision\n";

        File.WriteAllText(filePath, initialRow + header);
        hasInitializedHeader = true;
    }

    public void LogWaveData(int wave, string startTime, string endTime, float duration, float spawnRate, float actualSpeed, int rocksSpawned, int rocksDestroyed, int actualScore, float performance, string decision)
    {
        string dataRow = $"{wave},{startTime},{endTime},{duration:F2},{spawnRate:F2},{actualSpeed:F2},{rocksSpawned},{rocksDestroyed},{actualScore},{performance:F2}%,{decision}\n";
        File.AppendAllText(filePath, dataRow);

        Debug.Log("<color=cyan>WAVE " + wave + " LOGGED:</color> " + dataRow);
    }
}