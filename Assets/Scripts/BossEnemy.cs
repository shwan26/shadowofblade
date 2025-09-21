using UnityEngine;
using UnityEngine.AI;
using System.Collections;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BossEnemyHealth))]
public class BossEnemy : MonoBehaviour
{
    private bool _dead;
    [Header("Targeting")]
    public Transform player;
    public string playerTag = "Player";
    public float sightRange = 18f;
    public float attackRange = 2.3f;

    [Header("Movement")]
    public float repathInterval = 0.2f;

    [Header("Melee")]
    public int meleeDamage = 25;
    public float meleeCooldown = 1.6f;
    public float meleeRadius = 2.2f;
    public LayerMask playerLayers;

    [Header("Casting (Sword Rain)")]
    public GameObject skySwordPrefab;
    public int swordsPerCast = 8;
    public float castCooldown = 6f;
    public float castRadius = 4.5f;
    public float swordSpawnHeight = 10f;

    [Tooltip("Won't cast if too close; encourages closing distance.")]
    public float castMinRange = 4.0f;
    [Tooltip("Won't cast if too far; encourages walking until closer.")]
    public float castMaxRange = 12.0f;

    [Header("Animator (assign if Animator is not on the same GameObject)")]
    public Animator anim;

    private NavMeshAgent agent;
    private BossEnemyHealth health;

    private float lastMeleeTime = -999f;
    private float lastCastTime = -999f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int CastHash = Animator.StringToHash("Cast");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<BossEnemyHealth>();
        if (!anim) anim = GetComponent<Animator>();
        if (!anim) anim = GetComponentInChildren<Animator>();

