using UnityEngine;

public class ArrowDamage : MonoBehaviour
{
    public float damageAmount = 20f;
    public float damageCooldown = 0.5f; // Prevent multiple hits in 0.5 seconds

    private bool hasDamaged = false;
    private Health playerHealth;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player") && !hasDamaged)
        {
            playerHealth = hit.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                hasDamaged = true;

                // Destroy arrow after hitting player
                Destroy(gameObject, 0.1f);
            }
        }

        // Stick to other surfaces (optional)
        if (!hit.gameObject.CompareTag("Player"))
        {
            GetComponent<Rigidbody>().isKinematic = true;
            Destroy(gameObject, 3f); // Destroy after 3 seconds
        }
    }
}