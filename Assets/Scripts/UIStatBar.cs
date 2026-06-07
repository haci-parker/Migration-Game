using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows one top HUD stat as a label, numeric value, and horizontal bar.
/// The value is read from GameManager; this script only handles presentation.
/// </summary>
public class UIStatBar : MonoBehaviour
{
    public enum TopStatType
    {
        Health,
        FoodSupplies,
        JourneyProgress,
        Durability
    }

    [Header("Stat")]
    public TopStatType statType;
    public float maxValue = 100f;

    [Header("UI References")]
    public TMP_Text labelText;
    public TMP_Text valueText;
    public Image fillImage;

    [Header("Labels")]
    public string healthLabel = "Sağlık";
    public string foodSuppliesLabel = "Erzak";
    public string journeyProgressLabel = "Yol";
    public string durabilityLabel = "Direnç";

    private void Start()
    {
        UpdateBar();
    }

    private void Update()
    {
        UpdateBar();
    }

    private void UpdateBar()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            return;

        float currentValue = GetCurrentValue(gameManager);
        float normalizedValue = maxValue > 0f ? Mathf.Clamp01(currentValue / maxValue) : 0f;

        if (labelText != null)
            labelText.text = GetLabel();

        if (valueText != null)
            valueText.text = Mathf.RoundToInt(currentValue) + " / " + Mathf.RoundToInt(maxValue);

        if (fillImage != null)
            fillImage.fillAmount = normalizedValue;
    }

    private float GetCurrentValue(GameManager gameManager)
    {
        switch (statType)
        {
            case TopStatType.Health:
                return gameManager.Health;
            case TopStatType.FoodSupplies:
                return gameManager.FoodSupplies;
            case TopStatType.JourneyProgress:
                return gameManager.JourneyProgress;
            case TopStatType.Durability:
                return gameManager.Durability;
            default:
                return 0f;
        }
    }

    private string GetLabel()
    {
        switch (statType)
        {
            case TopStatType.Health:
                return healthLabel;
            case TopStatType.FoodSupplies:
                return foodSuppliesLabel;
            case TopStatType.JourneyProgress:
                return journeyProgressLabel;
            case TopStatType.Durability:
                return durabilityLabel;
            default:
                return "";
        }
    }
}
