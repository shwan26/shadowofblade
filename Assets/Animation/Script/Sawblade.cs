using UnityEngine;

public class Sawblade : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinSpeed = 360f; // Degrees per second

    [Header("Movement Settings")]
    public float moveDistance = 3f; // How far left/right it travels
    public float moveSpeed = 2f;    // How quickly it moves

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Spin the sawblade on its local Z-axis
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);

        // Calculate horizontal offset using a sine wave
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        // Apply the offset to its starting position
        transform.position = startPos + transform.right * offset;
    }
}
