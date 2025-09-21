// BossEnemyHealth.cs
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BossEnemyHealth : MonoBehaviour
{
    public int maxHP = 200;
    public int currentHP;
    public float deathDespawnDelay = 3f;

    public bool IsDead { get; private set; }

    public event Action<int, int> onDamaged; // (cur,max)
    public event Action onDied;

    void Awake() => currentHP = maxHP;

    public void TakeDamage(int dmg)
    {
        if (IsDead)
        {
            Debug.LogWarning("[BossHealth] TakeDamage called but boss is already DEAD.");
            return;
        }

        // Clamp damage to at least 1 so you never apply 0 or negative damage.
        int amount = Mathf.Max(1, dmg);

        // Show incoming damage before applying.
        Debug.Log($"[BossHealth] Incoming damage: {dmg}, clamped to {amount}");

        int oldHP = currentHP; // store previous HP for debugging

        currentHP = Mathf.Max(0, currentHP - amount);

        // Show the HP change
        Debug.Log($"[BossHealth] HP changed: {oldHP} → {currentHP}/{maxHP}");

        // Fire damaged event for UI bars etc.
        onDamaged?.Invoke(currentHP, maxHP);

        // Check for death
        if (currentHP <= 0)
        {
            Debug.Log($"[BossHealth] Boss has reached 0 HP. Triggering Die() at time {Time.time:F2}s.");
            Die();
        }
    }


    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        onDied?.Invoke(); // notify any UI / listeners

        // Tell the parent AI (even if Health is on a child)
        var bossAI = GetComponentInParent<BossEnemy>();
        if (bossAI != null)
        {
            bossAI.HandleExternalDeath();   // <- plays anim + destroys root
        }
        else
        {
            // Fallback: destroy the whole rig if no AI found
            Destroy(transform.root ? transform.root.gameObject : gameObject, deathDespawnDelay);
        }
    }
}
