using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementEarth : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public bool isGrounded;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private PlayerInput playerInput;

    // Input action references (auto-wired)
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private InputAction blockAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        var actions = playerInput.actions;
        moveAction       = actions["Move"];
        jumpAction       = actions["Jump"];
        lightAttackAction  = actions["LightAttack"];
        heavyAttackAction  = actions["HeavyAttack"];
        blockAction      = actions["Block"];
    }

    void OnEnable()
    {
        jumpAction.performed        += OnJump;
        lightAttackAction.performed += OnLightAttack;
        heavyAttackAction.performed += OnHeavyAttack;
        blockAction.performed       += OnBlock;
    }

    void OnDisable()
    {
        jumpAction.performed        -= OnJump;
        lightAttackAction.performed -= OnLightAttack;
        heavyAttackAction.performed -= OnHeavyAttack;
        blockAction.performed       -= OnBlock;
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGrounded", isGrounded);

        // Flip sprite to face opponent
        if (moveInput.x != 0)
            transform.localScale = new Vector3(
                moveInput.x > 0 ? 1 : -1, 1, 1);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    private void OnLightAttack(InputAction.CallbackContext ctx)
    {
        animator.SetTrigger("LightAttack");
        // TODO: enable hitbox collider for a few frames
    }

    private void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        animator.SetTrigger("HeavyAttack");
    }

    private void OnBlock(InputAction.CallbackContext ctx)
    {
        animator.SetBool("Blocking", ctx.ReadValueAsButton());
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground")) isGrounded = false;
    }
}
