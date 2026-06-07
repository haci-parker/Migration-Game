using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and updates the top HUD bars without changing GameManager.
/// Reads values from GameManager.Instance and renders Health, Food, Journey, and Durability.
/// </summary>
public class TopHudController : MonoBehaviour
{
    private enum TopHudStat
    {
        Health,
        FoodSupplies,
        JourneyProgress,
        Durability
    }

    private class StatView
    {
        public TopHudStat Stat;
        public TMP_Text ValueText;
        public Image FillImage;
    }

    [Header("Icons")]
    public Sprite healthIcon;
    public Sprite foodIcon;
    public Sprite journeyIcon;
    public Sprite durabilityIcon;

    [Header("Layout")]
    public float topHeight = 150f;
    public float gradientHeight = 220f;
    public float statsAreaHeight = 86f;
    public float horizontalPadding = 42f;
    public float statWidth = 260f;
    public float statSpacing = 28f;
    public float iconSize = 34f;
    public float barHeight = 8f;
    public float barTopOffset = -47f;

    [Header("Style")]
    public Color panelTopColor = new Color(0f, 0f, 0f, 0.99f);
    public Color panelMiddleColor = new Color(0f, 0f, 0f, 0.78f);
    public Color panelBottomColor = new Color(0f, 0f, 0f, 0f);
    public Color lineColor = new Color(1f, 1f, 1f, 0.16f);
    public Color iconColor = new Color(0.72f, 0.72f, 0.72f, 1f);
    public Color labelColor = new Color(0.78f, 0.74f, 0.68f, 1f);
    public Color valueColor = new Color(0.84f, 0.82f, 0.78f, 1f);
    public Color barBackColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
    public Color barFillColor = new Color(0.72f, 0.12f, 0.12f, 1f);
    public Color barOutlineColor = new Color(0.72f, 0.72f, 0.72f, 0.16f);

    private readonly StatView[] _statViews = new StatView[4];
    private RectTransform _root;
    private Sprite _panelGradientSprite;

    private void Awake()
    {
        BuildHud();
    }

    private void Update()
    {
        UpdateHud();
    }

