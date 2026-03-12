using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
    public Image[] healthBlocks;

    [Header("Health Colors")]
    public Color highHealthColor = Color.green;
    public Color medHealthColor = new Color(1f, 0.5f, 0f); // Default Orange
    public Color lowHealthColor = Color.red;

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

        if (damageSounds.Length > 0 && shipEffectsAudio != null)
        {
            int randomIndex = Random.Range(0, damageSounds.Length);
            AudioClip clipToPlay = damageSounds[randomIndex];
            if (clipToPlay != null) shipEffectsAudio.PlayOneShot(clipToPlay, damageVolume);
        }

        if (alarmLight != null)
        {
            StopCoroutine("FlashAlarm");
            StartCoroutine("FlashAlarm");
        }

        if (sirenAudio != null && currentHealth <= 20)
        {
            if (!sirenAudio.isPlaying) sirenAudio.Play();
        }

        if (currentHealth <= 0) GameOver();
    }

    IEnumerator FlashAlarm()
    {
        for (int i = 0; i < 3; i++)
        {
            alarmLight.intensity = alarmIntensity;
            yield return new WaitForSeconds(0.1f);
            alarmLight.intensity = 0;
            yield return new WaitForSeconds(0.1f);
        }

        while (currentHealth <= 20 && alarmLight != null)
        {
            alarmLight.intensity = alarmIntensity * 0.8f;
            yield return new WaitForSeconds(0.3f);
            alarmLight.intensity = 0;
            yield return new WaitForSeconds(0.3f);
        }
    }

    void UpdateUI()
    {
        // 1. Update Score
        if (scoreText != null)
        {
            scoreText.text = score.ToString("D5");
        }

        // 2. Update Health Blocks
        if (healthBlocks.Length > 0)
        {
            float healthPercent = currentHealth / maxHealth;
            int blocksToActive = Mathf.CeilToInt(healthPercent * healthBlocks.Length);

            // Determine the color based on the current percentage
            Color currentBlockColor = highHealthColor; // Default to Green

            if (healthPercent <= 0.3f)
                currentBlockColor = lowHealthColor; // 30% or less = Red
            else if (healthPercent <= 0.6f)
                currentBlockColor = medHealthColor; // 60% to 30% = Orange

            // Apply visibility and color
            for (int i = 0; i < healthBlocks.Length; i++)
            {
                if (i < blocksToActive)
                {
                    healthBlocks[i].enabled = true;
                    healthBlocks[i].color = currentBlockColor; // Apply the active color
                }
                else
                {
                    healthBlocks[i].enabled = false;
                }
            }
        }
    }

    void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}