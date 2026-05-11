using System.IO;
using UnityEngine;

[System.Serializable]
public class ParticipantProfile
{
    public string participantID;
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
    // --- NEW: Remembers the level we just returned from ---
    public string lastPlayedLevel = "Unknown";
    private string saveDirectory;

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
        saveDirectory = Application.persistentDataPath;
    }

    public void LoginParticipant(string id)
    {
        // --- CHANGED: Create the participant folder immediately upon login! ---
        string folderPath = Path.Combine(saveDirectory, id);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "Profile_" + id + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            currentProfile = JsonUtility.FromJson<ParticipantProfile>(json);
            Debug.Log("Loaded existing profile for: " + id);
        }
        else
        {
            currentProfile = new ParticipantProfile();
            currentProfile.participantID = id;
            SaveProfile();
            Debug.Log("Created new profile for: " + id);
        }
    }

    public void SaveProfile()
    {
        if (currentProfile == null || string.IsNullOrEmpty(currentProfile.participantID)) return;

        // --- CHANGED: Save inside the specific folder ---
        string folderPath = Path.Combine(saveDirectory, currentProfile.participantID);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "Profile_" + currentProfile.participantID + ".json");
        string json = JsonUtility.ToJson(currentProfile, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Profile Saved: " + currentProfile.participantID);
    }
}