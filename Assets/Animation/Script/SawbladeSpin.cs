using UnityEngine;

public class SawbladeSpin : MonoBehaviour
{
    public float spinSpeed = 360f;      // Spin speed in degrees/sec
    public float moveDistance = 5f;     // Total back-and-forth distance
    public float moveSpeed = 1f;        // Movement speed
    public bool startLeft = true;       // True = left-to-right first, false = right-to-left

    public AudioClip hitSound;          // Sound to play at movement extremes
    public float soundDistance = 5f;    // Only play sound if player is within this distance
    private AudioSource audioSource;

    private Vector3 startPosition;
    private bool soundPlayed = false;   // To play sound only once per extreme
    private Transform player;

    void Start()
    {
        startPosition = transform.position;

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
        // 1. Spin the sawblade
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);

        // 2. Back-and-forth movement
        float timeOffset = startLeft ? 0f : moveDistance;
        float pingPongValue = Mathf.PingPong(Time.time * moveSpeed + timeOffset, moveDistance);
        transform.position = startPosition + Vector3.right * pingPongValue;

        // 3. Play sound at extremes if player is near
        if (player != null && hitSound != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= soundDistance)
            {
                // Check if saw is near movement extremes
                if ((pingPongValue <= 0.01f || pingPongValue >= moveDistance - 0.01f) && !soundPlayed)
                {
                    audioSource.PlayOneShot(hitSound);
                    soundPlayed = true;
                }
                else if (pingPongValue > 0.01f && pingPongValue < moveDistance - 0.01f)
                {
                    soundPlayed = false; // reset for next extreme
                }
            }
        }
    }
}
