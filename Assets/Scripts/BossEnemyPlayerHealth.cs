// BossEnemyPlayerHealth.cs
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BossEnemyPlayerHealth : MonoBehaviour, BossEnemyIDamageable
{
    public int maxHP = 100;
    public int currentHP;

    public bool IsDead { get; private set; }

    // Events the UI expects
    public event Action<int,int> onChanged; // (current, max)
    public event Action onDied;

    void Awake()
    {
        currentHP = maxHP;
        onChanged?.Invoke(currentHP, maxHP); // initialize UI
    }

    float _invulnUntil;
    public float invulnTime = 0.25f;

    public void TakeDamage(int dmg)
    {
        if (IsDead || Time.time < _invulnUntil) return;
        _invulnUntil = Time.time + invulnTime;

        int amount = Mathf.Max(1, dmg);
        currentHP = Mathf.Max(0, currentHP - amount);
        Debug.Log($"[PlayerHealth] took {amount}, now {currentHP}/{maxHP}");
        onChanged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0) Die();
    }


    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        onDied?.Invoke();
        // TODO: disable player controller / play death anim / show game over, etc.
    }
}
