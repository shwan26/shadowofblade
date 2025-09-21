using UnityEngine;

public class DoubleWallTrap : MonoBehaviour
{
    public GameObject leftWall;
    public GameObject rightWall;
    public float moveSpeed = 5.0f;
    public float distanceToMove = 5.0f;
    private bool isActivated = false;
    private float distanceMoved = 0.0f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
        }
    }

    void FixedUpdate()
    {
        if (isActivated)
        {
            float distanceThisFrame = moveSpeed * Time.fixedDeltaTime;

            leftWall.transform.Translate(Vector3.right * distanceThisFrame);
            rightWall.transform.Translate(Vector3.left * distanceThisFrame);

            distanceMoved += distanceThisFrame;

            if (distanceMoved >= distanceToMove)
            {
                isActivated = false;
            }
        }
    }
}