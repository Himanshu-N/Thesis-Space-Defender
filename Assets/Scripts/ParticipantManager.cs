using System.IO;
using UnityEngine;

// This defines the exact data we store for each player
[System.Serializable]
public class ParticipantProfile
{
    public string participantID;

    // Status Trackers
    public bool hasCompletedAssessment = false;
    public bool hasCompletedEasy = false;
    public bool hasCompletedMedium = false;
    public bool hasCompletedHard = false;

    // Baseline Parameters (Calculated after Assessment)
    public float baselineSpeed = 15f;
    public float baselineSpawnRate = 3f;
}

public class ParticipantManager : MonoBehaviour
{
    public static ParticipantManager Instance;

    public ParticipantProfile currentProfile;
    private string saveDirectory;

    void Awake()
    {
        // This makes sure this script never gets destroyed when changing scenes!
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

        // Saves to the same safe folder as your CSVs
        saveDirectory = Application.persistentDataPath;
    }

    // Call this from the Main Menu when entering an ID
    public void LoginParticipant(string id)
    {
        string filePath = Path.Combine(saveDirectory, "Profile_" + id + ".json");

        if (File.Exists(filePath))
        {
            // Player exists, load their data
            string json = File.ReadAllText(filePath);
            currentProfile = JsonUtility.FromJson<ParticipantProfile>(json);
            Debug.Log("Loaded existing profile for: " + id);
        }
        else
        {
            // New player, create a fresh profile
            currentProfile = new ParticipantProfile();
            currentProfile.participantID = id;
            SaveProfile();
            Debug.Log("Created new profile for: " + id);
        }
    }

    public void SaveProfile()
    {
        if (currentProfile == null || string.IsNullOrEmpty(currentProfile.participantID)) return;

        string filePath = Path.Combine(saveDirectory, "Profile_" + currentProfile.participantID + ".json");
        string json = JsonUtility.ToJson(currentProfile, true); // 'true' makes it formatted and readable
        File.WriteAllText(filePath, json);

        Debug.Log("Profile Saved: " + currentProfile.participantID);
    }
}