using UnityEngine;

public class AsteroidHealth : MonoBehaviour
{
    public int health = 1;

    [Header("Splitting Mechanic")]
    public GameObject[] splitPrefabs;

    [Header("Hit Feedback")]
    public MeshRenderer meshRenderer;
    public Color damageFlashColor = Color.white;
    private Color originalColor;
    private bool isDead = false;
    void Start()
    {
        if (meshRenderer != null) originalColor = meshRenderer.material.color;
    }

    public bool TakeDamage(int amount)
    {
        if (isDead) return true; // THE LOCK: If it's already dying, ignore any extra missile hits!

        health -= amount;

        if (health <= 0)
        {
            isDead = true; // Lock it immediately
            SplitIntoPieces();
            return true;
        }
        else
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = damageFlashColor;
                Invoke("ResetColor", 0.1f);
            }
            return false;
        }
    }

    void SplitIntoPieces()
    {
        // 1. Grab the current speed and direction of the big rock
        Rigidbody myRb = GetComponent<Rigidbody>();
        Vector3 currentVelocity = myRb != null ? myRb.velocity : Vector3.zero;

        if (splitPrefabs != null && splitPrefabs.Length > 0)
        {
            foreach (GameObject prefab in splitPrefabs)
            {
                // Give them a slight offset so they don't clip into each other
                Vector3 randomOffset = Random.insideUnitSphere * 1.5f;
                GameObject piece = Instantiate(prefab, transform.position + randomOffset, Random.rotation);

                // --- THE PHYSICS FIX ---
                Rigidbody pieceRb = piece.GetComponent<Rigidbody>();
                if (pieceRb != null)
                {
                    // Inherit the parent's speed, PLUS add an explosive outward burst!
                    Vector3 burst = randomOffset.normalized * Random.Range(3f, 8f);
                    pieceRb.velocity = currentVelocity + burst;

                    // Add some random tumbling rotation
                    pieceRb.angularVelocity = Random.insideUnitSphere * Random.Range(2f, 5f);
                }
            }
        }
    }

    void ResetColor()
    {
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }
}