using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class BossEnemyPlayerController : MonoBehaviour
{
    // Animator routing
    [Header("Animator Layer Routing")]
    [Tooltip("Animator layer index that contains the attack states (your 'Attack Layer' = 1).")]
    [SerializeField] private int attackLayerIndex = 1;
    [Tooltip("Optional sub-state machine path. Leave empty if states are at the layer root.")]
    [SerializeField] private string attackSubPath = "";

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float runSpeed = 10f;
    public float sprintSpeed = 15f;
    public float rotationSpeed = 10f;

    [Header("Jumping")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float jumpTimeout = 0.1f;
    public float fallTimeout = 0.2f;

    [Header("Attacking")]
    public float attackMoveSpeed = 3f;
    public float comboResetTime = 1.5f;
    public float comboWindow = 0.5f;
    public Transform attackPoint;
    public float attackRange = 1.0f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    [Header("VFX / SFX")]
    public GameObject attackEffectPrefab;
    public float effectFadeOutTime = 0.5f;
    public AudioClip attackSound;
    public AudioClip jumpSound;
    [Range(0, 1)] public float attackVolume = 0.5f;
    [Range(0, 1)] public float jumpVolume = 0.5f;

    [Header("Footsteps")]
    public AudioClip leftFootstepSound;
    public AudioClip rightFootstepSound;
    [Range(0, 1)] public float leftFootstepVolume = 0.5f;
    [Range(0, 1)] public float rightFootstepVolume = 0.5f;

    [Header("Animator State Names (exact on the Attack Layer)")]
    public string attackState1 = "OneHand_Up_Attack_1_InPlace";
    public string attackState2 = "OneHand_Up_Attack_2_InPlace";
    public string attackState3 = "OneHand_Up_Attack_3_InPlace";

    [Header("Debug")]
    public bool debugLogs = true;

    // components
    CharacterController controller;
    Animator anim;
    AudioSource audioSource;

    // movement
    Vector3 velocity;
    bool isGrounded;
    bool isJumping;

    // attack
    int currentAttack = 0;
    float lastAttackTime = 0f;
    float lastComboTime = 0f;
    bool isAttacking = false;
    bool canCombo = false;
    GameObject currentEffect;

    // timers
    float jumpTimeoutDelta;
    float fallTimeoutDelta;

    // animator ids
    int animIDGrounded;
    int animIDJump;
    int animIDFreeFall;
    int animIDSpeed;
    int animIDStrafe;
    int animIDSprinting;
    int animIDAttackCombo;
    int animIDIsAttacking;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        AssignAnimationIDs();

        if (!audioSource) Debug.LogError("BossEnemyPlayerController: AudioSource missing.");
        else if (debugLogs) Debug.Log("BossEnemyPlayerController: AudioSource component found.");
    }

    void Start()
    {
        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W);
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool attackPressed = Input.GetMouseButtonDown(0);

        GroundedCheck();
        JumpAndGravity(jumpPressed);
        HandleAttack(attackPressed);

        float targetSpeed = HandleMovement(moveDir, isRunning, isSprinting);

        if (moveDir.magnitude > 0.1f && !isAttacking)
        {
            Quaternion look = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationSpeed);
        }

        controller.Move(velocity * Time.deltaTime);
        UpdateAnimator(moveDir, isSprinting, targetSpeed);
    }

    void AssignAnimationIDs()
    {
        animIDGrounded = Animator.StringToHash("IsGrounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDSpeed = Animator.StringToHash("Speed");
        animIDStrafe = Animator.StringToHash("Strafe");
        animIDSprinting = Animator.StringToHash("IsSprinting");
        animIDAttackCombo = Animator.StringToHash("AttackCombo");
        animIDIsAttacking = Animator.StringToHash("IsAttacking");
    }

    void GroundedCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (wasGrounded != isGrounded && isGrounded && !isJumping)
            anim.SetTrigger("Land");
    }

    float HandleMovement(Vector3 moveDir, bool isRunning, bool isSprinting)
    {
        float targetSpeed = isAttacking ? attackMoveSpeed :
            (isSprinting ? sprintSpeed : (isRunning ? runSpeed : walkSpeed));

        if (moveDir.magnitude >= 0.1f && !isAttacking)
            controller.Move(moveDir * targetSpeed * Time.deltaTime);

        return targetSpeed;
    }

    void UpdateAnimator(Vector3 moveDir, bool isSprinting, float targetSpeed)
    {
        float forward = Vector3.Dot(moveDir, transform.forward);
        float strafe = Vector3.Dot(moveDir, transform.right);
        float speedRatio = sprintSpeed > 0f ? targetSpeed / sprintSpeed : 0f;

        float tF = moveDir.magnitude > 0.1f ? forward * speedRatio : 0f;
        float tS = moveDir.magnitude > 0.1f ? strafe * speedRatio : 0f;

        if (isAttacking) { tF *= 0.3f; tS *= 0.3f; }

        anim.SetFloat(animIDSpeed, tF, 0.2f, Time.deltaTime);
        anim.SetFloat(animIDStrafe, tS, 0.2f, Time.deltaTime);
        anim.SetBool(animIDGrounded, isGrounded);
        anim.SetBool(animIDSprinting, isSprinting);
        anim.SetBool(animIDIsAttacking, isAttacking);
    }

    // --- CrossFade helper to target layer 1 with full-path hash ---
    bool TryCrossFadeExact(string shortStateName, float transition = 0.05f)
    {
        string layerName = anim.GetLayerName(attackLayerIndex); // e.g., "Attack Layer"
        string path = string.IsNullOrEmpty(attackSubPath)
            ? $"{layerName}.{shortStateName}"
            : $"{layerName}.{attackSubPath.Trim('/').Replace('/', '.')}.{shortStateName}";

        int fullPathHash = Animator.StringToHash(path);
        if (!anim.HasState(attackLayerIndex, fullPathHash))
        {
            Debug.LogError($"Animator state not found on layer {attackLayerIndex}: '{path}'.");
            return false;
        }
        anim.CrossFade(fullPathHash, transition, attackLayerIndex, 0f);
        return true;
    }

    void HandleAttack(bool attackPressed)
    {
        if (Time.time - lastAttackTime > comboResetTime)
            currentAttack = 0;

        if (attackPressed && !isAttacking) StartAttack();
        else if (attackPressed && isAttacking && canCombo) QueueCombo();
    }

    string GetAttackStateName(int idx)
    {
        if (idx == 1) return attackState1;
        if (idx == 2) return attackState2;
        return attackState3;
    }

    void StartAttack()
    {
        isAttacking = true;
        currentAttack = (currentAttack % 3) + 1;

        anim.SetInteger(animIDAttackCombo, currentAttack);

        var state = GetAttackStateName(currentAttack);
        TryCrossFadeExact(state, 0.05f);

        lastAttackTime = Time.time;
        lastComboTime = Time.time;
        canCombo = false;
    }

    void QueueCombo()
    {
        if (Time.time - lastComboTime >= comboWindow) return;

        currentAttack = (currentAttack % 3) + 1;
        anim.SetInteger(animIDAttackCombo, currentAttack);

        var state = GetAttackStateName(currentAttack);
        TryCrossFadeExact(state, 0.10f);

        lastComboTime = Time.time;
        canCombo = false;
    }

    // --- Animation Events ---
    public void OnAttackHit() => PerformAttack();
    public void EnableCombo() => canCombo = true;
    public void DisableCombo() => canCombo = false;

    public void EndAttack()
    {
        isAttacking = false;
        canCombo = false;

        if (currentEffect)
        {
            var ps = currentEffect.GetComponent<ParticleSystem>();
            if (ps) ps.Stop();
            Destroy(currentEffect, effectFadeOutTime);
            currentEffect = null;
        }
    }

    void JumpAndGravity(bool jumpPressed)
    {
        if (isGrounded)
        {
            fallTimeoutDelta = fallTimeout;

            if (velocity.y < 0f) velocity.y = -2f;

            if (jumpPressed && jumpTimeoutDelta <= 0f && !isAttacking)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                anim.SetTrigger(animIDJump);
                isJumping = true;

                if (audioSource && jumpSound) audioSource.PlayOneShot(jumpSound, jumpVolume);
            }

            if (jumpTimeoutDelta >= 0f) jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            jumpTimeoutDelta = jumpTimeout;
            if (fallTimeoutDelta >= 0f) fallTimeoutDelta -= Time.deltaTime;
            else anim.SetBool(animIDFreeFall, true);
            isJumping = false;
        }

        if (velocity.y > 0 || !isGrounded)
            velocity.y += gravity * Time.deltaTime;
    }

    public void OnLandEvent()
    {
        anim.SetBool(animIDFreeFall, false);
        isJumping = false;
    }

    // Footstep events (no params)
    public void PlayLeftFootstepSound()
    {
        if (anim.GetFloat(animIDSpeed) > 0.1f && audioSource && leftFootstepSound)
            audioSource.PlayOneShot(leftFootstepSound, leftFootstepVolume);
    }
    public void PlayRightFootstepSound()
    {
        if (anim.GetFloat(animIDSpeed) > 0.1f && audioSource && rightFootstepSound)
            audioSource.PlayOneShot(rightFootstepSound, rightFootstepVolume);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        if (groundCheck)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
    
    // Add near your other fields in BossEnemyPlayerController
    // In BossEnemyPlayerController.cs
    public Transform hitStart;         // hand bone
    public Transform hitEnd;           // sword tip
    public float capsuleRadius = 0.6f;
    public bool debugHitScan = true;

    public void PerformAttack()
    {
        // VFX/SFX unchanged
        if (attackEffectPrefab && attackPoint)
            currentEffect = Instantiate(attackEffectPrefab, attackPoint.position, attackPoint.rotation);

        if (audioSource && attackSound)
        {
            if (debugLogs) Debug.Log("Playing attack sound: " + attackSound.name);
            audioSource.PlayOneShot(attackSound, attackVolume);
        }

        // === Build capsule endpoints ===
        Vector3 p0, p1;
        bool haveAssignedCapsule = (hitStart && hitEnd);
        if (haveAssignedCapsule)
        {
            p0 = hitStart.position;
            p1 = hitEnd.position;
        }
        else
        {
            // Auto fallback: build a short capsule in front of the player
            Vector3 origin = attackPoint ? attackPoint.position : transform.position + transform.forward * 0.8f;
            p0 = origin - transform.right * 0.25f;
            p1 = origin + transform.right * 0.25f;
            if (debugHitScan) Debug.LogWarning("[Attack] Using auto capsule (assign hitStart/hitEnd to improve accuracy).");
        }

        if (debugHitScan)
        {
            Debug.DrawLine(p0, p1, Color.magenta, 0.75f);
            Debug.DrawRay((p0 + p1) * 0.5f, transform.forward * capsuleRadius, Color.magenta, 0.75f);
        }

        // === Primary pass: enemy layer(s) only ===
        int hitsCount = 0;
        var hits = Physics.OverlapCapsule(p0, p1, capsuleRadius, enemyLayers, QueryTriggerInteraction.Collide);
        foreach (var col in hits)
        {
            var boss = col.GetComponentInParent<BossEnemyHealth>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                hitsCount++;
                if (debugHitScan) Debug.Log($"[Attack] Capsule hit {col.name} → {boss.gameObject.name}");
            }
            else
            {
                var idmg = col.GetComponentInParent<BossEnemyIDamageable>();
                if (idmg != null)
                {
                    idmg.TakeDamage(attackDamage);
                    hitsCount++;
                    if (debugHitScan) Debug.Log($"[Attack] Capsule hit IDamageable {col.name}");
                }
            }
        }

        // === No hits? Diagnose aggressively ===
        if (hitsCount == 0 && debugHitScan)
        {
            // 1) All-layers scan to see what's physically inside the volume
            var all = Physics.OverlapCapsule(p0, p1, capsuleRadius, ~0, QueryTriggerInteraction.Collide);
            if (all.Length > 0)
            {
                string names = string.Join(", ",
                    System.Array.ConvertAll(all, c => $"{c.name}[{LayerMask.LayerToName(c.gameObject.layer)}]"));
                Debug.LogWarning($"[Attack] Capsule (all layers) contained: {names}");

                // 2) Is the boss nearby but just outside? Find closest BossEnemyHealth and print distance
                var allBoss = GameObject.FindObjectsOfType<BossEnemyHealth>();
                float bestDist = float.MaxValue;
                BossEnemyHealth closest = null;
                Vector3 capsuleMid = (p0 + p1) * 0.5f;
                foreach (var b in allBoss)
                {
                    float d = Vector3.Distance(capsuleMid, b.transform.position);
                    if (d < bestDist) { bestDist = d; closest = b; }
                }
                if (closest != null)
                {
                    Debug.LogWarning($"[Attack] Closest boss '{closest.name}' is {bestDist:F2}m from capsule center. " +
                                    $"Increase capsuleRadius or move/assign hitStart/hitEnd.");
                }
                else
                {
                    Debug.LogWarning("[Attack] No BossEnemyHealth found in scene. Add BossEnemyHealth to the boss root.");
                }

                // 3) Mask check reminder
                Debug.LogWarning("[Attack] If the boss is inside but not counted, ensure 'enemyLayers' includes the Enemy layer.");
            }
            else
            {
                Debug.LogWarning("[Attack] Capsule contained NOTHING. You're too far or the capsule is misplaced. " +
                                "Assign hitStart/hitEnd to the hand & tip, increase capsuleRadius, or get closer.");
            }
        }

        if (debugLogs) Debug.Log($"[Player] Attack hit {hitsCount} targets.");
    }


}
