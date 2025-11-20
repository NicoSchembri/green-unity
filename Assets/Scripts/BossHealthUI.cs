using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("References")]
    public Slider healthBar;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI stageText;

    [Header("Settings")]
    public string bossName = "Final Boss";

    [Header("Stage Colors (Optional)")]
    public Color stage1Color = Color.white;
    public Color stage2Color = Color.yellow;
    public Color stage3Color = Color.red;

    [Header("Health Bar Colors")]
    public Image healthBarFill;
    public bool changeColorByStage = true;

    [Header("UI Container")]
    public GameObject uiContainer;

    private int currentStage = 1;
    private int maxHealth;
    private int currentHealth;

    private void Start()
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
        else
        {
            if (healthBar != null) healthBar.gameObject.SetActive(false);
            if (bossNameText != null) bossNameText.gameObject.SetActive(false);
            if (stageText != null) stageText.gameObject.SetActive(false);
        }
    }

    public void ShowBossUI()
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(true);
        }
        else
        {
            if (healthBar != null) healthBar.gameObject.SetActive(true);
            if (bossNameText != null) bossNameText.gameObject.SetActive(true);
            if (stageText != null) stageText.gameObject.SetActive(true);
        }
    }

    public void HideBossUI()
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(false);
        }
        else
        {
            if (healthBar != null) healthBar.gameObject.SetActive(false);
            if (bossNameText != null) bossNameText.gameObject.SetActive(false);
            if (stageText != null) stageText.gameObject.SetActive(false);
        }
    }

    public void InitializeBoss(int stage, int health)
    {
        currentStage = stage;
        maxHealth = health;
        currentHealth = health;
        UpdateUI();
    }

    public void UpdateBossHealth(int stage, int health, int maxHP)
    {
        currentStage = stage;
        currentHealth = health;
        maxHealth = maxHP;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update health bar
        if (healthBar != null)
        {
            float fillAmount = maxHealth > 0 ? (float)currentHealth / (float)maxHealth : 0f;
            healthBar.value = fillAmount;
        }

        // Update boss name
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        // Update stage text
        if (stageText != null)
        {
            string stageDisplay = currentStage switch
            {
                1 => "Stage 1",
                2 => "Stage 2",
                3 or 4 => "Stage 3",
                _ => "Stage ?"
            };
            stageText.text = stageDisplay;
        }

        if (changeColorByStage && healthBarFill != null)
        {
            healthBarFill.color = currentStage switch
            {
                1 => stage1Color,
                2 => stage2Color,
                3 or 4 => stage3Color,
                _ => Color.white
            };
        }
    }
}