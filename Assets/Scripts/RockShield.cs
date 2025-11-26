using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class RockShield : MonoBehaviour
{
    [Header("Settings")]
    public int maxDurability = 3;
    public float shieldDistance = 1f;
    public string ownerTag;

    [Header("Audio")]
    public AudioClip activateSound;
    public AudioClip hitSound;
    [Range(0f, 1f)] public float audioVolume = 1f;

    private int currentDurability;
    private Collider2D col;
    private SpriteRenderer sr;
    private Transform owner;
    private Transform firePoint;
    private Vector2 dir;

    private AudioSource audioSource;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.isTrigger = true;
        currentDurability = maxDurability;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void Activate(Transform ownerTransform, Transform firePointTransform, Vector2 facingDir)
    {
        owner = ownerTransform;
        firePoint = firePointTransform;
        dir = facingDir.normalized;

        transform.position = owner.position + (Vector3)(dir * shieldDistance);
        transform.rotation = Quaternion.identity;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1);
        transform.localScale = scale;

        if (activateSound != null)
            audioSource.PlayOneShot(activateSound, audioVolume);
    }

    private void Update()
    {
        if (owner != null)
        {
            transform.position = owner.position + (Vector3)(dir * shieldDistance);
            transform.rotation = Quaternion.identity;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ownerTag))
            return;

        Fireball fireball = collision.GetComponent<Fireball>();
        if (fireball != null && fireball.ownerTag != ownerTag)
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
            return;
        }

        Arrow arrow = collision.GetComponent<Arrow>();
        if (arrow != null)
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
            return;
        }
    }

    public void TakeDamage(int amount)
    {
        currentDurability -= amount;

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound, audioVolume);

        if (currentDurability <= 0)
        {
            Destroy(gameObject);
        }
    }
}
