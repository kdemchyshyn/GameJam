using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility that auto-creates all character GameObjects in the current scene
/// with components, colliders, child objects, and references fully wired up.
/// Run via the menu: GameGem → Setup Characters.
/// 
/// After running, assign your sprites in the Inspector:
///   - Player SpriteRenderer → your normal player sprite
///   - PlayerEvolution → Evolved Sprite field → your evolved player sprite
///   - Chicken SpriteRenderer → your chicken sprite
///   - Dragon prefab SpriteRenderer → your dragon sprite
/// </summary>
public class SceneSetup : EditorWindow
{
    [MenuItem("GameGem/Setup Characters")]
    public static void SetupCharacters()
    {
        // Ensure layers exist (they should from TagManager, but just in case)
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (playerLayer == -1 || enemyLayer == -1)
        {
            EditorUtility.DisplayDialog(
                "Layer Missing",
                "Player or Enemy layer not found.\n\n" +
                "Make sure TagManager.asset has 'Player' on layer 6 and 'Enemy' on layer 7.\n" +
                "Try closing and reopening the project if you just updated TagManager.",
                "OK");
            return;
        }

        CreateTestGround();
        CreatePlayer(playerLayer);
        GameObject dragonPrefab = CreateDragonPrefab(enemyLayer);
        CreateChicken(enemyLayer, dragonPrefab);

        Debug.Log("[SceneSetup] ✓ All characters created! Assign your sprites in the Inspector, then press Play.");
        EditorUtility.DisplayDialog(
            "Setup Complete",
            "Created: Player, Chicken, Dragon prefab, and Test Ground.\n\n" +
            "TODO:\n" +
            "1. Assign your sprites (Player, Player Evolved, Chicken, Dragon)\n" +
            "2. Set Main Camera → Ground Layer on PlayerController to include 'Default'\n" +
            "3. Press Play and test!\n\n" +
            "Controls:\n" +
            "  A/D or ←/→ = Move\n" +
            "  Space/W/↑ = Jump\n" +
            "  J / Enter / Left Click = Attack",
            "Got it!");
    }

    // ── Test Ground ─────────────────────────────────────────────────────

    private static void CreateTestGround()
    {
        // Skip if one already exists
        if (GameObject.Find("TestGround") != null) return;

        GameObject ground = new GameObject("TestGround");
        ground.transform.position = new Vector3(0f, -3f, 0f);

        // Visual (a stretched white sprite so you can see the ground)
        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = new Color(0.4f, 0.55f, 0.35f); // earthy green
        ground.transform.localScale = new Vector3(20f, 1f, 1f);

        // Collider
        BoxCollider2D col = ground.AddComponent<BoxCollider2D>();
        // collider auto-fits the sprite

        Undo.RegisterCreatedObjectUndo(ground, "Create TestGround");
    }

    // ── Player ──────────────────────────────────────────────────────────

