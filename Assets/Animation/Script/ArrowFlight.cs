using UnityEngine;

public class ArrowFlight : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f;

    private Vector3 flightDirection;

    void Start()
    {
        flightDirection = transform.forward;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move forward
        transform.position += flightDirection * speed * Time.deltaTime;
    }
}
