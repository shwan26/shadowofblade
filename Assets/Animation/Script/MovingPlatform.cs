using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("Platform Movement")]
    public Vector3 moveOffset = new Vector3(0, 5f, 0); // How far to move up
    public float moveSpeed = 2f;

    [Header("Enemies To Watch")]
    public List<Health> enemiesToWatch = new List<Health>(); // Drag enemies here

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool moving = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;

        // Subscribe to death events
        foreach (Health enemy in enemiesToWatch)
        {
            if (enemy != null)
                enemy.OnDeath += CheckEnemies;
        }
    }

    void Update()
    {
        if (moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    private void CheckEnemies()
    {
        // Start moving only when all watched enemies are dead
        foreach (Health enemy in enemiesToWatch)
        {
            if (enemy != null && !enemy.isDead)
                return;
        }
        moving = true;
    }
}
