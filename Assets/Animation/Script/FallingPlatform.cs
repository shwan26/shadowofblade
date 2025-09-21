using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FallingPlatformWithGroundCheck : MonoBehaviour
{
    [Header("Platform Settings")]
    public float fallDelay = 2f;       // Time before the platform falls
    public float destroyDelay = 5f;    // Time to destroy after falling
    public float checkHeight = 0.1f;   // Distance above platform to check for player

    [Header("Sound Settings")]
    public AudioClip warningSound;     // Drag your warning sound here
    public float soundVolume = 1f;     // Volume of the sound

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool triggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // keep platform in place initially

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // don't play automatically
    }

    void Update()
    {
        if (!triggered)
        {
            // Check if player is standing on top
            Collider[] hits = Physics.OverlapBox(
                transform.position + Vector3.up * (0.5f + checkHeight / 2f),
                new Vector3(0.5f, checkHeight / 2f, 0.5f),
                Quaternion.identity
            );

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    triggered = true;

                    // Play warning sound before falling
                    if (warningSound != null)
                        audioSource.PlayOneShot(warningSound, soundVolume);

                    Invoke(nameof(Fall), fallDelay);
                    break;
                }
            }
        }
    }

    void Fall()
    {
        rb.isKinematic = false; // enable physics so the platform falls
        Destroy(gameObject, destroyDelay);
    }

    // Optional: Draw the overlap box in Scene view for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * (0.5f + checkHeight / 2f),
            new Vector3(1f, checkHeight, 1f)
        );
    }
}
