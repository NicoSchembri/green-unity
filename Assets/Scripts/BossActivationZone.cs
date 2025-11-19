using UnityEngine;

public class BossActivationZone : MonoBehaviour
{
    public BossController boss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            boss.ActivateBoss();
        }
    }
}
