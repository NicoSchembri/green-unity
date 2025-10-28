using UnityEngine;

public class Bush : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Fireball>() != null)
        {
            DestroyBush();
        }
    }

    private void DestroyBush()
    {
        Destroy(gameObject);
    }
}
