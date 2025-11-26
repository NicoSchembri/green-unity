using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class WaterSword : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;
    public float duration = 0.3f;
    public float swingDistance = 1.2f;
    public string ownerTag;

    [Header("Offsets")]
    public Vector2 playerOffset = Vector2.zero;
    public Vector2 bossOffset = new Vector2(1f, 0.5f);

    [Header("Audio")]
    public AudioClip swingSound;
    [Range(0f, 1f)] public float swingVolume = 1f;

    private Collider2D col;
    private SpriteRenderer sr;
    private Transform owner;
    private Vector2 dir;

    private AudioSource audioSource;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.isTrigger = true;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void Swing(Transform ownerTransform, Vector2 swingDir)
    {
        owner = ownerTransform;
        dir = swingDir.normalized;

        Vector2 offset = Vector2.zero;

        if (ownerTag == "Player")
            offset = playerOffset;
        else if (ownerTag == "Enemy")
        {
            offset = bossOffset;
            offset.x *= Mathf.Sign(dir.x);
        }

        transform.position = (Vector2)owner.position + offset + dir * swingDistance;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir.x);
        transform.localScale = scale;

        sr.flipX = dir.x < 0;

        if (swingSound != null && audioSource != null)
            audioSource.PlayOneShot(swingSound, swingVolume);

        Destroy(gameObject, duration);
    }

    private void Update()
    {
        if (owner != null)
        {
            Vector2 offset = Vector2.zero;

            if (ownerTag == "Player")
                offset = playerOffset;
            else if (ownerTag == "Enemy")
            {
                offset = bossOffset;
                offset.x *= Mathf.Sign(dir.x);
            }

            transform.position = (Vector2)owner.position + offset + dir * swingDistance;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ownerTag))
            return;

        if (ownerTag == "Player" && collision.CompareTag("Enemy"))
        {
            BossController boss = collision.GetComponent<BossController>();
            if (boss != null)
                boss.TakeDamage(damage);
            else
                Destroy(collision.gameObject);
        }
        else if (ownerTag == "Enemy" && collision.CompareTag("Player"))
        {
            CharacterMovement character = collision.GetComponent<CharacterMovement>();
            if (character != null)
                character.TakeDamage(damage);
        }
    }
}