    private void BuildHud()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("TopHudController needs a Canvas in the scene.");
            return;
        }

        Transform oldRoot = canvas.transform.Find("TopHudRoot");
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        GameObject rootObject = CreateUIObject("TopHudRoot", canvas.transform);
        _root = rootObject.GetComponent<RectTransform>();
        _root.anchorMin = new Vector2(0f, 1f);
        _root.anchorMax = new Vector2(1f, 1f);
        _root.pivot = new Vector2(0.5f, 1f);
        _root.anchoredPosition = Vector2.zero;
        _root.sizeDelta = new Vector2(0f, Mathf.Max(topHeight, gradientHeight));

        Image panelImage = rootObject.AddComponent<Image>();
        panelImage.sprite = CreatePanelGradientSprite();
        panelImage.type = Image.Type.Simple;
        panelImage.color = Color.white;
        panelImage.raycastTarget = false;

        RectTransform statsParent = CreateStatsParent(_root);
        CreateBottomLine(statsParent);

        _statViews[0] = CreateStat(statsParent, TopHudStat.Health, "Sağlık", healthIcon, 0f);
        _statViews[1] = CreateStat(statsParent, TopHudStat.FoodSupplies, "Erzak", foodIcon, 1f);
        _statViews[2] = CreateStat(statsParent, TopHudStat.JourneyProgress, "Yol", journeyIcon, 2f);
        _statViews[3] = CreateStat(statsParent, TopHudStat.Durability, "Direnç", durabilityIcon, 3f);
    }

    private RectTransform CreateStatsParent(RectTransform parent)
    {
        GameObject statsObject = CreateUIObject("StatsArea", parent);
        RectTransform statsRect = statsObject.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0f, 1f);
        statsRect.anchorMax = new Vector2(1f, 1f);
        statsRect.pivot = new Vector2(0.5f, 1f);
        statsRect.anchoredPosition = Vector2.zero;
        statsRect.sizeDelta = new Vector2(0f, statsAreaHeight);
        return statsRect;
    }

    private StatView CreateStat(RectTransform parent, TopHudStat stat, string label, Sprite icon, float index)
    {
        GameObject statObject = CreateUIObject(label + "Stat", parent);
        RectTransform statRect = statObject.GetComponent<RectTransform>();
        float slotMin = index / 4f;
        float slotMax = (index + 1f) / 4f;
        statRect.anchorMin = new Vector2(slotMin, 0f);
        statRect.anchorMax = new Vector2(slotMax, 1f);
        statRect.pivot = new Vector2(0.5f, 0.5f);
        statRect.anchoredPosition = Vector2.zero;
        float leftInset = index == 0f ? statSpacing : statSpacing * 0.5f;
        float rightInset = index == 3f ? statSpacing : statSpacing * 0.5f;
        statRect.offsetMin = new Vector2(leftInset, 0f);
        statRect.offsetMax = new Vector2(-rightInset, 0f);

        Image iconImage = CreateImage("Icon", statRect, iconColor);
        RectTransform iconRect = iconImage.rectTransform;
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(horizontalPadding * 0.55f, -1f);
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
        iconImage.enabled = icon != null;

        TMP_Text labelText = CreateText("Label", statRect, label, 18f, labelColor, TextAlignmentOptions.Left);
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        float contentLeft = horizontalPadding;
        float contentRight = horizontalPadding * 0.7f;

        labelRect.anchoredPosition = new Vector2(contentLeft, -20f);
        labelRect.sizeDelta = new Vector2(110f, 26f);

        TMP_Text valueText = CreateText("Value", statRect, "", 17f, valueColor, TextAlignmentOptions.Right);
        RectTransform valueRect = valueText.rectTransform;
        valueRect.anchorMin = new Vector2(1f, 1f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 1f);
        valueRect.anchoredPosition = new Vector2(-contentRight, -20f);
        valueRect.sizeDelta = new Vector2(94f, 26f);

        Image barBack = CreateImage("BarBack", statRect, barBackColor);
        RectTransform barBackRect = barBack.rectTransform;
        barBackRect.anchorMin = new Vector2(0f, 1f);
        barBackRect.anchorMax = new Vector2(1f, 1f);
        barBackRect.pivot = new Vector2(0.5f, 1f);
        barBackRect.anchoredPosition = new Vector2((contentLeft - contentRight) * 0.5f, barTopOffset);
        barBackRect.sizeDelta = new Vector2(-(contentLeft + contentRight), barHeight);

        Image fillImage = CreateImage("BarFill", barBackRect, barFillColor);
        RectTransform fillRect = fillImage.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;

        CreateBarOutline(barBackRect);

        if (index < 3f)
            CreateSmallLine(statRect);

        return new StatView
        {
            Stat = stat,
            ValueText = valueText,
            FillImage = fillImage
        };
    }

    private void UpdateHud()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            return;

        for (int i = 0; i < _statViews.Length; i++)
        {
            StatView view = _statViews[i];
            if (view == null)
                continue;

            float value = GetValue(gameManager, view.Stat);
            float maxValue = 100f;
            float normalizedValue = Mathf.Clamp01(value / maxValue);

            if (view.ValueText != null)
                view.ValueText.text = Mathf.RoundToInt(value) + " / " + Mathf.RoundToInt(maxValue);

            if (view.FillImage != null)
                view.FillImage.fillAmount = normalizedValue;
        }
    }

    private float GetValue(GameManager gameManager, TopHudStat stat)
    {
        switch (stat)
        {
            case TopHudStat.Health:
                return gameManager.Health;
            case TopHudStat.FoodSupplies:
                return gameManager.FoodSupplies;
            case TopHudStat.JourneyProgress:
                return gameManager.JourneyProgress;
            case TopHudStat.Durability:
                return gameManager.Durability;
            default:
                return 0f;
        }
    }

    private void CreateBottomLine(RectTransform parent)
    {
        Image line = CreateImage("BottomLine", parent, lineColor);
        RectTransform lineRect = line.rectTransform;
        lineRect.anchorMin = new Vector2(0f, 0f);
        lineRect.anchorMax = new Vector2(1f, 0f);
        lineRect.pivot = new Vector2(0.5f, 0f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(0f, 1f);
    }

    private Sprite CreatePanelGradientSprite()
    {
        if (_panelGradientSprite != null)
            return _panelGradientSprite;

        const int width = 2;
        const int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            float t = y / (height - 1f);
            Color effectiveTopColor = new Color(panelTopColor.r, panelTopColor.g, panelTopColor.b, Mathf.Max(panelTopColor.a, 0.98f));
            Color effectiveMiddleColor = new Color(panelMiddleColor.r, panelMiddleColor.g, panelMiddleColor.b, Mathf.Max(panelMiddleColor.a, 0.76f));
            Color effectiveBottomColor = new Color(panelBottomColor.r, panelBottomColor.g, panelBottomColor.b, 0f);

            Color softColor = new Color(effectiveMiddleColor.r, effectiveMiddleColor.g, effectiveMiddleColor.b, 0.22f);
            Color lowMidColor = new Color(effectiveMiddleColor.r, effectiveMiddleColor.g, effectiveMiddleColor.b, 0.48f);
            Color highMidColor = new Color(effectiveMiddleColor.r, effectiveMiddleColor.g, effectiveMiddleColor.b, 0.82f);

            Color color;
            if (t < 0.2f)
            {
                color = Color.Lerp(effectiveBottomColor, softColor, Mathf.SmoothStep(0f, 1f, t / 0.2f));
            }
            else if (t < 0.52f)
            {
                color = Color.Lerp(softColor, lowMidColor, Mathf.SmoothStep(0f, 1f, (t - 0.2f) / 0.32f));
            }
            else if (t < 0.82f)
            {
                color = Color.Lerp(lowMidColor, highMidColor, Mathf.SmoothStep(0f, 1f, (t - 0.52f) / 0.3f));
            }
            else
            {
                color = Color.Lerp(highMidColor, effectiveTopColor, Mathf.SmoothStep(0f, 1f, (t - 0.82f) / 0.18f));
            }

            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, color);
        }

        texture.Apply();
        _panelGradientSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f));
        return _panelGradientSprite;
    }

    private void CreateSmallLine(RectTransform parent)
    {
        Image line = CreateImage("Divider", parent, lineColor);
        RectTransform lineRect = line.rectTransform;
        lineRect.anchorMin = new Vector2(1f, 0.15f);
        lineRect.anchorMax = new Vector2(1f, 0.85f);
        lineRect.pivot = new Vector2(1f, 0.5f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(1f, 0f);
    }

    private void CreateBarOutline(RectTransform parent)
    {
        CreateBarLine("OutlineTop", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f));
        CreateBarLine("OutlineBottom", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 1f));
        CreateBarLine("OutlineLeft", parent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(1f, 0f));
        CreateBarLine("OutlineRight", parent, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(1f, 0f));
    }

    private void CreateBarLine(string objectName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        Image line = CreateImage(objectName, parent, barOutlineColor);
        RectTransform lineRect = line.rectTransform;
        lineRect.anchorMin = anchorMin;
        lineRect.anchorMax = anchorMax;
        lineRect.pivot = pivot;
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = sizeDelta;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = CreateUIObject(objectName, parent);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(string objectName, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = color;
        tmpText.alignment = alignment;
        tmpText.raycastTarget = false;
        tmpText.textWrappingMode = TextWrappingModes.NoWrap;
        return tmpText;
    }
}
