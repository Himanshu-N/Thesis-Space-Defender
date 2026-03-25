using UnityEngine;

public class D_Missile : MonoBehaviour
{
    public int scoreValue = 100;

    [Header("Audio Settings")]
    public AudioClip[] explosionSounds; // This creates a list in the Inspector!
    [Range(0f, 2f)] public float explosionVolume = 1.0f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 1. Check if the rock has health
            AsteroidHealth healthScript = collision.gameObject.GetComponent<AsteroidHealth>();
            bool isDead = true; // Default to true for old rocks that don't have the script yet

            if (healthScript != null)
            {
                isDead = healthScript.TakeDamage(1); // Subtract 1 health
            }

            // 2. Only explode and give points IF it actually died
            if (isDead)
            {
                if (explosionSounds.Length > 0) PlayProper3DSound(transform.position);
                if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreValue);

                Fracture frac = collision.gameObject.GetComponent<Fracture>();
                if (frac) frac.FractureObject();
                else Destroy(collision.gameObject);
            }

            // 3. The missile always destroys itself on impact
            Destroy(gameObject);
        }
    }

    void PlayProper3DSound(Vector3 pos)
    {
        // 1. Pick a random number between 0 and the total number of sounds
        int randomIndex = Random.Range(0, explosionSounds.Length);
        AudioClip clipToPlay = explosionSounds[randomIndex];

        if (clipToPlay == null) return; // Safety check

        GameObject tempAudio = new GameObject("TempExplosionAudio");
        tempAudio.transform.position = pos;

        AudioSource src = tempAudio.AddComponent<AudioSource>();
        src.clip = clipToPlay;
        src.volume = explosionVolume/2;

        src.spatialBlend = 1.0f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 20f;
        src.maxDistance = 300f;

        src.Play();
        Destroy(tempAudio, clipToPlay.length + 0.1f);
    }
}