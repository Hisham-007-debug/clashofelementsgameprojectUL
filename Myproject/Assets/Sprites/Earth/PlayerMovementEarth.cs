using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementEarth : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 3.2f;
    public float acceleration = 18f;
    public float deceleration = 22f;
    public float forwardJumpBoost = 1.8f;

    [Header("Jump Feel")]
    public float fallMultiplier = 3.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.35f;
    public LayerMask groundLayer;

    [Header("Hitbox References")]
    public HitboxController lightAttackHitbox;
    public HitboxController heavyAttackHitbox;

    [Header("Audio")]
    [SerializeField] private AudioClip punchSound;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private AudioSource audioSource;
    private Vector3 originalScale;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isInHitStun;
    private bool isBlocking;
    private bool isForwardJumping;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        originalScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        var actions = playerInput.actions;
        moveAction        = actions["Move"];
        jumpAction        = actions["Jump"];
        lightAttackAction = actions["LightAttack"];
        heavyAttackAction = actions["HeavyAttack"];
    }

    void OnEnable()
    {
        if (jumpAction != null)        jumpAction.performed        += OnJump;
        if (lightAttackAction != null) lightAttackAction.performed += OnLightAttack;
        if (heavyAttackAction != null) heavyAttackAction.performed += OnHeavyAttack;
        if (lightAttackHitbox != null) lightAttackHitbox.onSuccessfulHit += PlayPunchSound;
    }

    void OnDisable()
    {
        if (jumpAction != null)        jumpAction.performed        -= OnJump;
        if (lightAttackAction != null) lightAttackAction.performed -= OnLightAttack;
        if (heavyAttackAction != null) heavyAttackAction.performed -= OnHeavyAttack;
        if (lightAttackHitbox != null) lightAttackHitbox.onSuccessfulHit -= PlayPunchSound;

    }

    void Update()
    {
        // Ground check
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        animator.SetBool("IsGrounded", isGrounded);

        // Reset forward jump state on landing
        if (isGrounded)
            isForwardJumping = false;

        if (isInHitStun) return;

        // Movement
        if (moveAction != null)
            moveInput = moveAction.ReadValue<Vector2>();

        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsBlocking", isBlocking);

        // Flip sprite while keeping original scale
        if (moveInput.x > 0)
            transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z);
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(
                -Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z);
    }

    void FixedUpdate()
    {
        if (isInHitStun) return;

        // Smooth horizontal movement (ONLY on ground)
        if (isGrounded)
        {
            float targetSpeed = moveInput.x * moveSpeed;
            float speedDifference = targetSpeed - rb.linearVelocity.x;

            float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            float movementForce = speedDifference * accelRate;

            rb.AddForce(new Vector2(movementForce, 0f));

            // Clamp horizontal speed
            float clampedX = Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed);
            rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
        }

        // Better jump feel (faster falling)
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded && !isInHitStun)
        {
            // Moving jump = forward flip jump
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                rb.linearVelocity = new Vector2(moveInput.x * forwardJumpBoost, jumpForce);
                animator.SetTrigger("ForwardJump");
                isForwardJumping = true;
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, jumpForce);
                animator.SetTrigger("Jump");
                isForwardJumping = false;
            }
        }
    }

    private void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!isInHitStun)
            animator.SetTrigger("LightAttack");
    }

    private void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (!isInHitStun)
            animator.SetTrigger("HeavyAttack");
    }

    private void PlayPunchSound()
    {
        if (punchSound != null)
            audioSource.PlayOneShot(punchSound);
    }

    private void OnBlock(InputAction.CallbackContext ctx)
    {
        isBlocking = true;
    }

    private void OnBlockReleased(InputAction.CallbackContext ctx)
    {
        isBlocking = false;
    }

    // Called by Animation Events
    public void EnableLightHitbox()  { Debug.Log("[Earth] EnableLightHitbox fired"); lightAttackHitbox?.EnableHitbox(); }
    public void DisableLightHitbox() { lightAttackHitbox?.DisableHitbox(); }
    public void EnableHeavyHitbox()  { Debug.Log("[Earth] EnableHeavyHitbox fired"); heavyAttackHitbox?.EnableHitbox(); }
    public void DisableHeavyHitbox() { heavyAttackHitbox?.DisableHitbox(); }

    // Called by HitboxController when this fighter gets hit
    public void ApplyHitStun(float duration, float knockbackForce, Vector3 attackerPosition)
    {
        if (isBlocking)
        {
            float blockDir = transform.position.x > attackerPosition.x ? 1f : -1f;
            rb.AddForce(new Vector2(blockDir * knockbackForce * 0.3f, 0), ForceMode2D.Impulse);
            animator.SetTrigger("Block");
            return;
        }

        StartCoroutine(HitStunCoroutine(duration, knockbackForce, attackerPosition));
    }

    private IEnumerator HitStunCoroutine(float duration, float force, Vector3 attackerPos)
    {
        isInHitStun = true;
        animator.SetTrigger("Hit");

        float dir = transform.position.x > attackerPos.x ? 1f : -1f;
        rb.AddForce(new Vector2(dir * force, 1.5f), ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);
        isInHitStun = false;
    }

    // Draw ground check in Scene view
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}