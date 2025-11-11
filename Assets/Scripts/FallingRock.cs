using UnityEngine;

public class FallingRock : MonoBehaviour
{
    [Header("Detection Settings")]
    public Vector2 detectionBoxSize = new Vector2(4f, 8f);
    public Vector2 detectionBoxOffset = new Vector2(0f, -5f);
    public string playerTag = "Player";
    public LayerMask detectionMask = ~0; 

    [Header("Fall Settings")]
    public float fallDelay = 0.2f;
    public int damageAmount = 1;

    private Rigidbody2D rb;
    private bool hasTriggered = false;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (hasTriggered) return;

        Vector2 center = (Vector2)transform.position + detectionBoxOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, detectionBoxSize, 0f, detectionMask);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (!hit.CompareTag(playerTag)) continue;

            Debug.Log($"FallingRock triggered by: {hit.name}");
            TriggerFall();
            break;
        }
    }

    void TriggerFall()
    {
        hasTriggered = true;
        Invoke(nameof(StartFalling), fallDelay);
    }

    void StartFalling()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f; // Fall Speed
        isFalling = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling) return;

        // Deal damage to the player if it hits
        if (collision.gameObject.CompareTag(playerTag))
        {
            Debug.Log($"Rock hit player: {collision.gameObject.name}");

            CharacterMovement player = collision.gameObject.GetComponent<CharacterMovement>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("Player does not have a CharacterMovement component!");
            }
        }

        // Get rid of the rock after hitting something
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + detectionBoxOffset;
        Gizmos.DrawWireCube(center, detectionBoxSize);
    }
}