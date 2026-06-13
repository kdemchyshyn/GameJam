using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Key-press melee attack system. Activates a child hitbox for a brief duration
/// on each attack. The hitbox should have a trigger Collider2D + DamageDealer.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Tooltip("How long the attack hitbox stays active per swing.")]
    [SerializeField] private float attackDuration = 0.15f;

    [Header("References")]
    [Tooltip("Child GameObject with a trigger Collider2D and DamageDealer.")]
    [SerializeField] private GameObject attackHitbox;

    [SerializeField] private DamageDealer damageDealer;

    private float lastAttackTime = -Mathf.Infinity;
    private float hitboxTimer;
    private bool hitboxActive;

    // ── Public API ──────────────────────────────────────────────────────

    public float AttackDamage
    {
        get => attackDamage;
        set
        {
            attackDamage = value;
            if (damageDealer != null)
                damageDealer.DamageAmount = value;
        }
    }

    // ── Unity Lifecycle ─────────────────────────────────────────────────

    private void Start()
    {
        // Make sure hitbox starts hidden
        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        // Sync initial damage value
        if (damageDealer != null)
            damageDealer.DamageAmount = attackDamage;
    }

    private void Update()
    {
        HandleAttackInput();
        UpdateHitboxTimer();
    }

    // ── Input ───────────────────────────────────────────────────────────

    private void HandleAttackInput()
    {
        bool attackPressed = false;

        Keyboard kb = Keyboard.current;
        if (kb != null)
            attackPressed = kb.jKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame;

        Mouse mouse = Mouse.current;
        if (mouse != null && !attackPressed)
            attackPressed = mouse.leftButton.wasPressedThisFrame;

        if (attackPressed && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }
    }

    // ── Attack Logic ────────────────────────────────────────────────────

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            hitboxActive = true;
            hitboxTimer = attackDuration;
        }
    }

    private void UpdateHitboxTimer()
    {
        if (!hitboxActive) return;

        hitboxTimer -= Time.deltaTime;
        if (hitboxTimer <= 0f)
        {
            hitboxActive = false;
            if (attackHitbox != null)
                attackHitbox.SetActive(false);
        }
    }
}
