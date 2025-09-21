using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BossEnemyHealthUI : MonoBehaviour
{
    public BossEnemyHealth bossHealth;   // leave empty to auto-find
    public Text hpText;                  // optional legacy Text

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (!slider) { Debug.LogError("[BossEnemyHealthUI] No Slider found."); enabled = false; return; }
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;
        gameObject.SetActive(false); // hidden until bound
    }

    void Start()
    {
        if (!bossHealth) bossHealth = FindFirstObjectByType<BossEnemyHealth>();
        if (bossHealth) Bind(bossHealth);
    }

    public void Bind(BossEnemyHealth target)
    {
        if (!target) return;

        // Unhook previous
        if (bossHealth != null)
        {
            bossHealth.onDamaged -= OnDamaged;
            bossHealth.onDied    -= OnDied;
        }

        bossHealth = target;
        bossHealth.onDamaged += OnDamaged;
        bossHealth.onDied    += OnDied;

        UpdateBar(bossHealth.currentHP, bossHealth.maxHP);
        gameObject.SetActive(true);
    }

    void OnDamaged(int cur, int max) => UpdateBar(cur, max);

    void UpdateBar(int cur, int max)
    {
        float v = max > 0 ? (float)cur / max : 0f;
        slider.value = v;
        if (hpText) hpText.text = $"{cur}/{max}";
    }

    void OnDied()
    {
        gameObject.SetActive(false);

        // Optional: re-bind if a new boss spawns later
        var next = FindFirstObjectByType<BossEnemyHealth>();
        if (next && next != bossHealth) Bind(next);
    }

    void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.onDamaged -= OnDamaged;
            bossHealth.onDied    -= OnDied;
        }
    }
}
