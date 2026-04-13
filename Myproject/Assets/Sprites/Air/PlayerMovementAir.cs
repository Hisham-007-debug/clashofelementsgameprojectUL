using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementAir : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Hitbox References")]
    public HitboxController lightAttackHitbox;
    public HitboxController heavyAttackHitbox;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private Vector3 originalScale;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isInHitStun;
    private bool isBlocking;
    private float groundedLogTimer;

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
    }

    void OnDisable()
    {
        if (jumpAction != null)        jumpAction.performed        -= OnJump;
        if (lightAttackAction != null) lightAttackAction.performed -= OnLightAttack;
        if (heavyAttackAction != null) heavyAttackAction.performed -= OnHeavyAttack;

    }

    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
        groundedLogTimer += Time.deltaTime;
        if (groundedLogTimer >= 10f) { Debug.Log("isGrounded: " + isGrounded); groundedLogTimer = 0f; }
        animator.SetBool("IsGrounded", isGrounded);

        if (isInHitStun) return;

        // Movement
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
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext ctx)
{
    if (isGrounded && !isInHitStun)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.SetTrigger("Jump");
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

    private void OnBlock(InputAction.CallbackContext ctx)
    {
        isBlocking = true;
    }

    private void OnBlockReleased(InputAction.CallbackContext ctx)
    {
        isBlocking = false;
    }

    // Called by Animation Events
    public void EnableLightHitbox()  => lightAttackHitbox?.EnableHitbox();
    public void DisableLightHitbox() => lightAttackHitbox?.DisableHitbox();
    public void EnableHeavyHitbox()  => heavyAttackHitbox?.EnableHitbox();
    public void DisableHeavyHitbox() => heavyAttackHitbox?.DisableHitbox();

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
        rb.AddForce(new Vector2(dir * force, 3f), ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);
        isInHitStun = false;
    }
}