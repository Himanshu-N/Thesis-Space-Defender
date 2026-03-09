using UnityEngine;
using UnityEngine.XR; // This is the VR library

public class D_ShipBlaster : MonoBehaviour
{
    [Header("References")]
    public GameObject missilePrefab;
    public Transform firePoint1;      // Left Gun
    public Transform firePoint2;      // Right Gun
    public Transform crosshair;       // The moving UI Image

    [Header("Aiming Logic")]
    public Transform pilotEye;
    public float convergenceDistance = 100f;

    [Header("Settings")]
    public float missileSpeed = 50f;

    // VR specific variable to stop rapid-fire
    private bool wasTriggerPressed = false;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioSource blasterAudioSource; // Add this line

    void Start()
    {
        if (pilotEye == null) pilotEye = Camera.main.transform;
    }

    void Update()
    {
        // --- 1. PC Keyboard/Mouse Fallback (For testing without headset) ---
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        // --- 2. VR Controller Logic (Right Hand Trigger) ---
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHand.isValid)
        {
            // Read the trigger value
            rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressed);

            // If it wasn't pressed last frame, but IS pressed this frame = "Button Down"
            if (isTriggerPressed && !wasTriggerPressed)
            {
                Shoot();
            }
            // Save state for next frame
            wasTriggerPressed = isTriggerPressed;
        }
    }

    void Shoot()
    {
        if (missilePrefab == null || crosshair == null || pilotEye == null) return;

        // Aim calculation
        Vector3 eyeToCrosshair = (crosshair.position - pilotEye.position).normalized;
        Vector3 targetPoint = pilotEye.position + (eyeToCrosshair * convergenceDistance);

        // --- MISSILE 1 (Left) ---
        GameObject missile1 = Instantiate(missilePrefab, firePoint1.position, Quaternion.identity);
        Vector3 aimDir1 = (targetPoint - firePoint1.position).normalized;
        missile1.transform.rotation = Quaternion.LookRotation(aimDir1);
        if (missile1.TryGetComponent<Rigidbody>(out Rigidbody rb1)) rb1.velocity = aimDir1 * missileSpeed;
        Destroy(missile1, 5f);

        // --- MISSILE 2 (Right) ---
        GameObject missile2 = Instantiate(missilePrefab, firePoint2.position, Quaternion.identity);
        Vector3 aimDir2 = (targetPoint - firePoint2.position).normalized;
        missile2.transform.rotation = Quaternion.LookRotation(aimDir2);
        if (missile2.TryGetComponent<Rigidbody>(out Rigidbody rb2)) rb2.velocity = aimDir2 * missileSpeed;
        Destroy(missile2, 5f);

        if (shootSound != null && blasterAudioSource != null)
        {
            // PlayOneShot is instantaneous and doesn't create lag
            blasterAudioSource.PlayOneShot(shootSound);
        }
    }
}