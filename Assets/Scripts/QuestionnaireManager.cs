using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuestionnaireManager : MonoBehaviour
{
    [Header("Question Banks")]
    [TextArea(2, 3)] public string[] assessmentQuestions; // 5 calibration questions
    [TextArea(2, 3)] public string[] experimentalQuestions; // 12 standard questions

    [Header("Testing Mode")]
    [Tooltip("Check this to force the 5-question Assessment layout for testing")]
    public bool testAssessmentMode = false;

    [Header("UI References")]
    public TMP_Text questionCounterText;
    public TMP_Text questionBodyText;
    public Button[] ratingButtons;
    public Button nextButton;
    public TMP_Text nextButtonText;

    [Header("Visual Feedback")]
    public Color defaultColor = Color.white;
    public Color selectedColor = new Color(0.2f, 0.8f, 0.2f);

    private string[] activeQuestions;
    private int currentQuestionIndex = 0;
    private int currentSelectedValue = -1;
    private int[] recordedAnswers;

    void Start()
    {
        InitializeQuestionSet();
        recordedAnswers = new int[activeQuestions.Length];
        ShowQuestion(0);
    }

    void InitializeQuestionSet()
    {
        string level = "Unknown";
        if (ParticipantManager.Instance != null) level = ParticipantManager.Instance.lastPlayedLevel;

        // Determine which set to use
        if (level == "Assessment" || testAssessmentMode)
        {
            activeQuestions = assessmentQuestions;
        }
        else
        {
            activeQuestions = experimentalQuestions;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) SceneManager.LoadScene("MainMenu");
    }

    public void ShowQuestion(int index)
    {
        currentQuestionIndex = index;
        currentSelectedValue = -1;

        questionCounterText.text = $"Question {index + 1} of {activeQuestions.Length}";
        questionBodyText.text = activeQuestions[index];

        foreach (Button btn in ratingButtons) btn.GetComponent<Image>().color = defaultColor;
        nextButton.interactable = false;

        nextButtonText.text = (currentQuestionIndex == activeQuestions.Length - 1) ? "SUBMIT" : "NEXT";
    }

    public void SelectRating(int rating)
    {
        currentSelectedValue = rating;
        for (int i = 0; i < ratingButtons.Length; i++)
        {
            ratingButtons[i].GetComponent<Image>().color = (i == rating - 1) ? selectedColor : defaultColor;
        }
        nextButton.interactable = true;
    }

    public void OnNextButtonClicked()
    {
        recordedAnswers[currentQuestionIndex] = currentSelectedValue;
        if (currentQuestionIndex < activeQuestions.Length - 1) ShowQuestion(currentQuestionIndex + 1);
        else
        {
            SaveData();
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void SaveData()
    {
        string pID = "Guest";
        string level = "Unknown";

        if (ParticipantManager.Instance != null && ParticipantManager.Instance.currentProfile != null)
        {
            pID = ParticipantManager.Instance.currentProfile.participantID;
            level = ParticipantManager.Instance.lastPlayedLevel;
            if (testAssessmentMode) level = "Assessment_TEST";
        }

        string baseDir = Path.Combine(Application.persistentDataPath, "data");
        string destFolder = Path.Combine(baseDir, pID, level);
        if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

        // Naming logic: Static for known levels, Timestamped for Guests/Unknowns
        string fileName = "questionnaire.csv";
        if (pID == "Guest" || level == "Unknown")
        {
            fileName = $"questionnaire_{level}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        }

        string filePath = Path.Combine(destFolder, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"Participant:,{pID},Level Evaluated:,{level}");
            writer.WriteLine($"DateTime:,{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine("");
            writer.WriteLine("Q_Index,Score,Question_Text");

            for (int i = 0; i < activeQuestions.Length; i++)
            {
                writer.WriteLine($"Q{i + 1},{recordedAnswers[i]},\"{activeQuestions[i]}\"");
            }
        }
        Debug.Log("Questionnaire Saved to: " + filePath);
    }
}