using UnityEngine;
using System.Collections;
using TMPro;

public class BossDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Intro Dialogue")]
    [TextArea(3, 10)]
    public string[] introDialogue = new string[]
    {
        "Boss: So, you've made it this far...",
        "Frog: I'm here to restore peace to the kingdom!",
        "Boss: We'll see about that. Prepare yourself!"
    };

    [Header("Stage 2 Dialogue")]
    [TextArea(3, 10)]
    public string[] stage2Dialogue = new string[]
    {
        "Boss: Impressive... but I'm just getting started!",
        "Frog: I won't give up!",
        "Boss: Then face my true power!"
    };

    [Header("Stage 3 Dialogue")]
    [TextArea(3, 10)]
    public string[] stage3Dialogue = new string[]
    {
        "Boss: You're stronger than I thought...",
        "Frog: It's over! Surrender now!",
        "Boss: Never! This is my final stand!"
    };

    [Header("Defeat Dialogue")]
    [TextArea(3, 10)]
    public string[] defeatDialogue = new string[]
    {
        "Boss: I... I can't believe it...",
        "Frog: The kingdom is saved!",
        "Boss: You have... truly earned this victory...",
        "The kingdom thanks you, hero."
    };

    [Header("Player Reference")]
    public GameObject player;

    private CharacterMovement playerMovement;
    private Rigidbody2D playerRb;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerMovement = player.GetComponent<CharacterMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public IEnumerator PlayIntroDialogue()
    {
        yield return StartCoroutine(PlayDialogue(introDialogue));
    }

    public IEnumerator PlayStage2Dialogue()
    {
        yield return StartCoroutine(PlayDialogue(stage2Dialogue));
    }

    public IEnumerator PlayStage3Dialogue()
    {
        yield return StartCoroutine(PlayDialogue(stage3Dialogue));
    }

    public IEnumerator PlayDefeatDialogue()
    {
        yield return StartCoroutine(PlayDialogue(defeatDialogue));
    }

    private IEnumerator PlayDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
            yield break;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        foreach (string line in lines)
        {
            if (dialogueText != null)
                dialogueText.text = line;

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;
    }
}