        if (playerLayers.value == 0)    // default if not assigned
            playerLayers = LayerMask.GetMask("Player");
    }

    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }

        health.onDamaged += (_, __) => { if (!_dead) anim.SetTrigger(HitHash); };
        health.onDied += OnDied;
        StartCoroutine(RepathLoop());
        
    }


    IEnumerator RepathLoop()
    {
        var wait = new WaitForSeconds(repathInterval);

        while (true)
        {
            if (_dead) yield break; 
            if (!player)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go) player = go.transform;
                yield return wait;
                continue;
            }

            float dist = Vector3.Distance(transform.position, player.position);
            bool canSee = dist <= sightRange;

            // Small debug pulse to understand agent state
            if (!agent.hasPath && !agent.pathPending)
            {
                // Try to get a path if in sight range
                if (canSee) agent.SetDestination(player.position);
            }

            if (canSee)
            {
                bool inMelee = dist <= attackRange;
                bool canMelee = (Time.time - lastMeleeTime) >= meleeCooldown;
                bool canCast = (Time.time - lastCastTime) >= castCooldown
                                && dist >= castMinRange && dist <= castMaxRange;

                if (inMelee && canMelee)
                {
                    StopMoving();
                    anim.SetFloat(SpeedHash, 0f);
                    anim.SetTrigger(AttackHash);
                    lastMeleeTime = Time.time;
                    Invoke(nameof(ResumeChase), 0.6f);
                }

                else if (canCast)
                {
                    // Commit to cast at mid-range, not right at melee
                    StopMoving();
                    anim.SetFloat(SpeedHash, 0f);
                    anim.SetTrigger(CastHash);
                    lastCastTime = Time.time;
                    // AE_CastRelease() will spawn swords; then resume
                    Invoke(nameof(ResumeChase), 0.8f);
                }
                else
                {
                    // CHASE
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                    anim.SetFloat(SpeedHash, agent.velocity.magnitude);

                    // Helpful debug if it's not closing:
                    if (agent.pathStatus == NavMeshPathStatus.PathPartial)
                        Debug.LogWarning("[BossEnemy] Path is PARTIAL. Check gaps in NavMesh.");
                    if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                        Debug.LogWarning("[BossEnemy] Path INVALID. Boss or Player likely off NavMesh.");
                    if (agent.stoppingDistance >= attackRange)
                        Debug.LogWarning($"[BossEnemy] stoppingDistance ({agent.stoppingDistance}) >= attackRange ({attackRange}). Lower stoppingDistance.");
                }
            }
            else
            {
                // Idle if cannot see
                StopMoving(false);
                anim.SetFloat(SpeedHash, 0f);
            }

            yield return wait;

            // Debug pulse: how close are we?
            // (Comment out if too noisy)
            // Debug.Log($"[BossEnemy] dist={dist:F2} remain={agent.remainingDistance:F2} stop={agent.stoppingDistance:F2} hasPath={agent.hasPath} pending={agent.pathPending} isStopped={agent.isStopped}");
        }
    }

    private void StopMoving(bool clearPath = true)
    {
        agent.isStopped = true;
        if (clearPath) agent.ResetPath();
    }

    private void ResumeChase()
    {
        // Called after attacks/casts: ensures we don't stay frozen
        agent.isStopped = false;
        if (player) agent.SetDestination(player.position);
    }

    private void OnDied()
    {
        if (_dead) return;
        _dead = true;

        // stop movement & AI
        CancelInvoke();           // kills pending ResumeChase etc.
        StopAllCoroutines();      // kills RepathLoop
        agent.isStopped = true;
        agent.ResetPath();

        // stop future hits/spells & clear in-flight triggers
        anim.ResetTrigger(AttackHash);
        anim.ResetTrigger(CastHash);
        anim.ResetTrigger(HitHash);
        anim.SetFloat(SpeedHash, 0f);

        // play death only
        // SAFER: ensure the "Die" state exists; name must match your Animator
        anim.SetTrigger(DieHash); // or anim.CrossFade("Die", 0.1f);

        // optional: prevent any colliders from causing more damage events
        // foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

        // optionally disable NavMeshAgent after a frame to avoid root-motion conflicts
        // StartCoroutine(DisableAgentNextFrame());

        Destroy(gameObject, 3f);
    }

    // If you prefer to centralize external kills:
    public void HandleExternalDeath()
    {
        if (_dead) return;
        OnDied();
    }


    // ===== Animation Events (hook these on your clips) =====
    public void AE_MeleeHit() { if (_dead) return; DoMeleeHit(); }
    public void AE_CastRelease()
    {
        if (_dead) return;
        var sp = GetComponent<SwordRainSpell>();
        if (sp != null) sp.OnCastEmit();
        else Debug.LogWarning("[BossEnemy] No SwordRainSpell on this Animator object.");
    }


    Collider[] _hitsBuf = new Collider[16];
    private void DoMeleeHit()
    {
        // Position the swing a bit in front of the enemy
        Vector3 center = transform.position + transform.forward * 1.2f;

        int found = Physics.OverlapSphereNonAlloc(center, meleeRadius, _hitsBuf, playerLayers);
        Debug.Log($"[BossEnemy] AE_MeleeHit → Overlap {found} colliders (mask={playerLayers.value}) at {center}, r={meleeRadius}");

        if (found == 0)
        {
            // Helpful hints if nothing was found
            Debug.LogWarning("[BossEnemy] Melee found no Player. Check: (1) Attack clip has AE_MeleeHit, (2) Player layer included in playerLayers, " +
                            "(3) meleeRadius big enough, (4) agent.stoppingDistance < attackRange.");
            return;
        }

        bool didDamage = false;
        for (int i = 0; i < found; i++)
        {
            var col = _hitsBuf[i];
            var dmg = col.GetComponentInParent<BossEnemyIDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(meleeDamage);
                didDamage = true;
                Debug.Log($"[BossEnemy] Melee hit {col.name} for {meleeDamage}");
            }
        }

        if (!didDamage)
            Debug.LogWarning("[BossEnemy] Overlap hit colliders, but none had BossEnemyIDamageable. Is BossEnemyPlayerHealth on the player root or parent?");
    }

    private void SpawnSwordRain()
    {     }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.2f, meleeRadius);
    }
   

}
