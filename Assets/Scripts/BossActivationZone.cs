using UnityEngine;
using System.Collections;

public class BossActivationZone : MonoBehaviour
{
    [Header("Boss References")]
    public BossController boss;
    public BossDialogueManager dialogueManager;

    [Header("Walls")]
    public GameObject[] invisibleWalls;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            // Turn on invisible walls
            foreach (GameObject wall in invisibleWalls)
            {
                if (wall != null)
                    wall.SetActive(true);
            }

            StartCoroutine(ActivateBossSequence());
        }
    }

    private IEnumerator ActivateBossSequence()
    {
        if (boss != null)
            boss.ActivateBoss();

        if (dialogueManager != null)
            yield return StartCoroutine(dialogueManager.PlayIntroDialogue());

        if (boss != null)
        {
            boss.healthUI?.ShowBossUI();
            boss.ResumeFighting();
        }
    }

    public void DisableWalls()
    {
        foreach (GameObject wall in invisibleWalls)
        {
            if (wall != null)
                wall.SetActive(false);
        }
    }
}
