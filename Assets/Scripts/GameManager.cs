using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR;

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
    public TMP_Text timerSubText;
    public TMP_Text waveText;
    public GameObject centerAnnouncerObject;

    [Header("Ammo UI")]
    public Image[] ammoBlocks;
    public Color ammoColor = Color.cyan;
    public Color reloadBlinkColor = Color.red;
    private Coroutine reloadCoroutine;

    [Header("End Screen References")]
    public TMP_Text endTitleText;
    public TMP_Text finalScoreText;
    public TMP_Text finalRoundsText;
    public TMP_Text finalTimeText;

    [Header("Game Stats & Difficulty")]
    public int totalScore = 0;
    public int currentWaveScore = 0;
    public int scoreRewardPerRock = 100; // NEW: Editable in the Inspector!
    public int totalRoundsFired = 0;
    public float totalTimePlaying = 0f;
    public float currentRockSpeed = 50f; // CHANGED: Now tracks Actual Speed, not a multiplier

    [Header("Visual FX & Audio")]
    public Light alarmLight;
    public float alarmIntensity = 2.0f;
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
        if (centerAnnouncerObject != null) centerAnnouncerObject.SetActive(false);

        UpdateScoreUI();
        if (alarmLight != null) alarmLight.intensity = 0f;
    }

    void Update()
    {
        if (currentState == GameState.Playing) totalTimePlaying += Time.deltaTime;
    }



    public void StartGame()
    {
        currentState = GameState.Playing;
        isLevelActive = true;
        if (instructionCanvas != null) instructionCanvas.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(true);
    }

    // --- UPDATED SCORE LOGIC ---

    public void ResetWaveScore()
    {
        currentWaveScore = 0;
    }

    public void AddScore(int amount)
    {
        if (currentState != GameState.Playing) return;
        totalScore += amount;
        currentWaveScore += amount;
        UpdateScoreUI();
    }

    public void RegisterShipHit(int scorePenalty)
    {
        if (currentState != GameState.Playing) return;

        totalScore -= scorePenalty;
        currentWaveScore -= scorePenalty;

        // CLAMPING RULES (If less than zero, equals zero)
        if (totalScore < 0) totalScore = 0;
        if (currentWaveScore < 0) currentWaveScore = 0;

        UpdateScoreUI();

        if (damageSounds.Length > 0 && shipEffectsAudio != null)
        {
            int randomIndex = Random.Range(0, damageSounds.Length);
            shipEffectsAudio.PlayOneShot(damageSounds[randomIndex], damageVolume);
        }

        if (alarmLight != null) { StopCoroutine("FlashAlarm"); StartCoroutine("FlashAlarm"); }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = totalScore.ToString();
    }

    // --- UTILS & UI ---

    public void RegisterShot() { totalRoundsFired += 1; }

    public void ShowAnnouncer(string message)
    {
        if (centerAnnouncerObject != null)
        {
            TMP_Text textComponent = centerAnnouncerObject.GetComponentInChildren<TMP_Text>();
            if (textComponent != null) textComponent.text = message;
            centerAnnouncerObject.SetActive(true);
        }
    }

    public void HideAnnouncer() { if (centerAnnouncerObject != null) centerAnnouncerObject.SetActive(false); }
    public void SetTimerSubText(string message) { if (timerSubText != null) timerSubText.text = message; }
    public void ShowDashboardDashes() { if (dashboardTimerText != null) dashboardTimerText.text = "--:--"; }

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
            isVisible = !isVisible; yield return new WaitForSeconds(0.2f); timer += 0.2f;
        }
    }

    IEnumerator FlashAlarm()
    {
        for (int i = 0; i < 3; i++)
        {
            alarmLight.intensity = alarmIntensity; yield return new WaitForSeconds(0.1f);
            alarmLight.intensity = 0; yield return new WaitForSeconds(0.1f);
        }
    }

    public void LevelComplete()
    {
        currentState = GameState.GameOver;
        isLevelActive = false;

        if (alarmLight != null) alarmLight.intensity = 0f;
        if (hudCanvas != null) hudCanvas.SetActive(false);
        if (centerAnnouncerObject != null) centerAnnouncerObject.SetActive(false);
        if (endScreenCanvas != null) endScreenCanvas.SetActive(true);

        if (endTitleText != null) endTitleText.text = "ASSESSMENT COMPLETE";
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + totalScore;
        if (finalRoundsText != null) finalRoundsText.text = "Missiles Fired: " + totalRoundsFired;

        if (finalTimeText != null)
        {
            int mins = Mathf.FloorToInt(totalTimePlaying / 60F);
            int secs = Mathf.FloorToInt(totalTimePlaying - mins * 60);
            finalTimeText.text = string.Format("Total Time: {0:00}:{1:00}", mins, secs);
        }
    }
}