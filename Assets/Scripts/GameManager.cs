using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Stats")]
    public int score = 0;
    public float currentHealth = 100;
    public float maxHealth = 100;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text healthText;

    [Header("Visual FX")]
    public Light alarmLight;
    public float alarmIntensity = 2.0f;

    [Header("Audio")]
    public AudioSource sirenAudio;
    public AudioSource shipEffectsAudio;
    public AudioClip[] damageSounds;
    [Range(0f, 2f)] public float damageVolume = 1.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
        if (alarmLight != null) alarmLight.intensity = 0f;
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

        // --- Play Random Damage Sound ---
        if (damageSounds.Length > 0 && shipEffectsAudio != null)
        {
            int randomIndex = Random.Range(0, damageSounds.Length);
            AudioClip clipToPlay = damageSounds[randomIndex];

            if (clipToPlay != null)
            {
                shipEffectsAudio.PlayOneShot(clipToPlay, damageVolume);
            }
        }

        // --- Flash Red Light ---
        if (alarmLight != null)
        {
            StopCoroutine("FlashAlarm");
            StartCoroutine("FlashAlarm");
        }

        // --- Siren Logic ---
        if (sirenAudio != null && currentHealth <= 40)
        {
            if (!sirenAudio.isPlaying) sirenAudio.Play();
        }

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    IEnumerator FlashAlarm()
    {
        // 1. Rapid flash (feedback for taking a hit)
        for (int i = 0; i < 3; i++)
        {
            alarmLight.intensity = alarmIntensity;
            yield return new WaitForSeconds(0.1f);
            alarmLight.intensity = 0;
            yield return new WaitForSeconds(0.1f);
        }

        // 2. Continuous strobe (if health is in the critical zone)
        while (currentHealth <= 20 && alarmLight != null)
        {
            alarmLight.intensity = alarmIntensity * 0.8f; // Slightly dimmer for the constant loop
            yield return new WaitForSeconds(0.3f);
            alarmLight.intensity = 0;
            yield return new WaitForSeconds(0.3f);
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + score;
        if (healthText != null) healthText.text = "HULL: " + currentHealth + "%";
    }

    void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}