using UnityEngine;

public class Missile : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit an Enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 1. Try to get the "Fracture" script from the object we hit
            Fracture fractureScript = collision.gameObject.GetComponent<Fracture>();

            if (fractureScript != null)
            {
                // 2. If it has the script, trigger the break!
                fractureScript.FractureObject();
            }
            else
            {
                // Fallback: If it has no fracture script, just destroy it
                Destroy(collision.gameObject);
            }

            // 3. Destroy the missile itself
            Destroy(gameObject);
        }
    }
}