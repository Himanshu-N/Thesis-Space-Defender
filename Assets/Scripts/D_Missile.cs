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
            // Only play if the list has at least one sound in it
            if (explosionSounds.Length > 0)
            {
                PlayProper3DSound(transform.position);
            }

            if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreValue);

            Fracture frac = collision.gameObject.GetComponent<Fracture>();
            if (frac) frac.FractureObject();
            else Destroy(collision.gameObject);

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