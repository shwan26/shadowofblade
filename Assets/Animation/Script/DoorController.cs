// DoorController.cs
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private bool shouldMove = false;
    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position + Vector3.left * moveDistance;
    }

    void Update()
    {
        if (shouldMove)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                shouldMove = false;
            }
        }
    }

    public void OpenDoor()
    {
        shouldMove = true;
    }
}