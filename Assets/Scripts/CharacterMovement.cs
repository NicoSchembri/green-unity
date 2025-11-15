using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxJumpForce = 24f;
    public float jumpBuildSpeed = 5f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float coyoteTime = 0.1f;

    [Header("Health")]
    public int maxHearts = 5;
    public int currentHearts = 5;
    public HeartsUI heartsUI;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip runSound;
    [Range(0f, 1f)]
    public float runSoundVolume = 1f;

    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public Transform firePoint;

    [Header("Water Sword Settings")]
    public GameObject waterSwordPrefab;

    [Header("Rock Shield Settings")]
    public GameObject rockShieldPrefab;

    [Header("Animation Frames")]
    public Sprite idleSprite;
    public Sprite jumpSprite;
    public Sprite[] runSprites;
    public float frameRate = 0.1f;

    [Header("Spell UI Icons")]
    public Image fireballIcon;
    public Image waterSwordIcon;
    public Image rockShieldIcon;

    private Color availableColor = Color.white;
    private Color cooldownColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Cooldowns (1 second each)")]
    public float fireballCooldown = 1f;
    public float waterSwordCooldown = 1f;
    public float rockShieldCooldown = 1f;

    private float fireballCooldownTimer = 0f;
    private float waterSwordCooldownTimer = 0f;
    private float rockShieldCooldownTimer = 0f;

    // Spell unlock stuff
    public enum SpellType { Fireball, WaterSword, RockShield }

    private bool fireballUnlocked = false;
    private bool waterSwordUnlocked = false;
    private bool rockShieldUnlocked = false;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;

    private bool isGrounded;
    private float coyoteCounter;
    private bool jumpHeld;
    private bool isJumping;

    private bool facingRight = true;

    private float frameTimer;
    private int frameIndex;
    private AudioSource runAudioSource;

    private Vector3 spawnPoint;
    private Vector3 currentCheckpoint;

    private GameObject activeRockShield;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        spawnPoint = transform.position;
        currentCheckpoint = spawnPoint;

        if (col.sharedMaterial == null)
        {
            PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
            noFriction.friction = 0f;
            noFriction.bounciness = 0f;
            col.sharedMaterial = noFriction;
        }

        if (heartsUI != null)
            heartsUI.UpdateHearts(currentHearts);

        sr.sprite = idleSprite;

        PlayerPrefs.SetInt("Spell_Fireball", 0);
        PlayerPrefs.SetInt("Spell_WaterSword", 0);
        PlayerPrefs.SetInt("Spell_RockShield", 0);
        PlayerPrefs.Save();

        LoadSpellUnlocks();
        UpdateSpellIconVisibility();

        runAudioSource = gameObject.AddComponent<AudioSource>();
        runAudioSource.loop = true;
        runAudioSource.playOnAwake = false;
        runAudioSource.volume = runSoundVolume;
        if (runSound != null)
            runAudioSource.clip = runSound;
    }

    void Update()
    {
        // Cooldown timers
        fireballCooldownTimer -= Time.deltaTime;
        waterSwordCooldownTimer -= Time.deltaTime;
        rockShieldCooldownTimer -= Time.deltaTime;

        UpdateSpellIcons();

        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump") && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpHeld = true;
            coyoteCounter = 0f;

            if (jumpSound != null)
                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
        }

        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            isJumping = false;
        }

        // Fireball
        if (Input.GetKeyDown(KeyCode.E) &&
            fireballUnlocked &&
            fireballCooldownTimer <= 0f &&
            fireballPrefab != null &&
            firePoint != null &&
            activeRockShield == null)
        {
            ShootFireball();
            fireballCooldownTimer = fireballCooldown;
        }

        // Water Sword
        if (Input.GetKeyDown(KeyCode.Q) &&
            waterSwordUnlocked &&
            waterSwordCooldownTimer <= 0f &&
            waterSwordPrefab != null &&
            firePoint != null &&
            activeRockShield == null)
        {
            SwingWaterSword();
            waterSwordCooldownTimer = waterSwordCooldown;
        }

        // Rock Shield
        if (rockShieldUnlocked &&
            rockShieldCooldownTimer <= 0f &&
            rockShieldPrefab != null &&
            firePoint != null &&
            isGrounded)
        {
            if (Input.GetMouseButton(1))
            {
                if (activeRockShield == null)
                {
                    activeRockShield = Instantiate(rockShieldPrefab);
                    RockShield shield = activeRockShield.GetComponent<RockShield>();
                    if (shield != null)
                    {
                        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
                        shield.ownerTag = "Player";
                        shield.Activate(transform, firePoint, dir);
                    }
                }
            }
            else
            {
                if (activeRockShield != null)
                {
                    Destroy(activeRockShield);
                    activeRockShield = null;
                    rockShieldCooldownTimer = rockShieldCooldown;
                }
            }
        }
        else
        {
            if (activeRockShield != null)
            {
                Destroy(activeRockShield);
                activeRockShield = null;
                rockShieldCooldownTimer = rockShieldCooldown;
            }
        }
    }

    void FixedUpdate()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0 && !facingRight) Flip();
        else if (move < 0 && facingRight) Flip();

        if (jumpHeld && isJumping)
        {
            float newVelY = rb.linearVelocity.y + jumpBuildSpeed * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(newVelY, maxJumpForce));
        }

        UpdateSpriteAnimation(move);
    }

    private void UpdateSpriteAnimation(float move)
    {
        bool running = Mathf.Abs(move) > 0.1f;

        if (!isGrounded)
        {
            sr.sprite = jumpSprite;
            if (runAudioSource.isPlaying) runAudioSource.Stop();
            return;
        }

        if (running)
        {
            frameTimer += Time.fixedDeltaTime;
            if (frameTimer >= frameRate)
            {
                frameTimer = 0f;
                frameIndex = (frameIndex + 1) % runSprites.Length;
                sr.sprite = runSprites[frameIndex];
            }

            if (!runAudioSource.isPlaying && runSound != null)
                runAudioSource.Play();
        }
        else
        {
            sr.sprite = idleSprite;
            frameTimer = 0f;
            frameIndex = 0;

            if (runAudioSource.isPlaying)
                runAudioSource.Stop();
        }
    }

    private void UpdateSpellIcons()
    {
        if (fireballIcon != null)
            fireballIcon.color = fireballCooldownTimer > 0 ? cooldownColor : availableColor;

        if (waterSwordIcon != null)
            waterSwordIcon.color = waterSwordCooldownTimer > 0 ? cooldownColor : availableColor;

        if (rockShieldIcon != null)
            rockShieldIcon.color = rockShieldCooldownTimer > 0 ? cooldownColor : availableColor;
    }

    private void UpdateSpellIconVisibility()
    {
        if (fireballIcon != null)
            fireballIcon.gameObject.SetActive(fireballUnlocked);

        if (waterSwordIcon != null)
            waterSwordIcon.gameObject.SetActive(waterSwordUnlocked);

        if (rockShieldIcon != null)
            rockShieldIcon.gameObject.SetActive(rockShieldUnlocked);
    }

    private void ShootFireball()
    {
        GameObject obj = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fireball = obj.GetComponent<Fireball>();
        if (fireball == null) return;

        float dir = facingRight ? 1f : -1f;
        fireball.Launch(new Vector2(dir, 0f));
        fireball.ownerTag = "Player";

        Vector3 scale = obj.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        obj.transform.localScale = scale;
    }

    private void SwingWaterSword()
    {
        GameObject obj = Instantiate(waterSwordPrefab, firePoint.position, Quaternion.identity);
        WaterSword sword = obj.GetComponent<WaterSword>();
        if (!sword) return;

        float dir = facingRight ? 1f : -1f;
        sword.ownerTag = "Player";
        sword.Swing(transform, new Vector2(dir, 0f));
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D c) => CheckGround(c);
    private void OnCollisionStay2D(Collision2D c) => CheckGround(c);

    private void OnCollisionExit2D(Collision2D c)
    {
        if (((1 << c.gameObject.layer) & groundLayer) != 0)
            isGrounded = false;
    }

    private void CheckGround(Collision2D c)
    {
        if (((1 << c.gameObject.layer) & groundLayer) == 0) return;

        foreach (ContactPoint2D p in c.contacts)
        {
            if (p.normal.y > 0.9f && rb.linearVelocity.y <= 0f)
            {
                isGrounded = true;
                isJumping = false;
                return;
            }
        }
    }

    public void TakeDamage(int amt)
    {
        currentHearts = Mathf.Max(0, currentHearts - amt);
        heartsUI?.UpdateHearts(currentHearts);

        if (currentHearts <= 0) Die();
    }

    public void Heal(int amt)
    {
        currentHearts = Mathf.Min(maxHearts, currentHearts + amt);
        heartsUI?.UpdateHearts(currentHearts);
    }

    public void Bounce(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        isJumping = true;
        jumpHeld = false;
    }

    private void Die()
    {
        transform.position = currentCheckpoint;
        rb.linearVelocity = Vector2.zero;

        currentHearts = maxHearts;
        heartsUI?.UpdateHearts(currentHearts);

        isGrounded = false;
        isJumping = false;
        jumpHeld = false;
        coyoteCounter = coyoteTime;
    }

    public void SetCheckpoint(Vector3 pos) => currentCheckpoint = pos;

    public void UnlockSpell(SpellType spell)
    {
        switch (spell)
        {
            case SpellType.Fireball:
                fireballUnlocked = true;
                PlayerPrefs.SetInt("Spell_Fireball", 1);
                break;
            case SpellType.WaterSword:
                waterSwordUnlocked = true;
                PlayerPrefs.SetInt("Spell_WaterSword", 1);
                break;
            case SpellType.RockShield:
                rockShieldUnlocked = true;
                PlayerPrefs.SetInt("Spell_RockShield", 1);
                break;
        }

        PlayerPrefs.Save();
        UpdateSpellIconVisibility();
    }

    private void LoadSpellUnlocks()
    {
        fireballUnlocked = PlayerPrefs.GetInt("Spell_Fireball", 0) == 1;
        waterSwordUnlocked = PlayerPrefs.GetInt("Spell_WaterSword", 0) == 1;
        rockShieldUnlocked = PlayerPrefs.GetInt("Spell_RockShield", 0) == 1;

        UpdateSpellIconVisibility();

        Debug.Log($"Loaded spells - Fireball:{fireballUnlocked}, WaterSword:{waterSwordUnlocked}, RockShield:{rockShieldUnlocked}");
    }

    public bool IsSpellUnlocked(SpellType spell)
    {
        return spell switch
        {
            SpellType.Fireball => fireballUnlocked,
            SpellType.WaterSword => waterSwordUnlocked,
            SpellType.RockShield => rockShieldUnlocked,
            _ => false
        };
    }
}
