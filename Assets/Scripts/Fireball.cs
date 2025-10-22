using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Fireball : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage = 1;
    public string ownerTag; // So it will be either the player or 

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;

        // Rotate sprite to face movement direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ownerTag))
            return;

        if (ownerTag == "Player" && collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        else if (ownerTag == "Enemy" && collision.CompareTag("Player"))
        {
            CharacterMovement character = collision.GetComponent<CharacterMovement>();
            if (character != null)
                character.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
