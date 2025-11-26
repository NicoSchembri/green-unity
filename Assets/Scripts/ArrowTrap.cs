using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Detection Settings")]
    public Vector2 detectionBoxSize = new Vector2(6f, 3f);
    public Vector2 detectionBoxOffset = Vector2.zero;
    public string playerTag = "Player";
    public LayerMask detectionMask = ~0;

    [Header("Shooting Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public Vector2 shootDirection = Vector2.right;
    public float shootDelay = 0.1f;
    public int arrowCount = 1;
    public float arrowInterval = 0.15f;
    public float shootCooldown = 2f;

    [Header("Audio")]
    public AudioClip shootSound;   // When arrow fires
    [Range(0f, 1f)] public float audioVolume = 1f;

    private AudioSource audioSource;
    private bool isShooting = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = shootDirection.normalized;
            firePoint = fp.transform;
        }
    }

    void Update()
    {
        if (isShooting) return;

        Vector2 center = (Vector2)transform.position + detectionBoxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, detectionBoxSize, 0f, detectionMask);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (!hit.CompareTag(playerTag)) continue;

            StartCoroutine(ShootBurst());
            break;
        }
    }

    private System.Collections.IEnumerator ShootBurst()
    {
        isShooting = true;
        yield return new WaitForSeconds(shootDelay);

        for (int i = 0; i < arrowCount; i++)
        {
            ShootArrow();
            yield return new WaitForSeconds(arrowInterval);
        }

        yield return new WaitForSeconds(shootCooldown);
        isShooting = false;
    }

    void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("Arrow prefab not assigned to ArrowTrap!");
            return;
        }

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
            arrowScript.SetDirection(shootDirection.normalized);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound, audioVolume);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 center = (Vector2)transform.position + detectionBoxOffset;
        Gizmos.DrawWireCube(center, detectionBoxSize);

        Gizmos.color = Color.red;
        Vector3 start = firePoint != null ? firePoint.position : transform.position;
        Gizmos.DrawRay(start, shootDirection.normalized * 2f);
    }
}
