using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the player's evolution when HP drops below the threshold defined
/// in the Health component. Swaps the sprite to an evolved version and boosts
/// attack damage and movement speed. Fires once per lifetime.
/// </summary>
public class PlayerEvolution : MonoBehaviour
{
    [Header("Evolved Form")]
    [Tooltip("Drag the evolved character sprite here.")]
    [SerializeField] private Sprite evolvedSprite;

    [SerializeField] private float evolvedAttackDamage = 25f;
    [SerializeField] private float evolvedMoveSpeedBoost = 2f;

    [Header("Visual Feedback")]
    [SerializeField] private Color flashColor = new Color(1f, 0.85f, 0f); // gold
    [SerializeField] private float flashDuration = 0.6f;
    [SerializeField] private int flashCount = 4;

    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private PlayerCombat playerCombat;
    private Health health;
    private bool hasEvolved;

    // ── Unity Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
        playerCombat = GetComponent<PlayerCombat>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnThresholdReached.AddListener(Evolve);
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnThresholdReached.RemoveListener(Evolve);
    }

    // ── Evolution ───────────────────────────────────────────────────────

    private void Evolve()
    {
        if (hasEvolved) return;
        hasEvolved = true;

        // Swap sprite
        if (evolvedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = evolvedSprite;

        // Boost attack damage
        if (playerCombat != null)
            playerCombat.AttackDamage = evolvedAttackDamage;

        // Boost movement speed
        if (playerController != null)
            playerController.MoveSpeed += evolvedMoveSpeedBoost;

        // Eye-candy
        StartCoroutine(EvolutionFlashRoutine());

        Debug.Log("[PlayerEvolution] Player has evolved!");
    }

    // ── Visual Feedback ─────────────────────────────────────────────────

    private IEnumerator EvolutionFlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        float halfFlash = flashDuration / (flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(halfFlash);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(halfFlash);
        }
    }
}
