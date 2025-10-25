using UnityEngine;
using UnityEngine.UI;

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
    public SpellType spellType;

    [Header("UI Settings")]
    public string spellName = "Spell Name";
    public float promptOffset = 1.5f; 

    [Header("Visual Feedback")]
    public AudioClip unlockSound; 

    [Header("Book Settings")]
    public bool destroyAfterUnlock = false;

    private bool playerInRange = false;
    private bool isUnlocked = false;
    private Collider2D col;
    private SpriteRenderer sr;
    private GameObject promptInstance;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        col.isTrigger = true;

        string saveKey = GetSaveKey();
        isUnlocked = PlayerPrefs.GetInt(saveKey, 0) == 1;

        if (isUnlocked)
        {
            OnAlreadyUnlocked();
        }
    }

    void Update()
    {
        if (playerInRange && !isUnlocked && Input.GetKeyDown(KeyCode.Return))
        {
            UnlockSpell();
        }

        if (promptInstance != null)
        {
            promptInstance.transform.position = transform.position + Vector3.up * promptOffset;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isUnlocked)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            HidePrompt();
        }
    }

    private void UnlockSpell()
    {
        CharacterMovement player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterMovement>();
        if (player == null)
        {
            Debug.LogWarning("SpellBook: Could not find player!");
            return;
        }

        bool unlocked = false;
        switch (spellType)
        {
            case SpellType.Fireball:
                if (player.fireballPrefab != null)
                {
                    player.UnlockSpell(CharacterMovement.SpellType.Fireball);
                    unlocked = true;
                }
                else
                {
                    Debug.LogWarning("SpellBook: Fireball prefab not assigned to player!");
                }
                break;
            case SpellType.WaterSword:
                if (player.waterSwordPrefab != null)
                {
                    player.UnlockSpell(CharacterMovement.SpellType.WaterSword);
                    unlocked = true;
                }
                else
                {
                    Debug.LogWarning("SpellBook: WaterSword prefab not assigned to player!");
                }
                break;
            case SpellType.RockShield:
                if (player.rockShieldPrefab != null)
                {
                    player.UnlockSpell(CharacterMovement.SpellType.RockShield);
                    unlocked = true;
                }
                else
                {
                    Debug.LogWarning("SpellBook: RockShield prefab not assigned to player!");
                }
                break;
        }

        if (!unlocked)
            return;

        string saveKey = GetSaveKey();
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();

        isUnlocked = true;

        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }

        Debug.Log($"Unlocked spell: {spellName}!");



        HidePrompt();
        if (destroyAfterUnlock)
        {
            Destroy(gameObject, 0.5f); 
        }
        else
        {
            col.enabled = false; 
        }
    }

    private void OnAlreadyUnlocked()
    {
        if (destroyAfterUnlock)
        {
            gameObject.SetActive(false);
        }
        else
        {
            col.enabled = false;
        }
    }

    private void HidePrompt()
    {
        if (promptInstance != null)
        {
            Destroy(promptInstance);
            promptInstance = null;
        }
    }

    private string GetSaveKey()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return $"SpellBook_{sceneName}_{spellType}_{transform.position.x}_{transform.position.y}";
    }

    void OnDestroy()
    {
        HidePrompt();
    }
    public static void ResetAllUnlocks()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All spell unlocks have been reset!");
    }
}