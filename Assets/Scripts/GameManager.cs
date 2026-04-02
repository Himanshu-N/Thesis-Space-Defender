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
    public TMP_Text dashboardTimerText;
    public TMP_Text timerSubText; // <-- NEW!
    public TMP_Text waveText;
    // --- CHANGED: Now accepts the parent GameObject ---
    public GameObject centerAnnouncerObject;
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
    public TMP_Text finalRoundsText;
    public TMP_Text finalTimeText;

    [Header("Game Stats & Difficulty")]
    public int score = 0;
    public int currentHealth = 7; // CHANGED: Now an exact hit count
    public int maxHealth = 7;     // CHANGED: Max 7 hits
    public int totalRoundsFired = 0;
    public float totalTimePlaying = 0f;
    public float currentRockSpeedMultiplier = 1.0f;

    [Header("Health Colors")]
    public Color highHealthColor = Color.green;
    public Color medHealthColor = new Color(1f, 0.5f, 0f);
    public Color lowHealthColor = Color.red;

    [Header("Visual FX & Audio")]
    public Light alarmLight;
    public float alarmIntensity = 2.0f;
    public AudioSource sirenAudio;
    public AudioSource shipEffectsAudio;
    public AudioClip[] damageSounds;
    [Range(0f, 2f)] public float damageVolume = 1.0f;
    public AudioClip countdownTickSound;

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

        // --- CHANGED ---
        if (centerAnnouncerObject != null) centerAnnouncerObject.SetActive(false);

        UpdateUI();
        if (alarmLight != null) alarmLight.intensity = 0f;
    }

    void Update()
    {
        if (currentState == GameState.Instructions)
        {
            CheckForStartTrigger();
        }
        else if (currentState == GameState.Playing)
        {
            totalTimePlaying += Time.deltaTime;
        }
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

    public void RegisterShot() { totalRoundsFired += 1; }
    // --- CHANGED: Finds the text child, updates it, then turns on the whole parent ---
    public void ShowAnnouncer(string message)
    {
        if (centerAnnouncerObject != null)
        {
            TMP_Text textComponent = centerAnnouncerObject.GetComponentInChildren<TMP_Text>();
            if (textComponent != null) textComponent.text = message;

            centerAnnouncerObject.SetActive(true);
        }
    }

    public void HideAnnouncer()
    {
        if (centerAnnouncerObject != null) centerAnnouncerObject.SetActive(false);
    }
    // --------------------------------------------------------------------------------

    public void ShowDashboardDashes()
    {
        if (dashboardTimerText != null) dashboardTimerText.text = "--:--";
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

    public void PlayCountdownTick()
    {
        if (countdownTickSound != null && shipEffectsAudio != null) shipEffectsAudio.PlayOneShot(countdownTickSound, 1.0f);
    }

    public void AddScore(int amount)
    {
        if (currentState != GameState.Playing) return;
        score += amount;
        UpdateUI();
    }

    public void UpdateWaveUI(int currentWave, int totalWaves)
    {
        if (waveText != null) waveText.text = "Wave\n" + currentWave.ToString("D2") + "/" + totalWaves.ToString("D2");
    }

    public void UpdateAmmoUI(int current, int max)
    {
        if (ammoBlocks == null || ammoBlocks.Length == 0) return;
        if (reloadCoroutine != null) { StopCoroutine(reloadCoroutine); reloadCoroutine = null; }

        float percent = (float)current / max;
        int blocksToActive = Mathf.CeilToInt(percent * ammoBlocks.Length);

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

    public void TakeDamage(int amount)
    {
        if (currentState != GameState.Playing) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        if (damageSounds.Length > 0 && shipEffectsAudio != null)
        {
            int randomIndex = Random.Range(0, damageSounds.Length);
            shipEffectsAudio.PlayOneShot(damageSounds[randomIndex], damageVolume);
        }

        if (alarmLight != null) { StopCoroutine("FlashAlarm"); StartCoroutine("FlashAlarm"); }

        // --- CHANGED: Alarm triggers on the last 2 blocks ---
        if (sirenAudio != null && currentHealth <= 2) { if (!sirenAudio.isPlaying) sirenAudio.Play(); }

        if (currentHealth <= 0) LevelComplete(false);
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = score.ToString("D5");

        if (healthBlocks.Length > 0)
        {
            Color currentBlockColor = highHealthColor;

            // --- CHANGED: Hardcoded color states based on your exact rules ---
            if (currentHealth <= 2) currentBlockColor = lowHealthColor; // Red for 1 or 2 blocks
            else if (currentHealth <= 5) currentBlockColor = medHealthColor; // Orange for 3, 4, or 5 blocks

            for (int i = 0; i < healthBlocks.Length; i++)
            {
                // Directly maps hit points to blocks (no more decimals/percentages)
                healthBlocks[i].enabled = (i < currentHealth);
                healthBlocks[i].color = currentBlockColor;
            }
        }
    }

    IEnumerator FlashAlarm()
    {
        for (int i = 0; i < 3; i++)
        {
            alarmLight.intensity = alarmIntensity; yield return new WaitForSeconds(0.1f);
            alarmLight.intensity = 0; yield return new WaitForSeconds(0.1f);
        }

        // --- CHANGED: Keeps flashing if 2 blocks or lower ---
        while (currentHealth <= 2 && alarmLight != null && currentState == GameState.Playing)
        {
            alarmLight.intensity = alarmIntensity * 0.8f; yield return new WaitForSeconds(0.3f);
            alarmLight.intensity = 0; yield return new WaitForSeconds(0.3f);
        }
    }

    public void LevelComplete(bool survived)
    {
        currentState = GameState.GameOver;
        isLevelActive = false;

        if (sirenAudio != null) sirenAudio.Stop();
        if (alarmLight != null) alarmLight.intensity = 0f;

        if (hudCanvas != null) hudCanvas.SetActive(false);

        // --- CHANGED ---
        if (centerAnnouncerObject != null) centerAnnouncerObject.SetActive(false);

        if (endScreenCanvas != null) endScreenCanvas.SetActive(true);

        if (endTitleText != null) endTitleText.text = survived ? "SECTOR CLEARED" : "SHIP DESTROYED";
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + score;
        // Replace your old finalHealthText line with this one:
        if (finalHealthText != null) finalHealthText.text = "Hull Integrity: " + currentHealth + " / " + maxHealth;
        if (finalRoundsText != null) finalRoundsText.text = "Missiles Fired: " + totalRoundsFired;

        if (finalTimeText != null)
        {
            int mins = Mathf.FloorToInt(totalTimePlaying / 60F);
            int secs = Mathf.FloorToInt(totalTimePlaying - mins * 60);
            finalTimeText.text = string.Format("Time Alive: {0:00}:{1:00}", mins, secs);
        }
    }
    public void SetTimerSubText(string message)
    {
        if (timerSubText != null) timerSubText.text = message;
    }
}