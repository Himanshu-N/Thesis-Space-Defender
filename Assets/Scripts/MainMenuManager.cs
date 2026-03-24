using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainPanel;       // Holds Play & Exit buttons
    public GameObject difficultyPanel; // Holds Easy, Medium, Hard buttons

    void Start()
    {
        // Show the main buttons, hide the difficulty buttons on start
        mainPanel.SetActive(true);
        difficultyPanel.SetActive(false);
    }

    // --- BUTTON FUNCTIONS ---

    public void OnPlayClicked()
    {
        mainPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // These need to match the exact names of your duplicated scenes!
    public void OnEasyClicked() { SceneManager.LoadScene("Level_Easy"); }
    public void OnMediumClicked() { SceneManager.LoadScene("Level_Medium"); }
    public void OnHardClicked() { SceneManager.LoadScene("Level_Hard"); }

    public void OnExitClicked()
    {
        Debug.Log("Game Quit!");
        Application.Quit(); // Note: This only works in the built .exe, not in the Unity Editor
    }
}