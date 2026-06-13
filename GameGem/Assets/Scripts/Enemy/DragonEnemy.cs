using UnityEngine;

/// <summary>
/// Flying enemy with powerful attacks. Uses zero-gravity Rigidbody2D and moves
/// toward the player with a sine-wave flight pattern for organic movement.
/// Patrols in a gentle loop when the player is out of range.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class DragonEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float detectionRange = 10f;

    [Tooltip("How far above the player the dragon prefers to hover.")]
    [SerializeField] private float hoverHeight = 2.5f;

    [Header("Flight Pattern")]
    [SerializeField] private float sineAmplitude = 1f;
    [SerializeField] private float sineFrequency = 2f;

    [Header("Idle Patrol")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolWidth = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector2 spawnPosition;
    private float sineTimer;
    private bool isChasing;

    // ── Unity Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Dragon flies — no gravity
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        spawnPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            isChasing = dist <= detectionRange;
        }

        sineTimer += Time.deltaTime;
        FlipSprite();
    }

    private void FixedUpdate()
    {
        if (isChasing && player != null)
            ChasePlayer();
        else
            IdlePatrol();
    }

    // ── Chase AI ────────────────────────────────────────────────────────

    private void ChasePlayer()
    {
        // Target position: above the player
        Vector2 target = new Vector2(
            player.position.x,
            player.position.y + hoverHeight);

        // Sine-wave overlay for organic flight
        target.y += Mathf.Sin(sineTimer * sineFrequency) * sineAmplitude;

        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    // ── Idle Patrol ─────────────────────────────────────────────────────

    private void IdlePatrol()
    {
        // Gentle loop around the spawn point
        float x = spawnPosition.x + Mathf.Sin(sineTimer * 0.5f) * patrolWidth;
        float y = spawnPosition.y + Mathf.Sin(sineTimer * sineFrequency) * sineAmplitude;

        Vector2 target = new Vector2(x, y);
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * patrolSpeed;
    }

    // ── Visuals ─────────────────────────────────────────────────────────

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;

        if (rb.linearVelocity.x < -0.1f)
            spriteRenderer.flipX = true;
        else if (rb.linearVelocity.x > 0.1f)
            spriteRenderer.flipX = false;
    }

    // ── Gizmos ──────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Hover height indicator
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 hoverPos = player.position + Vector3.up * hoverHeight;
            Gizmos.DrawWireCube(hoverPos, Vector3.one * 0.3f);
        }
    }
}
