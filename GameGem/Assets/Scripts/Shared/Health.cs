using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable health component. Attach to any entity that can take damage.
/// Fires OnThresholdReached when HP drops below a configurable percentage (used
/// for player evolution and chicken→dragon transformation).
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Tooltip("Fraction (0-1) at which the threshold event fires. 0.1 = 10%.")]
    [SerializeField] private float thresholdPercent = 0.1f;

    [Header("Events")]
    public UnityEvent OnDamaged;
    public UnityEvent OnDied;
    public UnityEvent OnThresholdReached;

    /// <summary>Passes current health as a 0-1 ratio.</summary>
    public UnityEvent<float> OnHealthChanged;

    private float currentHealth;
    private bool thresholdTriggered;
    private bool isDead;

    // ── Public API ──────────────────────────────────────────────────────

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;

    // ── Unity Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // ── Damage / Heal ───────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnDamaged?.Invoke();
        OnHealthChanged?.Invoke(HealthPercent);

        // Threshold check — fires once while still alive
        if (!thresholdTriggered && currentHealth > 0f && HealthPercent <= thresholdPercent)
        {
            thresholdTriggered = true;
            OnThresholdReached?.Invoke();
        }

        if (currentHealth <= 0f)
        {
            isDead = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(HealthPercent);
    }

    // ── Initialization Helpers ──────────────────────────────────────────

    /// <summary>
    /// Set max health. Optionally resets current HP to the new max.
    /// </summary>
    public void SetMaxHealth(float newMax, bool fullHeal = false)
    {
        maxHealth = Mathf.Max(1f, newMax);
        if (fullHeal) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(HealthPercent);
    }

    /// <summary>
    /// Silently sets current health to a percentage of max, without firing events.
    /// Useful when transferring HP during transformations.
    /// </summary>
    public void SetHealthPercent(float percent)
    {
        currentHealth = maxHealth * Mathf.Clamp01(percent);
    }
}
