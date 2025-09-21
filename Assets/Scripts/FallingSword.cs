using UnityEngine;
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class FallingSword : MonoBehaviour
{
    public int damage = 20;
    public LayerMask hitLayers;   // set to Player in Inspector or via spawner
    public bool destroyOnHit = true;

    bool used;

    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Expect a primitive collider already set as trigger in the prefab.
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning("[Sword] Expecting a primitive trigger collider on the prefab.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (used) return;

        Debug.Log($"[Sword] trigger with {other.name} (layer {LayerMask.LayerToName(other.gameObject.layer)})");

        // Layer filter (comment out to test)
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            Debug.Log("[Sword] ignored by layer mask");
            return;
        }

        var hp = other.GetComponentInParent<BossEnemyPlayerHealth>();
        if (hp != null)
        {
            Debug.Log("[Sword] damaging player");
            hp.TakeDamage(damage);
            used = true;
            if (destroyOnHit) Destroy(gameObject);
            return;
        }

        var dmg = other.GetComponentInParent<BossEnemyIDamageable>();
        if (dmg != null)
        {
            Debug.Log("[Sword] damaging IDamageable");
            dmg.TakeDamage(damage);
            used = true;
            if (destroyOnHit) Destroy(gameObject);
        }
        else
        {
            Debug.Log("[Sword] no health component on hit target");
        }
    }

    void OnCollisionEnter(Collision c) => OnTriggerEnter(c.collider);
}
