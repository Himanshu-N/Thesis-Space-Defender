using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class OpenSignalsImporter : MonoBehaviour
{
    [Header("Folder Paths")]
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
            UpdateStatus("<color=red>ERROR</color>", "Error: OpenSignals folder not found.");
            return;
        }

        string participantID = ParticipantManager.Instance.currentProfile.participantID;
        string levelName = ParticipantManager.Instance.lastPlayedLevel;

        // Map path to: ProjectSpaceDefender/data/Participant_{ID}/{LevelName}/
        string baseDir = Path.Combine(Application.persistentDataPath, "data");
        string destFolder = Path.Combine(baseDir, participantID, levelName);

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

            if (newestConverted != null)
            {
                string destPath = Path.Combine(destFolder, "bvp_converted.txt");
                // C# File.Move throws an error if the destination exists. We must delete the old one first.
                if (File.Exists(destPath)) File.Delete(destPath);

                File.Move(newestConverted.FullName, destPath);
                filesMoved = true;
            }

            if (newestRaw != null)
            {
                string destPath = Path.Combine(destFolder, "bvp_raw.txt");
                if (File.Exists(destPath)) File.Delete(destPath);

                File.Move(newestRaw.FullName, destPath);
                filesMoved = true;
            }

            if (filesMoved)
            {
                UpdateStatus("<color=green>SYNC SUCCESS</color>", $"SUCCESS: Synced {levelName} files to Participant_{participantID} folder.");
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