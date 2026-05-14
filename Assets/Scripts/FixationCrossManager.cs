using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class FixationCrossManager : MonoBehaviour
{
    [Header("VR UI (World Space)")]
    public GameObject instructionTextObj;
    public GameObject fixationCrossObj;

    [Header("Desktop UI (Screen Overlay)")]
    public TMP_Text desktopTimerText;

    [Header("Timings")]
    public float instructionDuration = 10f;
    public float baselineDuration = 180f; // 3 minutes = 180 seconds

    private string timeInstrStart;
    private string timeCrossStart;
    private string timeCrossEnd;

    void Start()
    {
        // Tell the DataLogger to create the Baseline folder
        if (DataLogger.Instance != null) DataLogger.Instance.InitializeLogger("Baseline");

        StartCoroutine(BaselineRoutine());
    }

    void Update()
    {
        // Researcher Emergency Override
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("<color=yellow>RESEARCHER OVERRIDE: Returning to Menu</color>");
            SceneManager.LoadScene("MainMenu");
        }
    }

    IEnumerator BaselineRoutine()
    {
        // --- PHASE 1: INSTRUCTIONS ---
        instructionTextObj.SetActive(true);
        fixationCrossObj.SetActive(false);
        timeInstrStart = System.DateTime.Now.ToString("HH:mm:ss.fff");

        float timer = instructionDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (desktopTimerText != null) desktopTimerText.text = $"Instructions Displaying: {Mathf.CeilToInt(timer)}s";
            yield return null;
        }

        // --- PHASE 2: FIXATION CROSS (3 MIN) ---
        instructionTextObj.SetActive(false);
        fixationCrossObj.SetActive(true);
        timeCrossStart = System.DateTime.Now.ToString("HH:mm:ss.fff");

        timer = baselineDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (desktopTimerText != null)
            {
                int mins = Mathf.FloorToInt(timer / 60F);
                int secs = Mathf.FloorToInt(timer - mins * 60);
                desktopTimerText.text = $"Baseline Recording: {mins:00}:{secs:00}";
            }
            yield return null;
        }

        // --- PHASE 3: COMPLETION & LOGGING ---
        timeCrossEnd = System.DateTime.Now.ToString("HH:mm:ss.fff");

        if (DataLogger.Instance != null)
        {
            DataLogger.Instance.LogBaselineTimes(timeInstrStart, timeCrossStart, timeCrossEnd);
        }

        if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
        {
            ParticipantManager.Instance.currentProfile.hasCompletedBaseline = true;
            ParticipantManager.Instance.SaveProfile();
        }

        // Automatically return to menu so researcher can stop OpenSignals
        SceneManager.LoadScene("MainMenu");
    }
}