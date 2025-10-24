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

    private Collider2D col;
    private SpriteRenderer sr;
    private Transform owner;             
    private Vector2 dir;               

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.isTrigger = true;
    }

    public void Swing(Transform ownerTransform, Vector2 swingDir)
    {
        owner = ownerTransform;
        dir = swingDir.normalized;

        // Initial placement
        transform.position = owner.position + (Vector3)(dir * swingDistance);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1);
        transform.localScale = scale;
        sr.flipX = dir.x < 0;

        Destroy(gameObject, duration);
    }

    private void Update()
    {
        if (owner != null)
        {
            transform.position = owner.position + (Vector3)(dir * swingDistance);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ownerTag))
            return;

        if (ownerTag == "Player" && collision.CompareTag("Enemy"))
        {
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
