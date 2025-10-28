using UnityEngine;
using System.Collections;
using TMPro;

public class NarratorTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea(3, 10)]
    public string[] dialogueLines = new string[]
    {
        "Raccoon: You're finally here!",
        "I've been waiting for you for a long time.",
        "Ever since the king died, the whole kingdom has fallen into chaos.",
        "We need a hero to step forward and defeat all the monsters.",
        "Frog: Oh — looks like I've been chosen.",
        "Raccoon: Alright, no need for more words.",
        "See that spellbook on the pedestal? That's one of my old tricks.",
        "That fireball is not a toy — it hits hard.",
        "You can use it as your main attack.",
        "If you don't know how to cast it yet, try pressing the E key.",
        "Now go on your adventure — for our kingdom.",
        "By the way, you'll find other spellbooks later; remember to use them, they'll be very helpful.",
        "Good luck.",
        "Ah, this old man should go rest."
    };

    [Header("Display Settings")]
    public bool pausePlayerMovement = true;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private bool hasTriggered = false;
    private bool isPlayingDialogue = false;
    private int currentLineIndex = 0;
    private bool waitingForInput = false;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayDialogue(other.gameObject));
        }
    }

    private IEnumerator PlayDialogue(GameObject player)
    {
        isPlayingDialogue = true;
        currentLineIndex = 0;

        CharacterMovement playerMovement = null;
        Rigidbody2D playerRb = null;

        if (pausePlayerMovement)
        {
            playerMovement = player.GetComponent<CharacterMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();

            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueText != null && dialogueLines.Length > 0)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }

        waitingForInput = true;

        while (currentLineIndex < dialogueLines.Length)
        {
            yield return null;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (pausePlayerMovement && playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        isPlayingDialogue = false;
        waitingForInput = false;
    }

    private void Update()
    {
        if (waitingForInput && Input.GetKeyDown(KeyCode.Space))
        {
            currentLineIndex++;

            if (currentLineIndex < dialogueLines.Length)
            {
                if (dialogueText != null)
                {
                    dialogueText.text = dialogueLines[currentLineIndex];
                }
            }
        }
    }
}