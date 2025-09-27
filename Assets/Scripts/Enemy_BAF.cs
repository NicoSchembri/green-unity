using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy_BAF : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    private int moveDirection = -1;

    private Rigidbody2D rb;
    private BoxCollider2D col;

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    [Header("Flip Settings")]
    public float flipCooldown = 0.2f;
    private float lastFlipTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
        lastFlipTime = -flipCooldown;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

        if (col == null) col = GetComponent<BoxCollider2D>();

        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * moveDirection * col.bounds.extents.x + Vector2.down * (col.bounds.extents.y + 0.05f);
        RaycastHit2D groundInfo = Physics2D.Raycast(rayOrigin, Vector2.down, 0.2f, groundLayer);

        if (groundInfo.collider == null)
            TryFlip();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision, true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision, false); 
    }

    private void HandleCollision(Collision2D collision, bool canDamage)
    {
        // Flip the bug on walls
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                TryFlip();
                break;
            }
        }

        // Player collision section
        if (collision.gameObject.CompareTag("Player"))
        {
            CharacterMovement player = collision.gameObject.GetComponent<CharacterMovement>();
            if (player != null)
            {
                bool stomped = false;

                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f)
                    {
                        stomped = true;
                        break;
                    }
                }

                if (stomped)
                {
                    player.Bounce(player.jumpForce / 1.5f);
                    Die();
                }
                else if (canDamage)
                {
                    player.TakeDamage(1);
                }
            }
        }
    }

    private void TryFlip()
    {
        if (Time.time - lastFlipTime >= flipCooldown)
        {
            FlipDirection();
            lastFlipTime = Time.time;
        }
    }

    private void FlipDirection()
    {
        moveDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Die()
    {
        Debug.Log("Goomba squashed!");
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (col == null) return;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * moveDirection * col.bounds.extents.x + Vector2.down * (col.bounds.extents.y + 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * 0.2f);
    }
}
