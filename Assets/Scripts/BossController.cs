using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    [Header("Health & Stage")]
    public int stage1Health = 5;
    public int stage2Health = 10;
    public int stage3Health = 15;
    private int currentHealth;
    private int currentStage = 1;

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

    [Header("Abilities")]
    public GameObject fireballPrefab;
    public GameObject waterSlashPrefab;
    public GameObject lightningPrefab;
    public Transform firePoint;
    public float fireballCooldown = 2f;
    public float waterSlashCooldown = 3f;
    public float lightningCooldown = 5f;
    private float fireballTimer = 0f;
    private float waterSlashTimer = 0f;
    private float lightningTimer = 0f;

    [Header("Lightning Settings")]
    public float lightningHeight = 5f;
    public float lightningXRange = 2f;
    public int stage3LightningCount = 2;
    public int stage3_5LightningCount = 3;
    public float multiStrikeDelay = 0.2f;

    [Header("UI & Dialogue")]
    public BossHealthUI healthUI;
    public BossDialogueManager dialogueManager;

    [Header("Player Reference")]
    public Transform player;

    [Header("State")]
    public bool isActive = false;
    public bool isFighting = false;
    public bool invincible = false;

    // Stage flags
    private bool stage2Done = false;
    private bool stage3Done = false;
    private bool defeatDone = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = stage1Health;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = idleSprite;

        if (healthUI != null)
        {
            healthUI.InitializeBoss(currentStage, currentHealth);
            healthUI.HideBossUI();
        }
    }

    private void Update()
    {
        if (!isActive || player == null) return;

        if (isFighting)
        {
            HandleMovement();
            HandleAbilities();
        }

        HandleStageProgression();
    }

    private void HandleMovement()
    {
        Vector3 direction = player.position - transform.position;
        float moveAmount = Mathf.Sign(direction.x) * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(moveAmount, 0, 0);

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
                if (waterSlashTimer >= waterSlashCooldown) { WaterSlash(); waterSlashTimer = 0f; }
                if (lightningTimer >= lightningCooldown)
                {
                    StartCoroutine(MultiLightningAttack(stage3LightningCount));
                    lightningTimer = 0f;
                }
                break;
            case 4:
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

    private void Fireball()
    {
        if (firePoint == null || fireballPrefab == null || player == null) return;
        Vector2 dir = (player.position - firePoint.position).normalized;
        GameObject fb = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        fb.GetComponent<Fireball>().ownerTag = "Enemy";
        fb.GetComponent<Fireball>().Launch(dir);
    }

    private void WaterSlash()
    {
        if (waterSlashPrefab == null || firePoint == null) return;
        Vector3 spawnPos = firePoint.position;
        Vector2 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        GameObject ws = Instantiate(waterSlashPrefab, spawnPos, Quaternion.identity);
        if (ws.TryGetComponent(out WaterSword wsScript))
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
        ln.GetComponent<Lightning>()?.Strike(spawnPos, "Enemy", Random.value > 0.5f);
    }

    private IEnumerator MultiLightningAttack(int count)
    {
        for (int i = 0; i < count; i++)
        {
            LightningAttack();
            if (i < count - 1) yield return new WaitForSeconds(multiStrikeDelay);
        }
    }

    private void HandleStageProgression()
    {
        if (currentStage == 1 && currentHealth <= 0 && !stage2Done)
        {
            stage2Done = true;
            StartCoroutine(TransitionToStage2());
        }
        else if (currentStage == 2 && currentHealth <= 0 && !stage3Done)
        {
            stage3Done = true;
            StartCoroutine(TransitionToStage3());
        }
        else if ((currentStage == 3 || currentStage == 4) && currentHealth <= 0 && !defeatDone)
        {
            defeatDone = true;
            StartCoroutine(BossDefeatSequence());
        }
    }

    public void TakeDamage(int amount)
    {
        if (invincible) return;
        currentHealth -= amount;
        UpdateHealthUI();
    }

    private IEnumerator TransitionToStage2()
    {
        StopFighting();
        if (dialogueManager != null)
            yield return StartCoroutine(dialogueManager.PlayStage2Dialogue());

        currentStage = 2;
        currentHealth = stage2Health;
        ScaleDifficulty();
        UpdateHealthUI();
        ResumeFighting();
    }

    private IEnumerator TransitionToStage3()
    {
        StopFighting();
        if (dialogueManager != null)
            yield return StartCoroutine(dialogueManager.PlayStage3Dialogue());

        currentStage = 3;
        currentHealth = stage3Health;
        ScaleDifficulty();
        UpdateHealthUI();
        ResumeFighting();
    }

    private IEnumerator BossDefeatSequence()
    {
        StopFighting();

        if (dialogueManager != null)
            yield return StartCoroutine(dialogueManager.PlayDefeatDialogue());

        healthUI?.HideBossUI();

        // Small delay so effects can finish
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);

        // Load End Screen
        SceneManager.LoadScene("EndScreen");
    }

    public void StopFighting()
    {
        isFighting = false;
        invincible = true;
        sr.sprite = idleSprite;
    }

    public void ResumeFighting()
    {
        isFighting = true;
        invincible = false;
    }

    public void ActivateBoss()
    {
        isActive = true;
        healthUI?.ShowBossUI();
    }

    private void ScaleDifficulty()
    {
        moveSpeed *= stageMultiplier;
        fireballCooldown /= stageMultiplier;
        waterSlashCooldown /= stageMultiplier;
        lightningCooldown /= stageMultiplier;
    }

    private void UpdateHealthUI()
    {
        if (healthUI == null) return;

        int maxHP = currentStage switch
        {
            1 => stage1Health,
            2 => stage2Health,
            3 or 4 => stage3Health,
            _ => stage1Health
        };

        healthUI.UpdateBossHealth(currentStage, currentHealth, maxHP);
    }
}
