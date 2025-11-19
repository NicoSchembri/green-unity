using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Heart Settings")]
    public int maxHearts = 3;
    public int currentHearts = 3;

    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("UI References")]
    public Image[] heartImages;

    private void Start()
    {
        UpdateHearts(currentHearts);
    }

    public void UpdateHearts(int current)
    {
        currentHearts = current;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < maxHearts)
            {
                heartImages[i].gameObject.SetActive(true);
                heartImages[i].sprite = (i < current) ? fullHeart : emptyHeart;
            }
            else
            {
                heartImages[i].gameObject.SetActive(false);
            }
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