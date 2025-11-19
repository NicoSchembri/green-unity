using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Lightning : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;
    public float warningDuration = 0.5f; 
    public float activeDuration = 1f;    
    public float strikeSpeed = 20f;      
    public string ownerTag;

    [Header("Visual Settings")]
    public Color warningColor = new Color(1f, 1f, 0f, 0.5f); 
    public Color strikeColor = Color.white;

    private Collider2D col;
    private SpriteRenderer sr;
    private bool hasStruck = false;
    private bool isActive = false;
    private bool isStriking = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float strikeTimer = 0f;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.isTrigger = true;
        col.enabled = false;
    }

    public void Strike(Vector3 spawnPosition, string owner, bool flipX = false)
    {
        ownerTag = owner;
        startPosition = spawnPosition;
        targetPosition = spawnPosition; 
        transform.position = spawnPosition;
        sr.flipX = flipX;

        sr.color = warningColor;

        Invoke(nameof(StartStrike), warningDuration);
    }

    private void StartStrike()
    {
        isStriking = true;
        isActive = true;
        col.enabled = true;
        sr.color = strikeColor;

        Invoke(nameof(FadeAndDestroy), activeDuration);
    }

    private void FadeAndDestroy()
    {
        col.enabled = false; 

        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float fadeTime = 0.3f;
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
        {
            Debug.Log("Lightning not active yet");
            return;
        }

        if (collision.CompareTag(ownerTag))
        {
            Debug.Log($"Ignoring collision with owner: {ownerTag}");
            return;
        }

        Debug.Log($"Lightning hit: {collision.gameObject.name}, Tag: {collision.tag}, OwnerTag: {ownerTag}");

        if (ownerTag == "Player" && collision.CompareTag("Enemy"))
        {
            Debug.Log("Lightning damaged enemy");
            Destroy(collision.gameObject);
        }
        else if (ownerTag == "Enemy" && collision.CompareTag("Player"))
        {
            Debug.Log("Lightning hit player, attempting damage");
            CharacterMovement character = collision.GetComponent<CharacterMovement>();
            if (character != null)
            {
                Debug.Log($"Dealing {damage} damage to player");
                character.TakeDamage(damage);
            }
            else
            {
                Debug.LogError("CharacterMovement component not found on player!");
            }
        }
    }

    private void Update()
    {
        if (!isActive && sr != null)
        {
            float alpha = Mathf.PingPong(Time.time * 3f, 0.5f) + 0.3f;
            sr.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
        }
    }
}