using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SpellBook : MonoBehaviour
{
    public enum SpellType
    {
        Fireball,
        WaterSword,
        RockShield
    }

    [Header("Spell Settings")]
    public SpellType spellType;
    public string spellName = "Spell";

    [Header("Unlock Prompt")]
    public GameObject promptPrefab;
    public float promptOffset = 1.5f;

    [Header("Audio")]
    public AudioClip unlockSound;

    [Header("Book Behavior")]
    public bool hideAfterUnlock = false;

    private Collider2D col;
    private SpriteRenderer sr;
    private bool playerInRange = false;
    private bool isUnlocked = false;
    private GameObject promptInstance;

    void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.isTrigger = true;

        // Load unlock state from PlayerPrefs
        isUnlocked = PlayerPrefs.GetInt(GetKey(), 0) == 1;

        // Hide the book if already unlocked
        if (isUnlocked && hideAfterUnlock)
        {
            sr.enabled = false;
            col.enabled = false;
        }
    }

    void Update()
    {
        // Unlock spell when player presses f and is in range
        if (playerInRange && !isUnlocked && Input.GetKeyDown(KeyCode.F))
        {
            UnlockSpell();
        }

        // Keep the prompt above the book
        if (promptInstance != null)
        {
            promptInstance.transform.position = transform.position + Vector3.up * promptOffset;
        }

        // Hide prompt if the spell was unlocked while the player is still in range
        if (playerInRange && isUnlocked && promptInstance != null)
        {
            HidePrompt();
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (!c.CompareTag("Player")) return;

        playerInRange = true;

        // Show prompt only if not unlocked
        if (!isUnlocked)
            ShowPrompt();
    }

    private void OnTriggerExit2D(Collider2D c)
    {
        if (!c.CompareTag("Player")) return;

        playerInRange = false;
        HidePrompt();
    }

    private void UnlockSpell()
    {
        CharacterMovement player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterMovement>();
        if (player == null)
        {
            Debug.LogError("SpellBook: Player not found!");
            return;
        }

        switch (spellType)
        {
            case SpellType.Fireball:
                player.UnlockSpell(CharacterMovement.SpellType.Fireball);
                break;

            case SpellType.WaterSword:
                player.UnlockSpell(CharacterMovement.SpellType.WaterSword);
                break;

            case SpellType.RockShield:
                player.UnlockSpell(CharacterMovement.SpellType.RockShield);
                break;
        }

        isUnlocked = true;

        // Save unlock state
        PlayerPrefs.SetInt(GetKey(), 1);
        PlayerPrefs.Save();

        // Play unlock sound
        if (unlockSound != null)
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);

        Debug.Log($"Unlocked spell: {spellName}");

        HidePrompt();

        // Hide the book if needed
        if (hideAfterUnlock)
        {
            sr.enabled = false;
            col.enabled = false;
        }
    }

    private void ShowPrompt()
    {
        if (promptPrefab == null || promptInstance != null) return;

        promptInstance = Instantiate(
            promptPrefab,
            transform.position + Vector3.up * promptOffset,
            Quaternion.identity
        );
    }

    private void HidePrompt()
    {
        if (promptInstance != null)
            Destroy(promptInstance);

        promptInstance = null;
    }

    private string GetKey()
    {
        return $"Spell_{spellType}";
    }
}
