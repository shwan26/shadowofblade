using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Health : MonoBehaviour
{
    public enum CharacterType { Player, Enemy }
    [Header("Character Settings")]
    public CharacterType characterType = CharacterType.Enemy;

    [Header("General")]
    public float maxHealth = 100f;
    public float destroyDelay = 2f;
    private float currentHealth;
    public bool isDead { get; private set; }

    [Header("Player UI (Only for Player)")]
    public Slider healthBar;
    public Gradient gradient;
    public Image fillImage;
    public GameObject gameOverPanel;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip dieSound;
    public AudioClip gameOverSound;
    private AudioSource audioSource;

    [Header("VFX")]
    public GameObject hitEffectPrefab;

    [Header("Damage Mappings")]
    public float trapWallDamage = 100f;
    public float sawDamage = 100f;
    public float axeDamage = 50f;
    public float lavaDamage = 100f;

    private Dictionary<string, float> damageValues;
    private Animator anim;

    // 👉 Event to notify listeners when this character dies
    public System.Action OnDeath;

    void Awake()
    {
        damageValues = new Dictionary<string, float>()
        {
            {"TrapWall", trapWallDamage},
            {"Sawblade", sawDamage},
            {"Axe", axeDamage},
            {"Lava", lavaDamage}
        };

        if (characterType == CharacterType.Player && gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (characterType == CharacterType.Player && healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            UpdateHealthBarColor();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (characterType == CharacterType.Player && healthBar != null)
        {
            healthBar.value = currentHealth;
            UpdateHealthBarColor();
        }

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        if (anim != null)
            anim.SetTrigger("Hit");

        if (currentHealth <= 0) Die();
    }

    private void UpdateHealthBarColor()
    {
        if (fillImage != null && gradient != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            fillImage.color = gradient.Evaluate(healthPercentage);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        if (damageValues.ContainsKey(hit.gameObject.tag))
            TakeDamage(damageValues[hit.gameObject.tag]);
    }

    private void Die()
    {
        isDead = true;

        if (anim != null) anim.SetTrigger("Die");

        if (audioSource != null && dieSound != null)
            audioSource.PlayOneShot(dieSound);

        // 🔔 Notify listeners (like MovingPlatform)
        OnDeath?.Invoke();

        if (characterType == CharacterType.Player)
        {
            if (audioSource != null && gameOverSound != null)
                audioSource.PlayOneShot(gameOverSound);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }

        Destroy(gameObject, destroyDelay);
    }
}
