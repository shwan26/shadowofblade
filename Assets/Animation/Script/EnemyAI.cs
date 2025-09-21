// using UnityEngine;
// using UnityEngine.AI;

// public class EnemyAI : MonoBehaviour
// {
//     [Header("Target & Detection")]
//     public Transform target;
//     public float lookRadius = 10f;     // Detection radius
//     public float attackRadius = 2f;    // Attack range
//     public float attackCooldown = 2f;  // Time between attacks
//     public float attackDamage = 10f;
//     public Collider attackCollider;    // Optional collider for melee

//     [Header("Movement")]
//     public float agentMaxSpeed = 5f;

//     [Header("Bounds / Patrol")]
//     public Vector3 homePosition;       // Enemy spawn/home position
//     public float moveRadius = 5f;      // Maximum distance enemy can move from home

//     // ----- New header and variables for attack effects -----
//     [Header("Attack Effects")]
//     public GameObject attackEffectPrefab;
//     public AudioClip attackSound;

//     private NavMeshAgent agent;
//     private Animator animator;
//     private AudioSource audioSource;
//     // --------------------------------------------------------

//     private float distanceToTarget;
//     private float nextAttackTime = 0f;

//     void Awake()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         animator = GetComponent<Animator>();
//         agent.speed = agentMaxSpeed;
//         agent.isStopped = false;

//         // ----- New: Get the AudioSource component -----
//         audioSource = GetComponent<AudioSource>();
//         // ----------------------------------------------

//         // Set home position to the enemy's current position at Awake
//         if (homePosition == Vector3.zero)
//             homePosition = transform.position;

//         // Find player if not assigned
//         if (target == null)
//         {
//             GameObject player = GameObject.FindGameObjectWithTag("Player");
//             if (player != null)
//             {
//                 target = player.transform;
//             }
//         }
//     }

//     void Update()
//     {
//         if (target == null) return;

//         distanceToTarget = Vector3.Distance(target.position, transform.position);

//         // Debug logs
//         // Debug.Log($"Home: {homePosition}, Enemy: {transform.position}, Player: {target.position}, DistanceToTarget: {distanceToTarget}, PlayerInBounds: {IsPlayerInBounds()}");

//         // Attack
//         if (distanceToTarget <= attackRadius && IsPlayerInBounds())
//         {
//             agent.SetDestination(transform.position); // Stop moving
//             FaceTarget();

//             if (Time.time >= nextAttackTime)
//             {
//                 animator.SetTrigger("Attack");
//                 nextAttackTime = Time.time + attackCooldown;
//             }
//         }
//         // Chase player if inside bounds
//         else if (distanceToTarget <= lookRadius && IsPlayerInBounds())
//         {
//             agent.SetDestination(target.position);
//         }
//         // Player out of bounds or too far -> return home
//         else
//         {
//             float distanceToHome = Vector3.Distance(transform.position, homePosition);
//             if (distanceToHome > 0.1f)
//             {
//                 agent.SetDestination(homePosition);
//             }
//             else
//             {
//                 agent.SetDestination(transform.position);
//             }
//         }

//         // Update animator speed
//         animator.SetFloat("Speed", agent.velocity.magnitude / agentMaxSpeed);
//     }

//     // Check if player is within the enemy's bounded area
//     private bool IsPlayerInBounds()
//     {
//         if (target == null) return false;

//         // Use distance from homePosition instead of current enemy position
//         float distanceFromHomeToPlayer = Vector3.Distance(homePosition, target.position);

//         // Optional: clamp distance check to ignore Y-axis differences
//         // float distanceFromHomeToPlayer = Vector3.Distance(new Vector3(homePosition.x, 0, homePosition.z), new Vector3(target.position.x, 0, target.position.z));

//         return distanceFromHomeToPlayer <= moveRadius;
//     }

//     private void FaceTarget()
//     {
//         Vector3 direction = (target.position - transform.position).normalized;
//         Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
//         transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
//     }

