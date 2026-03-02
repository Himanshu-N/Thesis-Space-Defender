using UnityEngine;

public class ShipBlaster : MonoBehaviour
{
    [Header("Settings")]
    public GameObject missilePrefab; // The bullet prefab
    public float missileSpeed = 50f;

    [Header("Setup")]
    public Transform firePoint; // Where the bullet spawns

    void Start()
    {
        // Auto-assign FirePoint to the Camera if you leave it empty
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        // Left Click (Fire1) or Spacebar to shoot
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (missilePrefab == null) return;

        // 1. Spawn the missile
        GameObject missile = Instantiate(missilePrefab, firePoint.position, firePoint.rotation);

        // 2. Launch it forward
        Rigidbody rb = missile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * missileSpeed;
        }

        // 3. Destroy after 5 seconds to keep game clean
        Destroy(missile, 5f);
    }
}