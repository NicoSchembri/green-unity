using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class VenusFlyTrap : MonoBehaviour
{
    [Header("Settings")]
    public float closeDelay = 1.5f;
    public float reopenDelay = 2f;
    public int damage = 1;

    [Header("Sprites")]
    public Sprite openSprite;
    public Sprite closedSprite;

    [Header("Top Trigger Collider")]
    public Collider2D topTriggerCollider;

    private SpriteRenderer spriteRenderer;
    private bool isClosed = false;
    private bool isClosing = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (openSprite != null)
            spriteRenderer.sprite = openSprite;

        if (topTriggerCollider != null)
            topTriggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isClosing && !isClosed)
        {
            if (other.transform.position.y > topTriggerCollider.bounds.center.y)
            {
                StartCoroutine(CloseTrap(other.gameObject));
            }
        }
    }

    private IEnumerator CloseTrap(GameObject player)
    {
        isClosing = true;

        yield return new WaitForSeconds(closeDelay);

        isClosed = true;
        isClosing = false;

        if (closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        if (player != null && topTriggerCollider != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null && topTriggerCollider.IsTouching(playerCollider))
            {
                CharacterMovement character = player.GetComponent<CharacterMovement>();
                if (character != null)
                    character.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(reopenDelay);
        isClosed = false;
        if (openSprite != null)
            spriteRenderer.sprite = openSprite;
    }
}
