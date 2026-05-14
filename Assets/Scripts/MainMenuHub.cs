using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuHub : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject loginPanel;
    public GameObject levelSelectPanel;

    [Header("Login UI")]
    public TMP_InputField idInputField;
    public GameObject logoutButton; // NEW: Strict control over the logout button

    [Header("Profile Display (Level Panel)")]
    public TMP_Text currentPlayerText; // CHANGED: Separated from the Welcome text
    public TMP_Text statusText;

    [Header("Level Buttons")]
    public Button baselineButton;
    public Button assessmentButton;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button questionnaireButton;

    void Start()
    {
        ShowMainPanel();
    }

    // --- NEW: Helper method to safely reset the UI ---
    void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        loginPanel.SetActive(false);
        levelSelectPanel.SetActive(false);

        // Strictly hide post-login elements
        if (logoutButton != null) logoutButton.SetActive(false);
        if (currentPlayerText != null) currentPlayerText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    public void OnInitialPlayClicked()
    {
        mainPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    public void OnLoginButtonClicked()
    {
        string enteredID = idInputField.text.Trim();
        if (string.IsNullOrEmpty(enteredID)) return;

        ParticipantManager.Instance.LoginParticipant(enteredID);
        UpdateLevelSelectUI();

        loginPanel.SetActive(false);
        levelSelectPanel.SetActive(true);

        // Strictly SHOW post-login elements
        if (logoutButton != null) logoutButton.SetActive(true);
        if (currentPlayerText != null) currentPlayerText.gameObject.SetActive(true);
        if (statusText != null) statusText.gameObject.SetActive(true);
    }

    void UpdateLevelSelectUI()
    {
        ParticipantProfile profile = ParticipantManager.Instance.currentProfile;

        currentPlayerText.text = "CURRENT PLAYER: " + profile.participantID;

        if (profile.hasCompletedAssessment)
        {
            statusText.text = $"Baseline Speed: {profile.baselineSpeed:F1} | Spawn: {profile.baselineSpawnRate:F2}";
        }
        else
        {
            statusText.text = "Status: Calibration Required";
        }

        // --- STRICT PROGRESSION LOGIC ---
        // 1. Baseline is always available to start
        baselineButton.interactable = true;

        // 2. Assessment unlocks ONLY after Baseline
        assessmentButton.interactable = profile.hasCompletedBaseline;

        // 3. Experimental levels unlock ONLY after Assessment
        easyButton.interactable = profile.hasCompletedAssessment;
        mediumButton.interactable = profile.hasCompletedAssessment;
        hardButton.interactable = profile.hasCompletedAssessment;

        questionnaireButton.interactable = (ParticipantManager.Instance.lastPlayedLevel != "Unknown");

        SetButtonColor(baselineButton, profile.hasCompletedBaseline);
        SetButtonColor(assessmentButton, profile.hasCompletedAssessment);
        SetButtonColor(easyButton, profile.hasCompletedEasy);
        SetButtonColor(mediumButton, profile.hasCompletedMedium);
        SetButtonColor(hardButton, profile.hasCompletedHard);
    }

    void SetButtonColor(Button btn, bool isComplete)
    {
        ColorBlock colors = btn.colors;
        colors.normalColor = isComplete ? new Color(0.2f, 0.8f, 0.2f) : Color.white;
        btn.colors = colors;
    }

    public void LoadAssessment() { SceneManager.LoadScene("Level_Assessment"); }
    public void LoadEasy() { SceneManager.LoadScene("Level_Easy"); }
    public void LoadMedium() { SceneManager.LoadScene("Level_Medium"); }
    public void LoadHard() { SceneManager.LoadScene("Level_Tough"); }
    public void LoadQuestionnaire() { SceneManager.LoadScene("Level_Questionnaire"); }
    public void LoadBaseline() { SceneManager.LoadScene("Level_Baseline"); }

    public void Logout()
    {
        ParticipantManager.Instance.currentProfile = null;
        idInputField.text = "";

        // This will instantly hide the level panel, logout button, and texts, and show the Main Panel!
        ShowMainPanel();
    }
}