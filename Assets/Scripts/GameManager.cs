using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Instructions, Playing, GameOver }
    public GameState currentState = GameState.Instructions;
    public bool isLevelActive = false;

    [Header("UI Panels")]
    public GameObject hudCanvas;
    public GameObject instructionCanvas;
    public GameObject endScreenCanvas;

    [Header("HUD References")]
    public TMP_Text scoreText;
    public TMP_Text dashboardTimerText; // Repurposed!
    public TMP_Text waveText;
    public TMP_Text centerAnnouncerText; // NEW: The giant Arcade text
    public Image[] healthBlocks;

    [Header("Ammo UI")]
    public Image[] ammoBlocks;
    public Color ammoColor = Color.cyan;
    public Color reloadBlinkColor = Color.red;
    private Coroutine reloadCoroutine;

    [Header("End Screen References")]
    public TMP_Text endTitleText;
    public TMP_Text finalScoreText;
    public TMP_Text finalHealthText;
    public TMP_Text finalRoundsText; // NEW: Rounds Fired Stat

    [Header("Game Stats")]
    public int score = 0;
    public float currentHealth = 100;
    public float maxHealth = 100;
    public int totalRoundsFired = 0; // NEW

    [Header("Health Colors")]
    public Color highHealthColor = Color.green;
    public Color medHealthColor = new Color(1f, 0.5f, 0f);
    public Color lowHealthColor = Color.red;

    [Header("Visual FX & Audio")]
    public Light alarmLight;
    public float alarmIntensity = 2.0f;
    public AudioSource sirenAudio;
    public AudioClip countdownTickSound; // <-- NEW: Drag your beep/tick sound here
    public AudioSource shipEffectsAudio;
    public AudioClip[] damageSounds;
    [Range(0f, 2f)] public float damageVolume = 1.0f;

    private float menuGracePeriod = 0.5f;
    private bool wasTriggerPressed = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        isLevelActive = false;
        currentState = GameState.Instructions;

        if (instructionCanvas != null) instructionCanvas.SetActive(true);
        if (hudCanvas != null) hudCanvas.SetActive(false);
        if (endScreenCanvas != null) endScreenCanvas.SetActive(false);
        if (centerAnnouncerText != null) centerAnnouncerText.gameObject.SetActive(false);

        UpdateUI();
        if (alarmLight != null) alarmLight.intensity = 0f;
    }

    void Update()
    {
        if (currentState == GameState.Instructions) CheckForStartTrigger();
    }

    void CheckForStartTrigger()
    {
        if (Input.GetKeyDown(KeyCode.Space)) StartGame();

        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHand.isValid)
        {
            rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressed);
            if (isTriggerPressed && !wasTriggerPressed) StartGame();
            wasTriggerPressed = isTriggerPressed;
        }
    }

    void StartGame()
    {
        currentState = GameState.Playing;
        isLevelActive = true;

        if (instructionCanvas != null) instructionCanvas.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(true);
    }

    // --- NEW: Public methods for the Spawner and Blaster to use ---

    public void RegisterShot() { totalRoundsFired += 2; } // +2 because you fire double missiles!

    public void ShowAnnouncer(string message)
    {
        if (centerAnnouncerText != null)
        {
            centerAnnouncerText.text = message;
            centerAnnouncerText.gameObject.SetActive(true);
        }
    }

    public void HideAnnouncer()
    {
        if (centerAnnouncerText != null) centerAnnouncerText.gameObject.SetActive(false);
    }

    public void UpdateDashboardTimer(float timeRemaining)
    {
        if (dashboardTimerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60F);
            int seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);
            dashboardTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // ----------------------------------------------------------------

    public void AddScore(int amount)
    {
        if (currentState != GameState.Playing) return;
        score += amount;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (currentState != GameState.Playing) return;

        currentHealth -= amount;
        UpdateUI();

        if (damageSounds.Length > 0 && shipEffectsAudio != null)
        {
            int randomIndex = Random.Range(0, damageSounds.Length);
            shipEffectsAudio.PlayOneShot(damageSounds[randomIndex], damageVolume);
        }

        if (alarmLight != null) { StopCoroutine("FlashAlarm"); StartCoroutine("FlashAlarm"); }
        if (sirenAudio != null && currentHealth <= 20) { if (!sirenAudio.isPlaying) sirenAudio.Play(); }

        if (currentHealth <= 0) LevelComplete(false);
    }

    public void UpdateWaveUI(int currentWave, int totalWaves)
    {
        if (waveText != null) waveText.text = "Wave\n" + currentWave.ToString("D2") + "/" + totalWaves.ToString("D2");
    }

    public void UpdateAmmoUI(int current, int max)
    {
        if (ammoBlocks.Length == 0 || currentState != GameState.Playing) return;
        if (reloadCoroutine != null) { StopCoroutine(reloadCoroutine); reloadCoroutine = null; }

        int blocksToActive = (current * ammoBlocks.Length) / max;
        for (int i = 0; i < ammoBlocks.Length; i++)
        {
            ammoBlocks[i].enabled = (i < blocksToActive);
            ammoBlocks[i].color = ammoColor;
        }
    }

    public void StartReloadBlink(float duration)
    {
        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(ReloadBlinkRoutine(duration));
    }

    IEnumerator ReloadBlinkRoutine(float duration)
    {
        float timer = 0; bool isVisible = true;
        while (timer < duration && currentState == GameState.Playing)
        {
            foreach (var block in ammoBlocks) { block.enabled = isVisible; block.color = reloadBlinkColor; }
            isVisible = !isVisible;
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }
    }

    IEnumerator FlashAlarm()
    {
        for (int i = 0; i < 3; i++)
        {
            alarmLight.intensity = alarmIntensity; yield return new WaitForSeconds(0.1f);
            alarmLight.intensity = 0; yield return new WaitForSeconds(0.1f);
        }
        while (currentHealth <= 20 && alarmLight != null && currentState == GameState.Playing)
        {
            alarmLight.intensity = alarmIntensity * 0.8f; yield return new WaitForSeconds(0.3f);
            alarmLight.intensity = 0; yield return new WaitForSeconds(0.3f);
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = score.ToString("D5");

        if (healthBlocks.Length > 0)
        {
            float healthPercent = currentHealth / maxHealth;
            int blocksToActive = Mathf.CeilToInt(healthPercent * healthBlocks.Length);
            Color currentBlockColor = highHealthColor;
            if (healthPercent <= 0.3f) currentBlockColor = lowHealthColor;
            else if (healthPercent <= 0.6f) currentBlockColor = medHealthColor;

            for (int i = 0; i < healthBlocks.Length; i++)
            {
                healthBlocks[i].enabled = (i < blocksToActive);
                healthBlocks[i].color = currentBlockColor;
            }
        }
    }

    public void LevelComplete(bool survived)
    {
        currentState = GameState.GameOver;
        isLevelActive = false;

        if (sirenAudio != null) sirenAudio.Stop();
        if (alarmLight != null) alarmLight.intensity = 0f;

        if (hudCanvas != null) hudCanvas.SetActive(false);
        if (centerAnnouncerText != null) centerAnnouncerText.gameObject.SetActive(false);
        if (endScreenCanvas != null) endScreenCanvas.SetActive(true);

        if (endTitleText != null) endTitleText.text = survived ? "SECTOR CLEARED" : "SHIP DESTROYED";
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + score;
        if (finalHealthText != null) finalHealthText.text = "Hull Integrity: " + currentHealth + "%";

        // --- NEW STAT ---
        if (finalRoundsText != null) finalRoundsText.text = "Missiles Fired: " + totalRoundsFired;
    }
    public void ShowDashboardDashes()
    {
        if (dashboardTimerText != null) dashboardTimerText.text = "--:--";
    }

    public void PlayCountdownTick()
    {
        if (countdownTickSound != null && shipEffectsAudio != null)
        {
            shipEffectsAudio.PlayOneShot(countdownTickSound, 1.0f);
        }
    }
}