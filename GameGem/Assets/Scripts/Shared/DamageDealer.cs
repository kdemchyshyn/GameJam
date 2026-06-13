using UnityEngine;

/// <summary>
/// Deals damage on trigger contact. Attach to a GameObject with a 2D trigger collider.
/// Uses layer comparison to prevent friendly-fire (objects on the same layer won't damage each other).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DamageDealer : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;

    [Tooltip("If true, this GameObject is destroyed after dealing damage once.")]
    [SerializeField] private bool destroyOnHit;

    [Tooltip("Minimum time between consecutive hits on the same target.")]
    [SerializeField] private float hitCooldown = 0.5f;

    private float lastHitTime = -Mathf.Infinity;

    // ── Public API ──────────────────────────────────────────────────────

    public float DamageAmount
    {
        get => damageAmount;
        set => damageAmount = value;
    }

    // ── Collision ───────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Allow repeated contact damage (e.g. standing on an enemy)
        TryDealDamage(other);
    }

    private void TryDealDamage(Collider2D other)
    {
        // Don't damage objects on the same layer (no friendly fire)
        if (other.gameObject.layer == gameObject.layer) return;

        // Cooldown prevents damage spam
        if (Time.time < lastHitTime + hitCooldown) return;

        Health health = other.GetComponent<Health>();
        if (health == null) health = other.GetComponentInParent<Health>();

        if (health != null && !health.IsDead)
        {
            health.TakeDamage(damageAmount);
            lastHitTime = Time.time;

            if (destroyOnHit)
                Destroy(gameObject);
        }
    }
}
