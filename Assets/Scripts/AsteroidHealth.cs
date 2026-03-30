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
        Rigidbody myRb = GetComponent<Rigidbody>();

        if (splitPrefabs != null && splitPrefabs.Length > 0)
        {
            foreach (GameObject prefab in splitPrefabs)
            {
                // Instantiate with EXACT position and rotation of the parent
                GameObject piece = Instantiate(prefab, transform.position, transform.rotation);

                // Copy the exact velocity so it doesn't shoot off in a random direction
                Rigidbody pieceRb = piece.GetComponent<Rigidbody>();
                if (pieceRb != null && myRb != null)
                {
                    pieceRb.velocity = myRb.velocity;
                    pieceRb.angularVelocity = myRb.angularVelocity;
                }
            }
        }
    }

    void ResetColor()
    {
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }
}