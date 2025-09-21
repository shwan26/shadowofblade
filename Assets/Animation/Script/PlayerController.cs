using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float runSpeed = 10f;
    public float sprintSpeed = 15f;

    [Header("Jumping")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float jumpTimeout = 0.1f;
    public float fallTimeout = 0.2f;

    [Header("Attacking")]
    public float attackMoveSpeed = 3f;
    public float comboResetTime = 1.5f;
    public float comboWindow = 0.5f;
    public LayerMask enemyLayers;
    public float attackRange = 0.5f;
    public Transform attackPoint;
    public int attackDamage = 20;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    [Header("Attack Effects")]
    public GameObject attackEffectPrefab;
    public float effectFadeOutTime = 0.5f; // Time for particles to fade out after the attack ends

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip jumpSound;

    // --- New header for footstep audio clips ---
    [Header("Footstep Audio")]
    public AudioClip leftFootstepSound;
    public AudioClip rightFootstepSound;

    // --- New variables for separate footstep audio volume controls ---
    [Range(0f, 1f)] public float leftFootstepVolume = 0.5f;
    [Range(0f, 1f)] public float rightFootstepVolume = 0.5f;

    // ----- New variables for audio volume controls -----
    [Range(0f, 1f)] public float attackVolume = 0.5f;
    [Range(0f, 1f)] public float jumpVolume = 0.5f;

    public GameObject jumpEffectPrefab;

    private CharacterController controller;
    private Animator anim;
    private AudioSource audioSource;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping = false;

    // Attack variables
    private int currentAttack = 0;
    private float lastAttackTime = 0f;
    private float lastComboTime = 0f;
    private bool isAttacking = false;
    private bool canCombo = false;

    private GameObject currentEffect;

    // Jump/fall timers
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    // Animator IDs
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDSpeed;
    private int animIDStrafe;
    private int animIDSprinting;
    private int animIDAttackCombo;
    private int animIDIsAttacking;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        AssignAnimationIDs();

        audioSource = GetComponent<AudioSource>();

        // ----- Debug Code: Check if AudioSource is found -----
        if (audioSource == null)
        {
            Debug.LogError("PlayerController: AudioSource component not found on this GameObject. Please add one to play attack sounds.");
        }
        else
        {
            Debug.Log("PlayerController: AudioSource component found.");
        }
        // -----------------------------------------------------
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
        Vector3 moveDirection = new Vector3(h, 0, v).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W);
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool attackPressed = Input.GetMouseButtonDown(0);

        GroundedCheck();
        JumpAndGravity(jumpPressed);
        HandleAttack(attackPressed);

        float targetSpeed = HandleMovement(moveDirection, isRunning, isSprinting);

        if (moveDirection.magnitude > 0.1f && !isAttacking)
        {
            Quaternion newRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
        }

        controller.Move(velocity * Time.deltaTime);

        UpdateAnimator(moveDirection, isSprinting, targetSpeed);
    }

    private void AssignAnimationIDs()
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

    private void GroundedCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (wasGrounded != isGrounded && isGrounded && !isJumping)
        {
            anim.SetTrigger("Land");
        }
    }

    private float HandleMovement(Vector3 moveDirection, bool isRunning, bool isSprinting)
    {
        float targetSpeed = isAttacking ? attackMoveSpeed : GetTargetSpeed(isRunning, isSprinting);

        if (moveDirection.magnitude >= 0.1f && !isAttacking)
        {
            controller.Move(moveDirection * targetSpeed * Time.deltaTime);
        }

        return targetSpeed;
    }

    private void UpdateAnimator(Vector3 moveDirection, bool isSprinting, float targetSpeed)
    {
        float forwardAmount = Vector3.Dot(moveDirection, transform.forward);
        float strafeAmount = Vector3.Dot(moveDirection, transform.right);
        float speedRatio = targetSpeed / sprintSpeed;
        float targetForward = moveDirection.magnitude > 0.1f ? forwardAmount * speedRatio : 0f;
        float targetStrafe = moveDirection.magnitude > 0.1f ? strafeAmount * speedRatio : 0f;

        if (isAttacking)
        {
            targetForward *= 0.3f;
            targetStrafe *= 0.3f;
        }

        anim.SetFloat(animIDSpeed, targetForward, 0.2f, Time.deltaTime);
        anim.SetFloat(animIDStrafe, targetStrafe, 0.2f, Time.deltaTime);
        anim.SetBool(animIDGrounded, isGrounded);
        anim.SetBool(animIDSprinting, isSprinting);
        anim.SetBool(animIDIsAttacking, isAttacking);
    }

    private void HandleAttack(bool attackPressed)
    {
        if (Time.time - lastAttackTime > comboResetTime)
        {
            currentAttack = 0;
        }

        if (attackPressed && !isAttacking)
        {
            StartAttack();
        }
        else if (attackPressed && isAttacking && canCombo)
        {
            QueueCombo();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        currentAttack++;
        if (currentAttack > 3) currentAttack = 1;

        anim.SetInteger(animIDAttackCombo, currentAttack);
        anim.CrossFade("Attack" + currentAttack, 0.05f);

        lastAttackTime = Time.time;
        lastComboTime = Time.time;
        canCombo = false;
    }

    private void QueueCombo()
    {
        if (Time.time - lastComboTime < comboWindow)
        {
            currentAttack++;
            if (currentAttack > 3) currentAttack = 1;

            anim.SetInteger(animIDAttackCombo, currentAttack);
            anim.CrossFade("Attack" + currentAttack, 0.1f);

            lastComboTime = Time.time;
            canCombo = false;
        }
    }

    public void PerformAttack()
    {
        if (attackEffectPrefab != null && attackPoint != null)
        {
            currentEffect = Instantiate(attackEffectPrefab, attackPoint.position, attackPoint.rotation);
        }

        if (audioSource != null && attackSound != null)
        {
            // ----- Debug Code: Check if the sound clip is assigned -----
            Debug.Log("Playing attack sound: " + attackSound.name);
            // -----------------------------------------------------------
            audioSource.PlayOneShot(attackSound, attackVolume);
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("PlayerController: Cannot play sound. AudioSource is missing or unassigned.");
        }
        else if (attackSound == null)
        {
            Debug.LogWarning("PlayerController: Cannot play sound. Attack Sound audio clip is not assigned.");
        }

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void EnableCombo() => canCombo = true;

    public void EndAttack()
    {
        isAttacking = false;
        canCombo = false;

        if (currentEffect != null)
        {
            ParticleSystem ps = currentEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
            }

            Destroy(currentEffect, effectFadeOutTime);
            currentEffect = null;
        }
    }

    private void JumpAndGravity(bool jumpPressed)
    {
        if (isGrounded)
        {
            fallTimeoutDelta = fallTimeout;

            if (velocity.y < 0.0f)
                velocity.y = -2f;

            if (jumpPressed && jumpTimeoutDelta <= 0.0f && !isAttacking)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                anim.SetTrigger(animIDJump);
                isJumping = true;

                // --- New: Play jump sound and instantiate jump effect ---
                if (audioSource != null && jumpSound != null)
                {
                    audioSource.PlayOneShot(jumpSound, jumpVolume);
                }
                if (jumpEffectPrefab != null)
                {
                    // Instantiate the effect at the player's position
                    GameObject jumpEffect = Instantiate(jumpEffectPrefab, transform.position, Quaternion.identity);
                    // Destroy the effect after a short duration to clean up the scene
                    Destroy(jumpEffect, 1f);
                }
            }

            if (jumpTimeoutDelta >= 0.0f)
                jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            jumpTimeoutDelta = jumpTimeout;

            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                anim.SetBool(animIDFreeFall, true);
            }

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

    // --- New: Methods for left and right footstep sounds ---
    public void PlayLeftFootstepSound()
    {
        if (anim.GetFloat(animIDSpeed) > 0.1f)
        {
            if (audioSource != null && leftFootstepSound != null)
            {
                audioSource.PlayOneShot(leftFootstepSound, leftFootstepVolume);
            }
        }
    }

    public void PlayRightFootstepSound()
    {
        if (anim.GetFloat(animIDSpeed) > 0.1f)
        {
            if (audioSource != null && rightFootstepSound != null)
            {
                audioSource.PlayOneShot(rightFootstepSound, rightFootstepVolume);
            }
        }
    }

    float GetTargetSpeed(bool isRunning, bool isSprinting)
    {
        if (isSprinting) return sprintSpeed;
        if (isRunning) return runSpeed;
        return walkSpeed;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
