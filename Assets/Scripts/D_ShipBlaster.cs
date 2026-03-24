using UnityEngine;
using UnityEngine.XR;
using System.Collections;

public class D_ShipBlaster : MonoBehaviour
{
    [Header("References")]
    public GameObject missilePrefab;
    public Transform firePoint1;
    public Transform firePoint2;
    public Transform crosshair;
    public Transform pilotEye;

    [Header("Settings")]
    public float missileSpeed = 50f;
    public float convergenceDistance = 100f;
    private bool wasTriggerPressed = false;
    private bool wasReloadPressed = false; // Needed for manual reload

    [Header("Ammo & Reload")]
    public int maxAmmo = 20;
    private int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioSource blasterAudioSource;

    private float menuGracePeriod = 0.5f;
    void Start()
    {
        if (pilotEye == null) pilotEye = Camera.main.transform;
        currentAmmo = maxAmmo;

        if (GameManager.Instance != null) GameManager.Instance.UpdateAmmoUI(currentAmmo, maxAmmo);
    }

    // --- ADD THIS WITH YOUR OTHER VARIABLES ---

    // ... (Keep Start() the same) ...

    void Update()
    {
        // 1. ALWAYS read inputs continuously
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isTriggerPressed = false;
        bool isPrimaryPressed = false;

        if (rightHand.isValid)
        {
            rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerPressed);
            rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out isPrimaryPressed);
        }

        bool tryingToShoot = Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space) || (isTriggerPressed && !wasTriggerPressed);
        bool tryingToReload = Input.GetKeyDown(KeyCode.R) || (isPrimaryPressed && !wasReloadPressed);

        wasTriggerPressed = isTriggerPressed;
        wasReloadPressed = isPrimaryPressed;

        // --- THE FIXES ---
        // 2. Do absolutely nothing if we are on the instruction or end screen
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Playing)
        {
            return;
        }

        // 3. Warm-up delay to swallow the "Menu Click"
        if (menuGracePeriod > 0)
        {
            menuGracePeriod -= Time.deltaTime;
            return;
        }

        // 4. Reload & Shoot Logic
        if (isReloading) return;

        if (tryingToReload && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        if (tryingToShoot)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (missilePrefab == null || crosshair == null || pilotEye == null) return;
        if (currentAmmo <= 0) return;

        Vector3 eyeToCrosshair = (crosshair.position - pilotEye.position).normalized;
        Vector3 targetPoint = pilotEye.position + (eyeToCrosshair * convergenceDistance);

        GameObject m1 = Instantiate(missilePrefab, firePoint1.position, Quaternion.identity);
        Vector3 aim1 = (targetPoint - firePoint1.position).normalized;
        m1.transform.rotation = Quaternion.LookRotation(aim1);
        if (m1.TryGetComponent<Rigidbody>(out Rigidbody rb1)) rb1.velocity = aim1 * missileSpeed;
        Destroy(m1, 5f);

        GameObject m2 = Instantiate(missilePrefab, firePoint2.position, Quaternion.identity);
        Vector3 aim2 = (targetPoint - firePoint2.position).normalized;
        m2.transform.rotation = Quaternion.LookRotation(aim2);
        if (m2.TryGetComponent<Rigidbody>(out Rigidbody rb2)) rb2.velocity = aim2 * missileSpeed;
        Destroy(m2, 5f);

        if (shootSound != null && blasterAudioSource != null) blasterAudioSource.PlayOneShot(shootSound);

        currentAmmo--;
        if (GameManager.Instance != null) GameManager.Instance.RegisterShot(); // <-- ADD THIS LINE

        if (currentAmmo <= 0) StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true; // Locks the gun immediately

        if (reloadSound != null && blasterAudioSource != null)
            blasterAudioSource.PlayOneShot(reloadSound);

        if (GameManager.Instance != null)
            GameManager.Instance.StartReloadBlink(reloadTime);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false; // Unlocks the gun

        if (GameManager.Instance != null) GameManager.Instance.UpdateAmmoUI(currentAmmo, maxAmmo);
    }
}