using UnityEngine;
using System.Collections.Generic;

public class ArrowTrap : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;       // The prefab of the arrow to be instantiated
    public float fireRate = 2f;          // Time in seconds between each arrow fired
    public int arrowsPerShot = 2;        // Number of arrows to fire at the same time

    [Header("Spawn Points")]
    public List<Transform> arrowSpawnPoints = new List<Transform>();
    // Add multiple empty GameObjects in Inspector

    private float nextFireTime = 0f;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Time.time >= nextFireTime)
        {
            FireRandomArrows();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireRandomArrows()
    {
        if (arrowSpawnPoints.Count == 0) return;

        // Create a temporary list to avoid repeating spawn points
        List<Transform> availableSpawns = new List<Transform>(arrowSpawnPoints);

        for (int i = 0; i < arrowsPerShot; i++)
        {
            if (availableSpawns.Count == 0) break; // No more spawn points left

            // Pick a random spawn point from the available list
            int randomIndex = Random.Range(0, availableSpawns.Count);
            Transform randomSpawn = availableSpawns[randomIndex];

            // Spawn the arrow
            Instantiate(arrowPrefab, randomSpawn.position, randomSpawn.rotation);

            // Remove used spawn point so it won't be reused in this shot
            availableSpawns.RemoveAt(randomIndex);
        }
    }
}
