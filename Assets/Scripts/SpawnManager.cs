using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("Prefabs")]
    public GameObject swordPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnSword(Vector3 position)
    {
        Instantiate(swordPrefab, position, Quaternion.LookRotation(Vector3.down));
    }

    public void SpawnSwordRain(Vector3 center, int count, float radius, float height)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 ring = Random.insideUnitCircle * radius;
            Vector3 ground = new(center.x + ring.x, center.y, center.z + ring.y);
            float y = ground.y;

            if (Physics.Raycast(ground + Vector3.up * 30f, Vector3.down, out var hit, 60f))
                y = hit.point.y;

            SpawnSword(new Vector3(ground.x, y + height, ground.z));
        }
    }
}
