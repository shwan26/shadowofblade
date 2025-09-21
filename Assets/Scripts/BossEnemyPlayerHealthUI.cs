// BossEnemyPlayerHealthUI.cs
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BossEnemyPlayerHealthUI : MonoBehaviour
{
    public BossEnemyPlayerHealth playerHealth;
    public Slider slider;

    void Start()
    {
        if (!playerHealth) playerHealth = FindFirstObjectByType<BossEnemyPlayerHealth>();
        if (!slider || !playerHealth) return;

        slider.minValue = 0f;
        slider.maxValue = playerHealth.maxHP;
        slider.value    = playerHealth.currentHP;

        playerHealth.onChanged += (cur, max) => { slider.maxValue = max; slider.value = cur; };
        playerHealth.onDied    += () => { /* TODO: Show game over */ gameObject.SetActive(false); };
    }
}
