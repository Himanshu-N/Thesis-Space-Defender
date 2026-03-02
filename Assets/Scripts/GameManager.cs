using UnityEngine;
using TMPro; // We will use TextMeshPro for sharp VR text
using UnityEngine.SceneManagement; // For restarting

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // The "Singleton" reference

    [Header("Game Stats")]
    public int score = 0;
    public int currentHealth = 100;
    public int maxHealth = 100;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text healthText;

    void Awake()
    {
        // Simple Singleton Setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + score;
        if (healthText != null) healthText.text = "HULL: " + currentHealth + "%";
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        // Reloads the scene to restart instantly
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}