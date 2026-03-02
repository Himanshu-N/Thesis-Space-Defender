using UnityEngine;

public class D_Missile : MonoBehaviour
{
    public int scoreValue = 100; // Points per hit

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 1. ADD SCORE
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            // 2. FRACTURE & DESTROY (Existing Logic)
            Fracture frac = collision.gameObject.GetComponent<Fracture>();
            if (frac) frac.FractureObject();
            else Destroy(collision.gameObject);

            Destroy(gameObject); // Destroy Missile
        }
    }
}