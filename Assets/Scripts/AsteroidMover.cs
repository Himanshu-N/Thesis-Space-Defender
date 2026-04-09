using UnityEngine;

public class AsteroidMover : MonoBehaviour
{
    public float speed = 15f;
    public float maxScale = 2.0f;
    public int damage = 1; // How much it hurts
    public int scorePenalty = 20; // Editable in the inspector!

    private Vector3 targetPos;
    private Vector3 startPos;
    private float totalDistance;
    private bool hasHitPlayer = false; // Prevent double damage

    void Start()
    {
        // Reads the difficulty from the GameManager and multiplies its speed!
        if (GameManager.Instance != null)
        {
            speed = GameManager.Instance.currentRockSpeed;
        }
        if (Camera.main != null) targetPos = Camera.main.transform.position;
        startPos = transform.position;
        totalDistance = Vector3.Distance(startPos, targetPos);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.angularVelocity = Random.insideUnitSphere * 2f;
    }

    void Update()
    {
        if (hasHitPlayer) return;

        // Move
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Scale Logic... (Keep your existing scaling code here)

        // DAMAGE LOGIC
        float currentDist = Vector3.Distance(transform.position, targetPos);

        // If it gets within 2 meters (basically inside the cockpit)
        if (currentDist < 2.0f)
        {
            HitPlayer();
        }
    }

    void HitPlayer()
    {
        hasHitPlayer = true;

        // 1. Apply Damage
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterShipHit(scorePenalty);
        }

        // 2. Visual Feedback (Optional Shake?)
        // (We will add Glass Break effect here later!)

        // 3. Destroy Rock
        Destroy(gameObject);
    }
}