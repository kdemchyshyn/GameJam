using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the Chicken → Dragon transformation. When the Health component's
/// threshold is reached, this plays a flash effect, spawns the Dragon prefab
/// at the chicken's position, transfers the remaining HP percentage, and
/// destroys the chicken.
/// </summary>
public class EnemyTransformation : MonoBehaviour
{
    [Header("Transformation")]
    [Tooltip("The Dragon prefab to spawn when the chicken's HP drops below the threshold.")]
    [SerializeField] private GameObject transformedPrefab;

    [Tooltip("Delay (seconds) after the flash before the actual spawn.")]
    [SerializeField] private float transformDelay = 0.3f;

    [Header("Visual Feedback")]
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.1f); // fiery orange
    [SerializeField] private int flashCount = 5;
    [SerializeField] private float flashDuration = 0.5f;

    private Health health;
    private SpriteRenderer spriteRenderer;
    private bool isTransforming;

    // ── Unity Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnThresholdReached.AddListener(OnThresholdReached);
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnThresholdReached.RemoveListener(OnThresholdReached);
    }

    // ── Transformation ──────────────────────────────────────────────────

    private void OnThresholdReached()
    {
        if (isTransforming) return;
        isTransforming = true;

        StartCoroutine(TransformSequence());
    }

    private IEnumerator TransformSequence()
    {
        // Flash effect
        if (spriteRenderer != null)
        {
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

        yield return new WaitForSeconds(transformDelay);

        // Spawn transformed entity
        if (transformedPrefab != null)
        {
            GameObject transformed = Instantiate(
                transformedPrefab,
                transform.position,
                Quaternion.identity);

            // Transfer remaining HP percentage so the dragon doesn't start at full health
            Health newHealth = transformed.GetComponent<Health>();
            if (newHealth != null && health != null)
            {
                newHealth.SetHealthPercent(health.HealthPercent);
            }

            Debug.Log("[EnemyTransformation] Chicken transformed into Dragon!");
        }
        else
        {
            Debug.LogWarning("[EnemyTransformation] No transformedPrefab assigned!");
        }

        // Destroy the original chicken
        Destroy(gameObject);
    }
}
