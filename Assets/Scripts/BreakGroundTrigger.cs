using UnityEngine;

public class BreakGroundTrigger : MonoBehaviour
{
    [Header("Blocks to Destroy")]
    public GameObject[] blocks;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != null)
                    Destroy(blocks[i]);
            }

            Destroy(gameObject);
        }
    }
}
