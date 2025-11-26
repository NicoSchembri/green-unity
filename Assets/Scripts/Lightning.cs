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

    [Header("Audio")]
    public AudioClip strikeSound;
    [Range(0f, 1f)] public float audioVolume = 1f;

    private Collider2D col;
    private SpriteRenderer sr;
    private bool isActive = false;

    private AudioSource audioSource;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.isTrigger = true;
        col.enabled = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void Strike(Vector3 spawnPosition, string owner, bool flipX = false)
    {
        ownerTag = owner;
        transform.position = spawnPosition;
        sr.flipX = flipX;

        sr.color = warningColor;

        Invoke(nameof(StartStrike), warningDuration);
    }

    private void StartStrike()
    {
        isActive = true;
        col.enabled = true;
        sr.color = strikeColor;

        // strike sound
        if (strikeSound != null)
            audioSource.PlayOneShot(strikeSound, audioVolume);

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
            return;

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

    private void Update()
    {
        if (!isActive && sr != null)
        {
            float alpha = Mathf.PingPong(Time.time * 3f, 0.5f) + 0.3f;
            sr.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
        }
    }
}
