using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Health & Stage")]
    public int stage1Health = 2;
    public int stage2Health = 4;
    public int stage3Health = 6;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stageMultiplier = 1.2f;
    private bool facingRight = true;

    [Header("Animation")]
    public Sprite idleSprite;
    public Sprite[] runSprites;
    public float frameRate = 0.1f;
    private SpriteRenderer sr;
    private float frameTimer;
    private int frameIndex;

    private Transform player;
    private bool isActive = false;

    [Header("Abilities")]
    public GameObject fireballPrefab;
    public GameObject waterSlashPrefab;
    public GameObject lightningPrefab;
    public float fireballCooldown = 2f;
    public float waterSlashCooldown = 3f;
    public float lightningCooldown = 5f;
    private float fireballTimer = 0f;
    private float waterSlashTimer = 0f;
    private float lightningTimer = 0f;

    private int currentStage = 1;

    [Header("Lightning Settings")]
    public float lightningHeight = 5f;
    public float lightningXRange = 2f;
    public int stage3LightningCount = 2;    // Number of strikes in stage 3
    public int stage3_5LightningCount = 3;  // Number of strikes in stage 3.5
    public float multiStrikeDelay = 0.2f;   // Delay between multiple strikes

    [Header("Ability Spawn Points")]
    public Transform firePoint;

    [Header("UI")]
    public BossHealthUI healthUI;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = stage1Health;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = idleSprite;

        // Initialize health UI
        if (healthUI != null)
        {
            healthUI.InitializeBoss(currentStage, currentHealth);
        }
    }

    private void Update()
    {
        if (!isActive || player == null) return;

        HandleMovement();
        HandleAbilities();
        HandleStageProgression();
    }

    private void HandleMovement()
    {
        Vector3 direction = player.position - transform.position;
        float moveAmount = Mathf.Sign(direction.x) * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(moveAmount, 0, 0);

        // Flip using SpriteRenderer instead of scaling
        if (direction.x > 0 && !facingRight) Flip();
        else if (direction.x < 0 && facingRight) Flip();

        AnimateMovement(Mathf.Abs(direction.x));
    }

    private void AnimateMovement(float horizontal)
    {
        if (horizontal > 0.1f)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameRate)
            {
                frameTimer = 0f;
                frameIndex = (frameIndex + 1) % runSprites.Length;
                sr.sprite = runSprites[frameIndex];
            }
        }
        else
        {
            sr.sprite = idleSprite;
            frameTimer = 0f;
            frameIndex = 0;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        sr.flipX = !facingRight;

        // Flip fire point
        if (firePoint != null)
        {
            Vector3 fp = firePoint.localPosition;
            fp.x = -fp.x;
            firePoint.localPosition = fp;
        }
    }

    private void HandleAbilities()
    {
        fireballTimer += Time.deltaTime;
        waterSlashTimer += Time.deltaTime;
        lightningTimer += Time.deltaTime;

        switch (currentStage)
        {
            case 1:
                if (fireballTimer >= fireballCooldown) { Fireball(); fireballTimer = 0f; }
                if (waterSlashTimer >= waterSlashCooldown) { WaterSlash(); waterSlashTimer = 0f; }
                break;

            case 2:
                if (waterSlashTimer >= waterSlashCooldown) { WaterSlash(); waterSlashTimer = 0f; }
                if (lightningTimer >= lightningCooldown) { LightningAttack(); lightningTimer = 0f; }
                break;

            case 3:
                // Stage 3 uses Stage 2's attacks
                if (waterSlashTimer >= waterSlashCooldown) { WaterSlash(); waterSlashTimer = 0f; }
                if (lightningTimer >= lightningCooldown)
                {
                    StartCoroutine(MultiLightningAttack(stage3LightningCount));
                    lightningTimer = 0f;
                }
                break;

            case 4: // Stage 3.5
                if (fireballTimer >= fireballCooldown) { Fireball(); fireballTimer = 0f; }
                if (waterSlashTimer >= waterSlashCooldown) { WaterSlash(); waterSlashTimer = 0f; }
                if (lightningTimer >= lightningCooldown)
                {
                    StartCoroutine(MultiLightningAttack(stage3_5LightningCount));
                    lightningTimer = 0f;
                }
                break;
        }
    }

    private void HandleStageProgression()
    {
        if (currentStage == 1 && currentHealth <= 0)
        {
            currentStage = 2;
            currentHealth = stage2Health;
            ScaleDifficulty();
            UpdateHealthUI();
        }
        else if (currentStage == 2 && currentHealth <= 0)
        {
            currentStage = 3;
            currentHealth = stage3Health;
            ScaleDifficulty();
            UpdateHealthUI();
        }
        else if (currentStage == 3 && currentHealth <= stage3Health / 2) // Transition to 3.5 at half health
        {
            if (currentStage != 4) // Only transition once
            {
                currentStage = 4; // Internal stage 3.5
                ScaleDifficulty();
                UpdateHealthUI();
            }
        }

        if (currentHealth <= 0 && (currentStage == 3 || currentStage == 4))
        {
            if (healthUI != null)
                healthUI.HideBossUI();
            Destroy(gameObject);
        }
    }

    private void ScaleDifficulty()
    {
        moveSpeed *= stageMultiplier;
        fireballCooldown /= stageMultiplier;
        waterSlashCooldown /= stageMultiplier;
        lightningCooldown /= stageMultiplier;
    }

    private void Fireball()
    {
        if (firePoint == null || fireballPrefab == null || player == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject fb = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fbScript = fb.GetComponent<Fireball>();
        fbScript.ownerTag = "Enemy";

        fbScript.Launch(dir);
    }

    private void WaterSlash()
    {
        if (waterSlashPrefab == null) return;

        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;

        Vector2 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        GameObject ws = Instantiate(waterSlashPrefab, spawnPos, Quaternion.identity);
        WaterSword wsScript = ws.GetComponent<WaterSword>();
        if (wsScript != null)
        {
            wsScript.ownerTag = "Enemy";
            wsScript.Swing(transform, dir);
        }
    }

    private void LightningAttack()
    {
        if (lightningPrefab == null || player == null) return;

        float randomX = Random.Range(-lightningXRange, lightningXRange);
        Vector3 spawnPos = new Vector3(player.position.x + randomX, player.position.y + lightningHeight, 0f);

        GameObject ln = Instantiate(lightningPrefab, spawnPos, Quaternion.identity);
        Lightning lnScript = ln.GetComponent<Lightning>();
        if (lnScript != null)
        {
            lnScript.Strike(spawnPos, "Enemy", false);
        }
    }

    private System.Collections.IEnumerator MultiLightningAttack(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSingleLightning();
            if (i < count - 1) // Don't delay after the last strike
            {
                yield return new WaitForSeconds(multiStrikeDelay);
            }
        }
    }

    private void SpawnSingleLightning()
    {
        if (lightningPrefab == null || player == null) return;

        // Random position around player
        float randomX = Random.Range(-lightningXRange, lightningXRange);
        Vector3 spawnPos = new Vector3(
            player.position.x + randomX,
            player.position.y + lightningHeight,
            0f
        );

        // Random flip for variety
        bool shouldFlip = Random.value > 0.5f;

        GameObject ln = Instantiate(lightningPrefab, spawnPos, Quaternion.identity);
        Lightning lnScript = ln.GetComponent<Lightning>();
        if (lnScript != null)
        {
            lnScript.Strike(spawnPos, "Enemy", shouldFlip);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UpdateHealthUI();
    }

    public void ActivateBoss()
    {
        isActive = true;
        if (healthUI != null)
        {
            healthUI.ShowBossUI();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthUI != null)
        {
            int maxHP = currentStage switch
            {
                1 => stage1Health,
                2 => stage2Health,
                3 or 4 => stage3Health, // Both use same max health
                _ => stage1Health
            };

            healthUI.UpdateBossHealth(currentStage, currentHealth, maxHP);
        }
    }
}