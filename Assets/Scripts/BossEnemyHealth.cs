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

    public event Action<int,int> onDamaged; // (cur,max)
    public event Action onDied;

    void Awake() => currentHP = maxHP;

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;
        int amount = Mathf.Max(1, dmg);
        currentHP = Mathf.Max(0, currentHP - amount);
        Debug.Log($"[BossHealth] took {amount}, now {currentHP}/{maxHP}");
        onDamaged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0) Die();
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
