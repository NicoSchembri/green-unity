using UnityEngine;
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

    // Spell unlock stuff
    public enum SpellType
    {
        Fireball,
        WaterSword,
        RockShield
    }

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

    private Vector3 spawnPoint;
    private Vector3 currentCheckpoint;

    private GameObject activeRockShield;

    private void Awake()
    {
        SpellBook.ResetAllUnlocks();
    }

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

        LoadSpellUnlocks();
    }

    void Update()
    {
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Jump input
        if (Input.GetButtonDown("Jump") && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpHeld = true;
            coyoteCounter = 0f;
        }

        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            isJumping = false;
        }

        if (Input.GetKeyDown(KeyCode.E) && fireballUnlocked && fireballPrefab != null && firePoint != null && activeRockShield == null)
        {
            ShootFireball();
        }

        if (Input.GetKeyDown(KeyCode.Q) && waterSwordUnlocked && waterSwordPrefab != null && firePoint != null && activeRockShield == null)
        {
            SwingWaterSword();
        }

        if (rockShieldUnlocked && rockShieldPrefab != null && firePoint != null && isGrounded)
        {
            if (Input.GetMouseButton(1))
            {
                if (activeRockShield == null)
                {
                    activeRockShield = Instantiate(rockShieldPrefab);
                    RockShield shield = activeRockShield.GetComponent<RockShield>();
                    if (shield != null)
                    {
                        Vector2 facingDir = facingRight ? Vector2.right : Vector2.left;
                        shield.ownerTag = "Player";
                        shield.Activate(transform, firePoint, facingDir);
                    }
                }
            }
            else
            {
                if (activeRockShield != null)
                {
                    Destroy(activeRockShield);
                    activeRockShield = null;
                }
            }
        }
        else
        {
            if (activeRockShield != null)
            {
                Destroy(activeRockShield);
                activeRockShield = null;
            }
        }
    }

    void FixedUpdate()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0 && !facingRight)
            Flip();
        else if (move < 0 && facingRight)
            Flip();

        if (jumpHeld && isJumping)
        {
            float newVelocityY = rb.linearVelocity.y + jumpBuildSpeed * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(newVelocityY, maxJumpForce));
        }

        UpdateSpriteAnimation(move);
    }

    private void UpdateSpriteAnimation(float move)
    {
        bool running = Mathf.Abs(move) > 0.1f;

        if (!isGrounded)
        {
            sr.sprite = jumpSprite;
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
        }
        else
        {
            sr.sprite = idleSprite;
            frameTimer = 0f;
            frameIndex = 0;
        }
    }

    private void ShootFireball()
    {
        if (fireballPrefab == null || firePoint == null)
            return;

        GameObject fireballObj = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fireball = fireballObj.GetComponent<Fireball>();
        if (fireball == null)
            return;

        float dir = facingRight ? 1f : -1f;
        fireball.Launch(new Vector2(dir, 0f));
        fireball.ownerTag = "Player";

        // Flip the sprite horizontally based on direction
        Vector3 scale = fireballObj.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        fireballObj.transform.localScale = scale;
    }

    private void SwingWaterSword()
    {
        if (waterSwordPrefab == null || firePoint == null)
            return;

        GameObject swordObj = Instantiate(waterSwordPrefab, firePoint.position, Quaternion.identity);
        WaterSword sword = swordObj.GetComponent<WaterSword>();
        if (sword == null)
            return;

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

    private void OnCollisionEnter2D(Collision2D collision) => CheckGroundCollision(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckGroundCollision(collision);

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            isGrounded = false;
    }

    private void CheckGroundCollision(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) == 0)
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.9f && rb.linearVelocity.y <= 0f)
            {
                isGrounded = true;
                isJumping = false;
                return;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHearts = Mathf.Max(0, currentHearts - amount);
        if (heartsUI != null)
            heartsUI.UpdateHearts(currentHearts);

        if (currentHearts <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHearts = Mathf.Min(maxHearts, currentHearts + amount);
        if (heartsUI != null)
            heartsUI.UpdateHearts(currentHearts);
    }

    public void Bounce(float bounceForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
        isJumping = true;
        jumpHeld = false;
    }

    private void Die()
    {
        transform.position = currentCheckpoint;
        rb.linearVelocity = Vector2.zero;

        currentHearts = maxHearts;
        if (heartsUI != null)
            heartsUI.UpdateHearts(currentHearts);

        isGrounded = false;
        isJumping = false;
        jumpHeld = false;
        coyoteCounter = coyoteTime;
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        currentCheckpoint = checkpointPosition;
    }

    // Spell system
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
    }

    private void LoadSpellUnlocks()
    {
        fireballUnlocked = PlayerPrefs.GetInt("Spell_Fireball", 0) == 1;
        waterSwordUnlocked = PlayerPrefs.GetInt("Spell_WaterSword", 0) == 1;
        rockShieldUnlocked = PlayerPrefs.GetInt("Spell_RockShield", 0) == 1;

        Debug.Log($"Loaded spells - Fireball: {fireballUnlocked}, WaterSword: {waterSwordUnlocked}, RockShield: {rockShieldUnlocked}");
    }

    public bool IsSpellUnlocked(SpellType spell)
    {
        switch (spell)
        {
            case SpellType.Fireball:
                return fireballUnlocked;
            case SpellType.WaterSword:
                return waterSwordUnlocked;
            case SpellType.RockShield:
                return rockShieldUnlocked;
            default:
                return false;
        }
    }
}