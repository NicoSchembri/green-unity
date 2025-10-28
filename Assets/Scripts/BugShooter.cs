using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class BugShooter : MonoBehaviour
{
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

    [Header("Audio")]
    public AudioClip bugIdleSound;   
    public AudioClip shootSound;      
    [Range(0f, 1f)] public float bugVolume = 0.6f;
    [Range(0f, 1f)] public float shootVolume = 0.8f;

    private Transform player;
    private bool canShoot = true;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = bugVolume;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (firePoint == null)
            Debug.LogWarning($"{name}: FirePoint not assigned!");
        if (fireballPrefab == null)
            Debug.LogWarning($"{name}: Fireball prefab not assigned!");

        // --- Play idle sound loop ---
        if (bugIdleSound != null)
        {
            audioSource.clip = bugIdleSound;
            audioSource.loop = true;      // force looping
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"{name}: No idle sound assigned!");
        }
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

        if (shootSound != null)
            AudioSource.PlayClipAtPoint(shootSound, transform.position, shootVolume);

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
