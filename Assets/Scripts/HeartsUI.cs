using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Heart Settings")]
    public int maxHearts = 5;
    public int currentHearts = 5;

    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("UI References")]
    public Image[] heartImages;

    public void UpdateHearts(int current)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].sprite = (i < current) ? fullHeart : emptyHeart;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHearts = Mathf.Max(0, currentHearts - amount);
        UpdateHearts(currentHearts);
    }

    public void Heal(int amount)
    {
        currentHearts = Mathf.Min(maxHearts, currentHearts + amount);
        UpdateHearts(currentHearts);
    }
}
