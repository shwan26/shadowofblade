using UnityEngine;

public class SwingAxe : MonoBehaviour
{
    public float swingAngle = 45f;       // Max swing angle
    public float swingSpeed = 2f;        // Swing speed
    public bool startLeft = true;        // Start swinging left-to-right or right-to-left
    public AudioClip swingSound;         // Swing sound
    public float soundThreshold = 0.95f; // When to trigger sound (normalized)
    public float soundDistance = 5f;     // Max distance for sound to play

    private float angle = 0f;
    private float phaseOffset = 0f;
    private AudioSource audioSource;
    private bool soundPlayed = false;
    private Transform player;

    void Start()
    {
        phaseOffset = startLeft ? 0f : Mathf.PI;

        // Add or get AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Rotate axe
        angle = Mathf.Sin(Time.time * swingSpeed + phaseOffset) * swingAngle;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= soundDistance)
            {
                float normalized = Mathf.Abs(Mathf.Sin(Time.time * swingSpeed + phaseOffset));
                if (normalized >= soundThreshold && !soundPlayed)
                {
                    if (swingSound != null)
                    {
                        // Fade volume based on distance (closer = louder)
                        float volume = 1f - (distance / soundDistance);
                        audioSource.PlayOneShot(swingSound, volume);
                    }
                    soundPlayed = true;
                }
                else if (normalized < soundThreshold)
                {
                    soundPlayed = false; // reset for next swing
                }
            }
        }
    }
}
