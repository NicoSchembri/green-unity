using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Fireball : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage = 1;
    public string ownerTag;

    [Header("Audio")]
    public AudioClip launchSound;
    [Range(0f, 1f)] public float launchVolume = 1f;

    private Rigidbody2D rb;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;

        if (Mathf.Abs(dir.y) > 0.01f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (launchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(launchSound, launchVolume);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ownerTag))
            return;

        bool shouldDestroy = false;

        if (ownerTag == "Player" && collision.CompareTag("Enemy"))
        {
            BossController boss = collision.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }
            else
            {
                Destroy(collision.gameObject); 
            }
            shouldDestroy = true;
        }
        else if (ownerTag == "Enemy" && collision.CompareTag("Player"))
        {
            CharacterMovement character = collision.GetComponent<CharacterMovement>();
            if (character != null)
                character.TakeDamage(damage);
            shouldDestroy = true;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            shouldDestroy = true;
        }

        if (shouldDestroy)
            Destroy(gameObject);
    }
}
