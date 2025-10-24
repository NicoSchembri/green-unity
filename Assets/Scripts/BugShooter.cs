using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BugShooter : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Player Tracking")]
    public float detectionRange = 10f;
    public float shootInterval = 2f;
    public Transform firePoint;
    public GameObject fireballPrefab;

    [Header("Fireball Settings")]
    public float fireballScale = 0.5f;
    public int fireballDamage = 1;

    [Header("Visuals")]
    public bool faceRightInitially = true;

    private Transform player;
    private bool canShoot = true;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (firePoint == null)
            Debug.LogWarning($"{name}: FirePoint not assigned!");
        if (fireballPrefab == null)
            Debug.LogWarning($"{name}: Fireball prefab not assigned!");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= detectionRange)
        {
            FacePlayer();
            if (canShoot)
                StartCoroutine(ShootAtPlayer());
        }
    }

    private void FacePlayer()
    {
        bool shouldFaceRight = player.position.x > transform.position.x;

        if (faceRightInitially)
            spriteRenderer.flipX = !shouldFaceRight;
        else
            spriteRenderer.flipX = shouldFaceRight;
    }

    private IEnumerator ShootAtPlayer()
    {
        canShoot = false;

        if (fireballPrefab != null && firePoint != null && player != null)
        {
            GameObject fireballObj = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
            fireballObj.transform.localScale = Vector3.one * fireballScale;

            Fireball fireball = fireballObj.GetComponent<Fireball>();
            if (fireball != null)
            {
                fireball.damage = fireballDamage;
                fireball.ownerTag = "Enemy";
                Vector2 dir = (player.position - firePoint.position).normalized;
                fireball.Launch(dir);
            }
        }

        yield return new WaitForSeconds(shootInterval);
        canShoot = true;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name} took {damage} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{name} was destroyed!");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Fireball fb = other.GetComponent<Fireball>();
        if (fb != null && fb.ownerTag == "Player")  
        {
            TakeDamage(fb.damage);
            Destroy(other.gameObject);
            return; 
        }
        WaterSword sword = other.GetComponent<WaterSword>();
        if (sword != null && sword.ownerTag == "Player")
        {
            TakeDamage(sword.damage);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
