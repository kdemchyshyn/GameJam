using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player horizontal movement, jumping, and sprite flipping.
/// Uses the new Input System's direct keyboard API (no action maps needed).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Ground Check")]
    [Tooltip("Empty child transform placed at the character's feet.")]
    [SerializeField] private Transform groundCheck;

    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float moveInput;
    private bool isGrounded;

    // ── Public API ──────────────────────────────────────────────────────

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public bool IsGrounded => isGrounded;

    // ── Unity Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Freeze Z rotation so the player doesn't topple over
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    // ── Input ───────────────────────────────────────────────────────────

    private void ReadInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Horizontal
        moveInput = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveInput -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveInput += 1f;

        // Jump (Space, W, or Up Arrow)
        bool jumpPressed = kb.spaceKey.wasPressedThisFrame
                        || kb.wKey.wasPressedThisFrame
                        || kb.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // ── Movement ────────────────────────────────────────────────────────

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // ── Ground Detection ────────────────────────────────────────────────

    private void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            // Fallback if no ground check transform is assigned
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
        }
    }

    // ── Visuals ─────────────────────────────────────────────────────────

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;

        if (moveInput < 0f)
            spriteRenderer.flipX = true;
        else if (moveInput > 0f)
            spriteRenderer.flipX = false;
    }

    // ── Gizmos ──────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
