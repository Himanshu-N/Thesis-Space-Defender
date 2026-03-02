using UnityEngine;

public class D_ShipBlaster : MonoBehaviour
{
    [Header("References")]
    public GameObject missilePrefab;
    public Transform firePoint1;      // Left Gun
    public Transform firePoint2;      // Right Gun
    public Transform crosshair;       // The moving UI Image

    [Header("Aiming Logic")]
    public Transform pilotEye;        // Drag your MAIN CAMERA here
    public float convergenceDistance = 100f; // How far away the beams meet (e.g., 100m)

    [Header("Settings")]
    public float missileSpeed = 50f;

    void Start()
    {
        // Auto-find camera if you forgot to assign it
        if (pilotEye == null)
            pilotEye = Camera.main.transform;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (missilePrefab == null || crosshair == null || pilotEye == null) return;

        // ---------------------------------------------------------
        // THE FIX: Calculate the "Real" Target in the distance
        // ---------------------------------------------------------

        // 1. Get the direction from your EYE through the CROSSHAIR
        Vector3 eyeToCrosshair = (crosshair.position - pilotEye.position).normalized;

        // 2. Extend that line out to the convergence distance
        // This automatically scales X and Y correctly!
        Vector3 targetPoint = pilotEye.position + (eyeToCrosshair * convergenceDistance);


        // --- MISSILE 1 (Left) ---
        GameObject missile1 = Instantiate(missilePrefab, firePoint1.position, Quaternion.identity);
        Vector3 aimDir1 = (targetPoint - firePoint1.position).normalized; // Aim at the calculated point

        missile1.transform.rotation = Quaternion.LookRotation(aimDir1);

        if (missile1.TryGetComponent<Rigidbody>(out Rigidbody rb1))
        {
            rb1.velocity = aimDir1 * missileSpeed;
        }
        Destroy(missile1, 5f);


        // --- MISSILE 2 (Right) ---
        GameObject missile2 = Instantiate(missilePrefab, firePoint2.position, Quaternion.identity);
        Vector3 aimDir2 = (targetPoint - firePoint2.position).normalized; // Aim at the calculated point

        missile2.transform.rotation = Quaternion.LookRotation(aimDir2);

        if (missile2.TryGetComponent<Rigidbody>(out Rigidbody rb2))
        {
            rb2.velocity = aimDir2 * missileSpeed;
        }
        Destroy(missile2, 5f);
    }
}