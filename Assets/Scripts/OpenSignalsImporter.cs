using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class OpenSignalsImporter : MonoBehaviour
{
    [Header("Folder Paths")]
    [Tooltip("Paste the exact folder path where OpenSignals saves its files on your PC")]
    public string openSignalsDirectory = @"C:\Users\YOUR_USERNAME\Documents\OpenSignals (r)evolution\files";

    [Header("UI Feedback")]
    public TMP_Text syncStatusText;

    public void SyncLatestFiles()
    {
        if (ParticipantManager.Instance == null || ParticipantManager.Instance.currentProfile == null)
        {
            UpdateStatus("<color=red>ERROR</color>", "Error: No Participant Logged In!");
            return;
        }

        if (!Directory.Exists(openSignalsDirectory))
        {
            UpdateStatus("<color=red>ERROR</color>", "Error: OpenSignals folder not found. Check the path in Inspector.");
            return;
        }

        string participantID = ParticipantManager.Instance.currentProfile.participantID;

        // --- CHANGED: Grab the level name they just played ---
        string levelName = ParticipantManager.Instance.lastPlayedLevel;

        string destFolder = Path.Combine(Application.persistentDataPath, participantID);

        if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

        DirectoryInfo dirInfo = new DirectoryInfo(openSignalsDirectory);

        try
        {
            FileInfo newestConverted = dirInfo.GetFiles("*converted*.txt")
                                              .OrderByDescending(f => f.CreationTime)
                                              .FirstOrDefault();

            FileInfo newestRaw = dirInfo.GetFiles("opensignals_*.txt")
                                        .Where(f => !f.Name.Contains("converted"))
                                        .OrderByDescending(f => f.CreationTime)
                                        .FirstOrDefault();

            bool filesMoved = false;
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (newestConverted != null)
            {
                // --- CHANGED: Added Level Name to the file! ---
                string newConvertedName = $"{participantID}_{levelName}_EDA-BVP_Converted_{timestamp}.txt";
                string destPath = Path.Combine(destFolder, newConvertedName);

                File.Move(newestConverted.FullName, destPath);
                filesMoved = true;
                Debug.Log($"Moved Converted: {newConvertedName}");
            }

            if (newestRaw != null)
            {
                // --- CHANGED: Added Level Name to the file! ---
                string newRawName = $"{participantID}_{levelName}_EDA-BVP_Raw_{timestamp}.txt";
                string destPath = Path.Combine(destFolder, newRawName);

                File.Move(newestRaw.FullName, destPath);
                filesMoved = true;
                Debug.Log($"Moved Raw: {newRawName}");
            }

            // --- CHANGED: Split the UI and Console messages ---
            if (filesMoved)
            {
                UpdateStatus("<color=green>SYNC SUCCESS</color>", $"SUCCESS: Synced {levelName} EDA files to {participantID} folder.");
            }
            else
            {
                UpdateStatus("<color=yellow>NO FILES FOUND</color>", "Warning: No OpenSignals files found to sync.");
            }
        }
        catch (System.Exception e)
        {
            UpdateStatus("<color=red>SYNC ERROR</color>", $"Error moving files: {e.Message}");
        }
    }

    // --- CHANGED: Now takes two separate messages ---
    void UpdateStatus(string uiMessage, string consoleMessage)
    {
        if (syncStatusText != null)
        {
            syncStatusText.text = uiMessage;
            Invoke("ClearStatus", 4f);
        }
        Debug.Log(consoleMessage);
    }

    void ClearStatus()
    {
        if (syncStatusText != null) syncStatusText.text = "";
    }
}