using System.IO;
using UnityEngine;

[System.Serializable]
public class ParticipantProfile
{
    public string participantID;

    // --- NEW: Baseline Tracker ---
    public bool hasCompletedBaseline = false;

    public bool hasCompletedAssessment = false;
    public bool hasCompletedEasy = false;
    public bool hasCompletedMedium = false;
    public bool hasCompletedHard = false;
    public float baselineSpeed = 15f;
    public float baselineSpawnRate = 3f;
}

public class ParticipantManager : MonoBehaviour
{
    public static ParticipantManager Instance;
    public ParticipantProfile currentProfile;
    public string lastPlayedLevel = "Unknown";

    private string baseDataDirectory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Root is now strictly data/ in the persistent data path
        baseDataDirectory = Path.Combine(Application.persistentDataPath, "data");
    }

    public void LoginParticipant(string id)
    {
        // Folder is now just the ID (e.g. data/p04)
        string participantFolder = Path.Combine(baseDataDirectory, id);
        if (!Directory.Exists(participantFolder))
        {
            Directory.CreateDirectory(participantFolder);
        }

        string filePath = Path.Combine(participantFolder, "Profile_" + id + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            currentProfile = JsonUtility.FromJson<ParticipantProfile>(json);
            Debug.Log("Loaded: " + id);
        }
        else
        {
            currentProfile = new ParticipantProfile();
            currentProfile.participantID = id;
            SaveProfile();
            Debug.Log("Created: " + id);
        }
    }

    public void SaveProfile()
    {
        if (currentProfile == null || string.IsNullOrEmpty(currentProfile.participantID)) return;

        string participantFolder = Path.Combine(baseDataDirectory, currentProfile.participantID);
        if (!Directory.Exists(participantFolder)) Directory.CreateDirectory(participantFolder);

        string filePath = Path.Combine(participantFolder, "Profile_" + currentProfile.participantID + ".json");
        string json = JsonUtility.ToJson(currentProfile, true);
        File.WriteAllText(filePath, json);
    }
}