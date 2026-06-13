using UnityEngine;

/// <summary>
/// Fast, weak ground-based enemy. Patrols left/right and chases the player
/// when within detection range. Flips at walls and platform edges.
/// Pair with DamageDealer for contact damage.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ChickenEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detectionRange = 6f;

    [Header("Patrol")]
    [Tooltip("Empty child at the front, used to detect walls.")]
    [SerializeField] private Transform wallCheck;

    [Tooltip("Empty child at the front-bottom, used to detect platform edges.")]
    [SerializeField] private Transform edgeCheck;

    [SerializeField] private float checkDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private float patrolDirection = 1f;
    private bool isChasing;
    private float currentDirection;

    // ── Unity Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
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

        if (isChasing)
            currentDirection = Mathf.Sign(player.position.x - transform.position.x);
        else
            Patrol();

        FlipSprite();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentDirection * moveSpeed, rb.linearVelocity.y);
    }

    // ── Patrol AI ───────────────────────────────────────────────────────

    private void Patrol()
    {
        currentDirection = patrolDirection;

        // Wall ahead?
        bool hitWall = false;
        if (wallCheck != null)
        {
            hitWall = Physics2D.Raycast(
                wallCheck.position,
                Vector2.right * patrolDirection,
                checkDistance,
                groundLayer);
        }

        // Edge ahead? (no ground in front of feet)
        bool atEdge = false;
        if (edgeCheck != null)
        {
            atEdge = !Physics2D.Raycast(
                edgeCheck.position,
                Vector2.down,
                checkDistance,
                groundLayer);
        }

        if (hitWall || atEdge)
            patrolDirection *= -1f;
    }

    // ── Visuals ─────────────────────────────────────────────────────────

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = currentDirection < 0f;
    }

    // ── Gizmos ──────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Wall check ray
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + Vector3.right * patrolDirection * checkDistance);
        }

        // Edge check ray
        if (edgeCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                edgeCheck.position,
                edgeCheck.position + Vector3.down * checkDistance);
        }
    }
}
