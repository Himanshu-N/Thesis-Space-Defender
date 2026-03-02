using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fracture : MonoBehaviour
{
    [Tooltip("\"Fractured\" is the object that this will break into")]
    public GameObject fractured;
    public float explosionForce = 500f; // Force of the burst
    public float explosionRadius = 5f;  // Size of the burst

    public void FractureObject()
    {
        // 1. Spawn the broken version (The cluster of rock chunks)
        GameObject brokenObj = Instantiate(fractured, transform.position, transform.rotation);

        // 2. Find all the rigidbodies (physics parts) inside the new broken object
        Rigidbody[] rbs = brokenObj.GetComponentsInChildren<Rigidbody>();

        // 3. Apply an explosion force to each chunk
        foreach (Rigidbody rb in rbs)
        {
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

        // 4. Destroy the original whole rock
        Destroy(gameObject);
    }
}