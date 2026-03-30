using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float lifetime = 5f; // Destroys the object after 5 seconds

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}