using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("CheckPoint Settings")]
    public bool isActive = true;

    [Header("Audio")]
    public AudioClip checkpointSound;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        if (collision.CompareTag("Player"))
        {
            CharacterMovement player = collision.GetComponent<CharacterMovement>();
            if (player != null)
            {
                player.SetCheckpoint(transform.position);

                player.currentHearts = player.maxHearts;
                player.heartsUI?.UpdateHearts(player.currentHearts);

                Debug.Log("Checkpoint reached! Fully healed.");

                if (checkpointSound != null)
                    AudioSource.PlayClipAtPoint(checkpointSound, transform.position);
            }
        }
    }
}
