using UnityEngine;
using UnityEngine.XR;
using System.Collections;

public class D_ShipBlaster : MonoBehaviour
{
    [Header("References")]
    public GameObject missilePrefab;
    public Transform firePoint; // CHANGED: Only one fire point now!
    public Transform crosshair;
    public Transform pilotEye;

    [Header("Settings")]
    public float missileSpeed = 50f;
    public float convergenceDistance = 100f;
    private bool wasTriggerPressed = false;
    private bool wasReloadPressed = false;
    private float menuGracePeriod = 0.5f;

    [Header("Ammo & Reload")]
    public int maxAmmo = 20;
    private int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioSource blasterAudioSource;

    void Start()
    {
        if (pilotEye == null) pilotEye = Camera.main.transform;
        currentAmmo = maxAmmo;

        if (GameManager.Instance != null) GameManager.Instance.UpdateAmmoUI(currentAmmo, maxAmmo);
    }

    void Update()
    {
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

        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        if (menuGracePeriod > 0)
        {
            menuGracePeriod -= Time.deltaTime;
            return;
        }

        if (isReloading) return;

        if (tryingToReload && currentAmmo < maxAmmo)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        if (tryingToShoot) Shoot();
    }

    void Shoot()
    {
        if (missilePrefab == null || crosshair == null || pilotEye == null || firePoint == null) return;
        if (currentAmmo <= 0) return;

        Vector3 eyeToCrosshair = (crosshair.position - pilotEye.position).normalized;
        Vector3 targetPoint = pilotEye.position + (eyeToCrosshair * convergenceDistance);

        // --- CHANGED: Only spawns one missile now ---
        GameObject m1 = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);
        Vector3 aim1 = (targetPoint - firePoint.position).normalized;
        m1.transform.rotation = Quaternion.LookRotation(aim1);

        if (m1.TryGetComponent<Rigidbody>(out Rigidbody rb1)) rb1.velocity = aim1 * missileSpeed;
        Destroy(m1, 5f);

        if (shootSound != null && blasterAudioSource != null) blasterAudioSource.PlayOneShot(shootSound);

        currentAmmo--;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterShot();
            GameManager.Instance.UpdateAmmoUI(currentAmmo, maxAmmo);
        }

        if (currentAmmo <= 0) StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;

        if (reloadSound != null && blasterAudioSource != null) blasterAudioSource.PlayOneShot(reloadSound);
        if (GameManager.Instance != null) GameManager.Instance.StartReloadBlink(reloadTime);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;

        if (GameManager.Instance != null) GameManager.Instance.UpdateAmmoUI(currentAmmo, maxAmmo);
    }
}