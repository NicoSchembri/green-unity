using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CharacterMovement_HoldJump : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float maxJumpForce = 24f; // double the jump force
    public float jumpBuildSpeed = 5f; // rate at which jump increases per second

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float coyoteTime = 0.1f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isGrounded;
    private float coyoteCounter;
    private bool jumpHeld;
    private bool isJumping;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Zero friction to prevent side sticking
        if (col.sharedMaterial == null)
        {
            PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
            noFriction.friction = 0f;
            noFriction.bounciness = 0f;
            col.sharedMaterial = noFriction;
        }
    }

    void Update()
    {
        // Update coyote timer
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Start jump
        if (Input.GetButtonDown("Jump") && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpHeld = true;
            coyoteCounter = 0f;
        }

        // Detect release of jump button
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            isJumping = false;
        }
    }

    void FixedUpdate()
    {
        // Horizontal movement
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Variable jump while holding
        if (jumpHeld && isJumping)
        {
            float newVelocityY = rb.linearVelocity.y + jumpBuildSpeed * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(newVelocityY, maxJumpForce));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

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
}