//     public void DealDamageToTarget()
//     {
//         // ----- New: Instantiate effect and play sound -----
//         if (attackEffectPrefab != null && target != null)
//         {
//             Instantiate(attackEffectPrefab, target.position, target.rotation);
//         }

//         if (audioSource != null && attackSound != null)
//         {
//             audioSource.PlayOneShot(attackSound);
//         }
//         // ---------------------------------------------------

//         if (target != null)
//         {
//             Health targetHealth = target.GetComponent<Health>();
//             if (targetHealth != null)
//             {
//                 targetHealth.TakeDamage(attackDamage);
//             }
//         }
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.gameObject.CompareTag("Player"))
//         {
//             Health playerHealth = other.gameObject.GetComponent<Health>();
//             if (playerHealth != null)
//             {
//                 playerHealth.TakeDamage(attackDamage);
//                 if (attackCollider != null)
//                     attackCollider.enabled = false;
//             }
//         }
//     }

//     public void EnableDamageCollider()
//     {
//         if (attackCollider != null)
//             attackCollider.enabled = true;
//     }

//     public void DisableDamageCollider()
//     {
//         if (attackCollider != null)
//             attackCollider.enabled = false;
//     }

//     void OnDrawGizmosSelected()
//     {
//         // Look radius
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, lookRadius);

//         // Move bounds
//         Gizmos.color = Color.green;
//         Gizmos.DrawWireSphere(homePosition, moveRadius);

//         // Attack radius
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, attackRadius);
//     }
// }


using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Target & Detection")]
    public Transform target;
    public float lookRadius = 10f;
    public float attackRadius = 2f;
    public float attackCooldown = 2f;
    public float attackDamage = 10f;
    public Collider attackCollider;

    [Header("Movement")]
    public float agentMaxSpeed = 5f;

    [Header("Bounds / Patrol")]
    public Vector3 homePosition;
    public float moveRadius = 5f;

    [Header("Attack Effects")]
    public GameObject attackEffectPrefab;
    public AudioClip attackSound;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    private float distanceToTarget;
    private float nextAttackTime = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        agent.speed = agentMaxSpeed;
        agent.isStopped = false;

        // Unique home position for each enemy
        homePosition = transform.position;

        // Randomize avoidance priority to reduce clustering
        agent.avoidancePriority = Random.Range(30, 60);

        // Find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        distanceToTarget = Vector3.Distance(target.position, transform.position);

        // Attack
        if (distanceToTarget <= attackRadius && IsPlayerInBounds())
        {
            agent.SetDestination(transform.position);
            FaceTarget();

            if (Time.time >= nextAttackTime)
            {
                animator.SetTrigger("Attack");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // Chase player with random offset
        else if (distanceToTarget <= lookRadius && IsPlayerInBounds())
        {
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            agent.SetDestination(target.position + offset);
        }
        // Return home if player out of bounds
        else
        {
            float distanceToHome = Vector3.Distance(transform.position, homePosition);
            if (distanceToHome > 0.1f)
                agent.SetDestination(homePosition);
            else
                agent.SetDestination(transform.position);
        }

        // Update animator speed
        animator.SetFloat("Speed", agent.velocity.magnitude / agentMaxSpeed);
    }

    private bool IsPlayerInBounds()
    {
        if (target == null) return false;
        float distanceFromHomeToPlayer = Vector3.Distance(homePosition, target.position);
        return distanceFromHomeToPlayer <= moveRadius;
    }

    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void DealDamageToTarget()
    {
        if (attackEffectPrefab != null && target != null)
            Instantiate(attackEffectPrefab, target.position, target.rotation);

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        if (target != null)
        {
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
                targetHealth.TakeDamage(attackDamage);
        }
    }

    public void EnableDamageCollider()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    public void DisableDamageCollider()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(homePosition, moveRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}