    private static void CreatePlayer(int playerLayer)
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            Debug.LogWarning("[SceneSetup] A Player-tagged object already exists. Skipping.");
            return;
        }

        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = playerLayer;
        player.transform.position = new Vector3(-3f, 0f, 0f);

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = new Color(0.2f, 0.6f, 1f); // placeholder blue
        player.transform.localScale = new Vector3(1f, 1.5f, 1f);

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CapsuleCollider2D capsule = player.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.8f, 1f);

        // Scripts
        PlayerController controller = player.AddComponent<PlayerController>();
        PlayerCombat combat = player.AddComponent<PlayerCombat>();
        player.AddComponent<PlayerEvolution>();
        player.AddComponent<Health>();

        // Ground Check child
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        groundCheck.layer = playerLayer;

        // Wire ground check via serialized field
        SerializedObject soController = new SerializedObject(controller);
        soController.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        // Set ground layer to Default (layer 0)
        soController.FindProperty("groundLayer").intValue = 1 << 0; // Default layer
        soController.ApplyModifiedProperties();

        // Attack Hitbox child
        GameObject attackHitbox = new GameObject("AttackHitbox");
        attackHitbox.transform.SetParent(player.transform);
        attackHitbox.transform.localPosition = new Vector3(0.7f, 0f, 0f);
        attackHitbox.layer = playerLayer;

        BoxCollider2D hitboxCol = attackHitbox.AddComponent<BoxCollider2D>();
        hitboxCol.isTrigger = true;
        hitboxCol.size = new Vector2(0.8f, 0.8f);

        DamageDealer hitboxDamage = attackHitbox.AddComponent<DamageDealer>();

        // Wire attack hitbox references
        SerializedObject soCombat = new SerializedObject(combat);
        soCombat.FindProperty("attackHitbox").objectReferenceValue = attackHitbox;
        soCombat.FindProperty("damageDealer").objectReferenceValue = hitboxDamage;
        soCombat.ApplyModifiedProperties();

        attackHitbox.SetActive(false);

        Undo.RegisterCreatedObjectUndo(player, "Create Player");
    }

    // ── Dragon Prefab ───────────────────────────────────────────────────

    private static GameObject CreateDragonPrefab(int enemyLayer)
    {
        string prefabDir = "Assets/Prefabs";
        string prefabPath = prefabDir + "/Dragon.prefab";

        // Check if prefab already exists
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            Debug.Log("[SceneSetup] Dragon prefab already exists at " + prefabPath);
            return existingPrefab;
        }

        // Create directory if needed
        if (!AssetDatabase.IsValidFolder(prefabDir))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Build the Dragon GameObject
        GameObject dragon = new GameObject("Dragon");
        dragon.tag = "Enemy";
        dragon.layer = enemyLayer;

        SpriteRenderer sr = dragon.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = new Color(0.85f, 0.2f, 0.1f); // placeholder red
        dragon.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

        Rigidbody2D rb = dragon.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CapsuleCollider2D capsule = dragon.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.8f, 0.9f);

        BoxCollider2D triggerCol = dragon.AddComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector2(1f, 1f);

        dragon.AddComponent<DragonEnemy>();

        Health dragonHealth = dragon.AddComponent<Health>();
        SerializedObject soHealth = new SerializedObject(dragonHealth);
        soHealth.FindProperty("maxHealth").floatValue = 80f;
        soHealth.ApplyModifiedProperties();

        DamageDealer dd = dragon.AddComponent<DamageDealer>();
        SerializedObject soDD = new SerializedObject(dd);
        soDD.FindProperty("damageAmount").floatValue = 20f;
        soDD.ApplyModifiedProperties();

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(dragon, prefabPath);
        Object.DestroyImmediate(dragon);

        Debug.Log("[SceneSetup] Created Dragon prefab at " + prefabPath);
        return prefab;
    }

    // ── Chicken ─────────────────────────────────────────────────────────

    private static void CreateChicken(int enemyLayer, GameObject dragonPrefab)
    {
        if (GameObject.Find("Chicken") != null)
        {
            Debug.LogWarning("[SceneSetup] A Chicken already exists. Skipping.");
            return;
        }

        GameObject chicken = new GameObject("Chicken");
        chicken.tag = "Enemy";
        chicken.layer = enemyLayer;
        chicken.transform.position = new Vector3(4f, 0f, 0f);

        SpriteRenderer sr = chicken.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = new Color(1f, 0.85f, 0.2f); // placeholder yellow
        chicken.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        Rigidbody2D rb = chicken.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CapsuleCollider2D capsule = chicken.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.9f, 1f);

        BoxCollider2D triggerCol = chicken.AddComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector2(1f, 1f);

        // Scripts
        ChickenEnemy chickenAI = chicken.AddComponent<ChickenEnemy>();

        Health chickenHealth = chicken.AddComponent<Health>();
        SerializedObject soHealth = new SerializedObject(chickenHealth);
        soHealth.FindProperty("maxHealth").floatValue = 50f;
        soHealth.ApplyModifiedProperties();

        EnemyTransformation transformation = chicken.AddComponent<EnemyTransformation>();
        SerializedObject soTrans = new SerializedObject(transformation);
        soTrans.FindProperty("transformedPrefab").objectReferenceValue = dragonPrefab;
        soTrans.ApplyModifiedProperties();

        DamageDealer dd = chicken.AddComponent<DamageDealer>();
        SerializedObject soDD = new SerializedObject(dd);
        soDD.FindProperty("damageAmount").floatValue = 5f;
        soDD.ApplyModifiedProperties();

        // WallCheck child
        GameObject wallCheck = new GameObject("WallCheck");
        wallCheck.transform.SetParent(chicken.transform);
        wallCheck.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        wallCheck.layer = enemyLayer;

        // EdgeCheck child
        GameObject edgeCheck = new GameObject("EdgeCheck");
        edgeCheck.transform.SetParent(chicken.transform);
        edgeCheck.transform.localPosition = new Vector3(0.5f, -0.5f, 0f);
        edgeCheck.layer = enemyLayer;

        // Wire checks
        SerializedObject soChicken = new SerializedObject(chickenAI);
        soChicken.FindProperty("wallCheck").objectReferenceValue = wallCheck.transform;
        soChicken.FindProperty("edgeCheck").objectReferenceValue = edgeCheck.transform;
        soChicken.FindProperty("groundLayer").intValue = 1 << 0; // Default layer
        soChicken.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(chicken, "Create Chicken");
    }
}
