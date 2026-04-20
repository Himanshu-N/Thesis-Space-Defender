using UnityEngine;

public class AsteroidMover : MonoBehaviour
{
    [Header("Movement & Targeting")]
    public float speed = 15f;
    public float maxScale = 2.0f;
    public Vector2 targetOffset = new Vector2(3f, 3f);

    [Header("Damage Settings")]
    public int damage = 1;
    public int scorePenalty = 50;

    [Header("VFX")]
    [Tooltip("Drag your 100-piece fracture prefab here")]
    public GameObject fracturePrefab;

    private Vector3 targetPos;
    private Vector3 startPos;
    private bool hasHitPlayer = false;

    void Start()
    {
        if (GameManager.Instance != null) speed = GameManager.Instance.currentRockSpeed;

        if (Camera.main != null)
        {
            Vector3 randomDrift = new Vector3(
                Random.Range(-targetOffset.x, targetOffset.x),
                Random.Range(-targetOffset.y, targetOffset.y),
                0f
            );
            targetPos = Camera.main.transform.position + randomDrift;
        }

        startPos = transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.angularVelocity = Random.insideUnitSphere * 2f;
    }

    void Update()
    {
        if (hasHitPlayer) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Camera.main != null && transform.position.z < Camera.main.transform.position.z - 2f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHitPlayer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            HitPlayer();
        }
    }

    void HitPlayer()
    {
        hasHitPlayer = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterShipHit(scorePenalty);
        }

        // --- NEW: FRACTURE ON IMPACT ---
        if (fracturePrefab != null)
        {
            GameObject debris = Instantiate(fracturePrefab, transform.position, transform.rotation);

            // Calculate the exact speed and direction it was traveling
            Vector3 travelDirection = (targetPos - startPos).normalized;
            Vector3 impactVelocity = travelDirection * speed;

            // Push every piece of debris past the player
            Rigidbody[] pieces = debris.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody pieceRb in pieces)
            {
                // Give it the forward momentum PLUS a little outward burst so it scatters across the windshield
                Vector3 randomScatter = Random.insideUnitSphere * 5f;
                pieceRb.velocity = impactVelocity + randomScatter;
            }
        }

        Destroy(gameObject);
    }
}