using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Builds and updates the top HUD bars without changing GameManager.
/// Reads values from GameManager.Instance and renders Health, Food, Journey, and Durability.
/// </summary>
public class TopHudController : MonoBehaviour
{
    private class MenuButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Material RuntimeMaterial;
        public bool AlwaysDesaturated;

        private void OnEnable()
        {
            SetSaturation(0f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!AlwaysDesaturated)
                SetSaturation(1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetSaturation(0f);
        }

        public void SetSaturation(float saturation)
        {
            if (RuntimeMaterial != null)
                RuntimeMaterial.SetFloat("_Saturation", AlwaysDesaturated ? 0f : saturation);
        }
    }

    private enum MenuMode
    {
        Main,
        Pause
    }

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
        public RectTransform FillRect;
    }

    private class BottomMetricView
    {
        public TMP_Text ValueText;
    }

    [Header("Icons")]
    public Sprite healthIcon;
    public Sprite foodIcon;
    public Sprite journeyIcon;
    public Sprite durabilityIcon;

    [Header("Bottom HUD Sprites")]
    public Sprite bottomPanelSprite;
    public Sprite bottomMetricFrameSprite;
    public Sprite bottomPopulationIcon;
    public Sprite bottomGoldIcon;
    public Sprite bottomClimateIcon;
    public Sprite bottomSunIcon;
    public Sprite bottomContinueSprite;

    [Header("Layout")]
    public float topHeight = 150f;
    public float gradientHeight = 220f;
    public float statsAreaHeight = 86f;
    [Range(0.3f, 1f)]
    public float statsWidthRatio = 0.6f;
    public float horizontalPadding = 42f;
    public float statWidth = 260f;
    public float statSpacing = 28f;
    public float iconSize = 46f;
    public float iconLeftPadding = 18f;
    public float contentAfterIcon = 48f;
    public float barHeight = 13f;
    public float barTopOffset = -58f;

    [Header("Bottom HUD Layout")]
    public float bottomHeight = 154f;
    public float bottomPanelHeight = 124f;
    public float bottomHorizontalPadding = 32f;
    public float bottomContinueWidth = 190f;
    public float bottomMetricIconSize = 82f;
    public float bottomMetricLabelSize = 18f;
    public float bottomMetricValueSize = 34f;

    [Header("Menu")]
    public Sprite menuIcon;
    public float menuRightPadding = 34f;
    public Vector2 menuButtonSize = new Vector2(46f, 38f);
    public Vector2 menuIconSize = new Vector2(24f, 18f);
    public Color menuFrameColor = new Color(1f, 1f, 1f, 0.16f);
    public Color menuStripeColor = new Color(0.74f, 0.74f, 0.74f, 0.9f);
    public VideoClip mainMenuVideo;
    [Range(0f, 1f)]
    public float mainMenuVideoVolume = 0.5f;
    public AudioClip mainMenuMusic;
    [Range(0f, 1f)]
    public float mainMenuMusicVolume = 0.5f;
    public Sprite newGameButtonSprite;
    public Sprite continueButtonSprite;
    public Sprite settingsButtonSprite;
    public Sprite quitButtonSprite;
    public Vector2 mainMenuButtonImageSize = new Vector2(640f, 86f);
    public Vector2 mainMenuButtonStart = new Vector2(150f, -520f);
    public float mainMenuButtonSpacing = 104f;
    public Shader saturationShader;

    [Header("Style")]
    public Color panelTopColor = new Color(0f, 0f, 0f, 0.99f);
    public Color panelMiddleColor = new Color(0f, 0f, 0f, 0.78f);
    public Color panelBottomColor = new Color(0f, 0f, 0f, 0f);
    public Color lineColor = new Color(1f, 1f, 1f, 0.16f);
    public Color iconColor = new Color(0.72f, 0.72f, 0.72f, 1f);
    public Color labelColor = new Color(0.92f, 0.9f, 0.86f, 1f);
    public Color valueColor = new Color(0.96f, 0.95f, 0.9f, 1f);
    public Color barBackColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
    public Color barFillColor = new Color(0.72f, 0.12f, 0.12f, 1f);
    public Color barOutlineColor = new Color(0.72f, 0.72f, 0.72f, 0.16f);
    public Color menuOverlayColor = new Color(0f, 0f, 0f, 0.04f);
    public Color menuAccentColor = new Color(0.72f, 0.1f, 0.08f, 1f);
    public Color menuButtonColor = new Color(0.025f, 0.023f, 0.021f, 0.74f);
    public Color menuButtonHoverColor = new Color(0.08f, 0.04f, 0.035f, 0.88f);
    public Color menuDisabledColor = new Color(0.55f, 0.55f, 0.55f, 0.48f);
    public Color bottomPanelTopColor = new Color(0f, 0f, 0f, 0f);
    public Color bottomPanelMiddleColor = new Color(0f, 0f, 0f, 0.72f);
    public Color bottomPanelBottomColor = new Color(0f, 0f, 0f, 0.96f);

    [Header("Typography")]
    public TMP_FontAsset fontAsset;
    public float labelFontSize = 26f;
    public float valueFontSize = 25f;
    public FontStyles labelFontStyle = FontStyles.Normal;
    public FontStyles valueFontStyle = FontStyles.Normal;
    public float characterSpacing = 0f;

    private readonly StatView[] _statViews = new StatView[4];
    private static bool s_startGameAfterSceneReload;

    private readonly BottomMetricView[] _bottomMetricViews = new BottomMetricView[3];
    private readonly float[] _gameplayTimeScales = { 1f, 2f, 5f };
    private RectTransform _root;
    private RectTransform _bottomRoot;
    private TMP_Text _speedMetricValueText;
    private TMP_Text _gameplaySpeedButtonText;
    private RectTransform _menuRoot;
    private RectTransform _menuFrameRoot;
    private RectTransform _victoryRoot;
    private RectTransform _defeatRoot;
    private Button _continueButton;
    private Button _startButton;
    private VideoPlayer _mainMenuVideoPlayer;
    private AudioSource _mainMenuAudioSource;
    private AudioSource _mainMenuMusicSource;
    private RenderTexture _mainMenuVideoTexture;
    private bool _hasActiveRun;
    private bool _gameStarted;
    private bool _gameEnded;
    private int _gameplayTimeScaleIndex;
    private Sprite _panelGradientSprite;
    private Sprite _bottomGradientSprite;

    private void Awake()
    {
        BuildHud();
        BuildBottomHud();
        BuildMenu();
        BuildVictoryScreen();
        BuildDefeatScreen();
        ShowMenu(MenuMode.Main);

        if (s_startGameAfterSceneReload)
        {
            s_startGameAfterSceneReload = false;
            BeginNewGameSession();
        }
    }

    private void Update()
    {
        if (_gameStarted && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ShowMenu(MenuMode.Pause);

        UpdateHud();
        CheckGameEndConditions();
    }

    private void OnDestroy()
    {
        if (_mainMenuVideoTexture != null)
        {
            _mainMenuVideoTexture.Release();
            Destroy(_mainMenuVideoTexture);
        }
    }

    private void OnValidate()
    {
        ApplyMainMenuVideoVolume();
        ApplyMainMenuMusicVolume();
    }

    private void ApplyMainMenuVideoVolume()
    {
        if (_mainMenuAudioSource != null)
            _mainMenuAudioSource.volume = mainMenuVideoVolume;

        if (_mainMenuVideoPlayer != null)
            _mainMenuVideoPlayer.SetDirectAudioVolume(0, mainMenuVideoVolume);
    }

    private void ApplyMainMenuMusicVolume()
    {
        if (_mainMenuMusicSource != null)
            _mainMenuMusicSource.volume = mainMenuMusicVolume;
    }

    private void BuildMainMenuMusic(GameObject rootObject)
    {
        if (mainMenuMusic == null)
            return;

        _mainMenuMusicSource = rootObject.AddComponent<AudioSource>();
        _mainMenuMusicSource.clip = mainMenuMusic;
        _mainMenuMusicSource.playOnAwake = false;
        _mainMenuMusicSource.loop = true;
        _mainMenuMusicSource.volume = mainMenuMusicVolume;
        _mainMenuMusicSource.Play();
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
        CreateMenuButton(_root);
        CreateSpeedMetric(_root);

        _statViews[0] = CreateStat(statsParent, TopHudStat.Health, "Sağlık", healthIcon, 0f);
        _statViews[1] = CreateStat(statsParent, TopHudStat.FoodSupplies, "Erzak", foodIcon, 1f);
        _statViews[2] = CreateStat(statsParent, TopHudStat.JourneyProgress, "Yol", journeyIcon, 2f);
        _statViews[3] = CreateStat(statsParent, TopHudStat.Durability, "Direnç", durabilityIcon, 3f);
    }

    private void BuildBottomHud()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        Transform oldRoot = canvas.transform.Find("BottomHudRoot");
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        GameObject rootObject = CreateUIObject("BottomHudRoot", canvas.transform);
        _bottomRoot = rootObject.GetComponent<RectTransform>();
        _bottomRoot.anchorMin = new Vector2(0f, 0f);
        _bottomRoot.anchorMax = new Vector2(1f, 0f);
        _bottomRoot.pivot = new Vector2(0.5f, 0f);
        _bottomRoot.anchoredPosition = Vector2.zero;
        _bottomRoot.sizeDelta = new Vector2(0f, bottomHeight);

        Image gradient = rootObject.AddComponent<Image>();
        gradient.sprite = CreateBottomGradientSprite();
        gradient.color = Color.white;
        gradient.raycastTarget = false;

        RectTransform panel = CreateBottomPanel(_bottomRoot);
        RectTransform metrics = CreateUIObject("Metrics", panel).GetComponent<RectTransform>();
        metrics.anchorMin = new Vector2(0f, 0f);
        metrics.anchorMax = new Vector2(1f, 1f);
        metrics.offsetMin = new Vector2(18f, 0f);
        metrics.offsetMax = new Vector2(-(bottomContinueWidth + 18f), 0f);

        _bottomMetricViews[0] = CreateBottomMetric(metrics, 0f, "Nüfus", bottomPopulationIcon);
        _bottomMetricViews[1] = CreateBottomMetric(metrics, 1f, "Altın", bottomGoldIcon);
        _bottomMetricViews[2] = CreateBottomMetric(metrics, 2f, "İklim", bottomClimateIcon);

        CreateBottomSun(metrics);
        CreateBottomContinue(panel);
    }

    private RectTransform CreateBottomPanel(RectTransform parent)
    {
        GameObject panelObject = CreateUIObject("BottomPanel", parent);
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, 12f);
        panel.sizeDelta = new Vector2(-bottomHorizontalPadding * 2f, bottomPanelHeight);

        Image image = panelObject.AddComponent<Image>();
        image.sprite = bottomPanelSprite;
        image.color = bottomPanelSprite != null ? Color.white : new Color(0.015f, 0.014f, 0.013f, 0.84f);
        image.raycastTarget = false;
        image.type = bottomPanelSprite != null ? Image.Type.Sliced : Image.Type.Simple;

        CreateMenuBoxFrame(panel, menuFrameColor);
        return panel;
    }

    private BottomMetricView CreateBottomMetric(RectTransform parent, float index, string label, Sprite icon)
    {
        GameObject metricObject = CreateUIObject(label + "BottomMetric", parent);
        RectTransform metric = metricObject.GetComponent<RectTransform>();
        float slotMin = index / 3.65f;
        float slotMax = (index + 1f) / 3.65f;
        metric.anchorMin = new Vector2(slotMin, 0f);
        metric.anchorMax = new Vector2(slotMax, 1f);
        metric.offsetMin = new Vector2(10f, 12f);
        metric.offsetMax = new Vector2(-10f, -10f);

        if (bottomMetricFrameSprite != null)
        {
            Image frame = metricObject.AddComponent<Image>();
            frame.sprite = bottomMetricFrameSprite;
            frame.color = Color.white;
            frame.preserveAspect = true;
            frame.raycastTarget = false;
        }

        Image iconImage = CreateImage("Icon", metric, iconColor);
        RectTransform iconRect = iconImage.rectTransform;
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(bottomMetricIconSize * 0.5f, 0f);
        iconRect.sizeDelta = new Vector2(bottomMetricIconSize, bottomMetricIconSize);
        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
        iconImage.enabled = icon != null;

        if (icon == null)
            CreateSimpleMetricGlyph(iconRect, label);

        TMP_Text labelText = CreateText("Label", metric, label, bottomMetricLabelSize, labelColor, TextAlignmentOptions.Left, labelFontStyle);
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = new Vector2(bottomMetricIconSize + 14f, -10f);
        labelRect.sizeDelta = new Vector2(-(bottomMetricIconSize + 20f), 24f);

        TMP_Text valueText = CreateText("Value", metric, "", bottomMetricValueSize, menuAccentColor, TextAlignmentOptions.Left, FontStyles.Normal);
        RectTransform valueRect = valueText.rectTransform;
        valueRect.anchorMin = new Vector2(0f, 0.5f);
        valueRect.anchorMax = new Vector2(1f, 0.5f);
        valueRect.pivot = new Vector2(0f, 0.5f);
        valueRect.anchoredPosition = new Vector2(bottomMetricIconSize + 14f, -4f);
        valueRect.sizeDelta = new Vector2(-(bottomMetricIconSize + 20f), 48f);

        if (index < 2f)
            CreateSmallLine(metric);

        return new BottomMetricView { ValueText = valueText };
    }

    private void CreateBottomSun(RectTransform parent)
    {
        RectTransform sun = CreateUIObject("SunStatus", parent).GetComponent<RectTransform>();
        sun.anchorMin = new Vector2(1f, 0.5f);
        sun.anchorMax = new Vector2(1f, 0.5f);
        sun.pivot = new Vector2(0.5f, 0.5f);
        sun.anchoredPosition = new Vector2(-54f, 0f);
        sun.sizeDelta = new Vector2(88f, 88f);

        if (bottomSunIcon != null)
        {
            Image image = sun.gameObject.AddComponent<Image>();
            image.sprite = bottomSunIcon;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return;
        }

        Image core = CreateImage("Core", sun, menuAccentColor);
        core.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        core.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        core.rectTransform.sizeDelta = new Vector2(28f, 28f);

        for (int i = 0; i < 12; i++)
        {
            Image ray = CreateImage("Ray" + i, sun, menuAccentColor);
            RectTransform rayRect = ray.rectTransform;
            rayRect.anchorMin = new Vector2(0.5f, 0.5f);
            rayRect.anchorMax = new Vector2(0.5f, 0.5f);
            rayRect.pivot = new Vector2(0.5f, 0.5f);
            float angle = i * 30f;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            rayRect.anchoredPosition = direction * 32f;
            rayRect.sizeDelta = new Vector2(3f, 14f);
            rayRect.localEulerAngles = new Vector3(0f, 0f, angle);
        }
    }

    private void CreateBottomContinue(RectTransform parent)
    {
        GameObject buttonObject = CreateUIObject("BottomContinue", parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(bottomContinueWidth, bottomPanelHeight);

        Image background = buttonObject.AddComponent<Image>();
        background.sprite = bottomContinueSprite;
        background.color = bottomContinueSprite != null ? Color.white : new Color(0.01f, 0.009f, 0.008f, 0.9f);
        background.raycastTarget = true;
        background.type = bottomContinueSprite != null ? Image.Type.Sliced : Image.Type.Simple;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(CycleGameplaySpeed);

        if (bottomContinueSprite == null)
            CreateMenuBoxFrame(buttonRect, menuAccentColor);

        _gameplaySpeedButtonText = CreateText("Label", buttonRect, GetGameplaySpeedLabel(), 28f, menuAccentColor, TextAlignmentOptions.Center, FontStyles.Bold);
        TMP_Text label = _gameplaySpeedButtonText;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 8f);
        labelRect.offsetMax = new Vector2(-16f, -8f);
    }

    private void CreateSimpleMetricGlyph(RectTransform parent, string label)
    {
        Image ring = CreateImage("FallbackRing", parent, menuFrameColor);
        RectTransform ringRect = ring.rectTransform;
        ringRect.anchorMin = Vector2.zero;
        ringRect.anchorMax = Vector2.one;
        ringRect.offsetMin = Vector2.zero;
        ringRect.offsetMax = Vector2.zero;
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillAmount = 0.88f;

        string glyph = "?";
        if (label == "Hız")
            glyph = ">";
        else if (label == "Nüfus")
            glyph = "P";
        else if (label == "Altın")
            glyph = "$";
        else if (label == "İklim")
            glyph = "C";

        TMP_Text text = CreateText("FallbackGlyph", parent, glyph, 32f, iconColor, TextAlignmentOptions.Center, FontStyles.Bold);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private RectTransform CreateStatsParent(RectTransform parent)
    {
        GameObject statsObject = CreateUIObject("StatsArea", parent);
        RectTransform statsRect = statsObject.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0f, 1f);
        statsRect.anchorMax = new Vector2(statsWidthRatio, 1f);
        statsRect.pivot = new Vector2(0f, 1f);
        statsRect.anchoredPosition = Vector2.zero;
        statsRect.sizeDelta = new Vector2(0f, statsAreaHeight);
        return statsRect;
    }

    private void CreateMenuButton(RectTransform parent)
    {
        GameObject menuObject = CreateUIObject("SettingsMenu", parent);
        RectTransform menuRect = menuObject.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(1f, 1f);
        menuRect.anchorMax = new Vector2(1f, 1f);
        menuRect.pivot = new Vector2(1f, 1f);
        menuRect.anchoredPosition = new Vector2(-menuRightPadding, -17f);
        menuRect.sizeDelta = menuButtonSize;

        Image hitArea = menuObject.AddComponent<Image>();
        hitArea.color = new Color(1f, 1f, 1f, 0f);
        hitArea.raycastTarget = true;

        Button button = menuObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.82f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.62f);
        colors.selectedColor = Color.white;
        button.colors = colors;
        button.targetGraphic = hitArea;
        button.onClick.AddListener(() => ShowMenu(MenuMode.Pause));

        CreateMenuFrame(menuRect);

        if (menuIcon != null)
        {
            Image iconImage = CreateImage("MenuIcon", menuRect, menuStripeColor);
            RectTransform iconRect = iconImage.rectTransform;
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = menuIconSize;
            iconImage.sprite = menuIcon;
            iconImage.preserveAspect = true;
        }
        else
        {
            CreateMenuStripe(menuRect, 7f);
            CreateMenuStripe(menuRect, 0f);
            CreateMenuStripe(menuRect, -7f);
        }
    }

    private void CreateSpeedMetric(RectTransform parent)
    {
        GameObject metricObject = CreateUIObject("SpeedMetric", parent);
        RectTransform metricRect = metricObject.GetComponent<RectTransform>();
        metricRect.anchorMin = new Vector2(1f, 0.5f);
        metricRect.anchorMax = new Vector2(1f, 0.5f);
        metricRect.pivot = new Vector2(1f, 0.5f);
        metricRect.anchoredPosition = new Vector2(-34f, 0f);
        metricRect.sizeDelta = new Vector2(154f, 86f);

        Image background = metricObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.018f, 0.016f, 0.76f);
        background.raycastTarget = false;

        CreateMenuBoxFrame(metricRect, menuFrameColor);

        TMP_Text labelText = CreateText("Label", metricRect, "Hız", 20f, labelColor, TextAlignmentOptions.Center, labelFontStyle);
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(0f, -10f);
        labelRect.sizeDelta = new Vector2(-20f, 26f);

        _speedMetricValueText = CreateText("Value", metricRect, Mathf.RoundToInt(GameManager.DefaultSpeedKmh) + " km/h", 28f, valueColor, TextAlignmentOptions.Center, FontStyles.Bold);
        RectTransform valueRect = _speedMetricValueText.rectTransform;
        valueRect.anchorMin = new Vector2(0f, 0f);
        valueRect.anchorMax = new Vector2(1f, 0f);
        valueRect.pivot = new Vector2(0.5f, 0f);
        valueRect.anchoredPosition = new Vector2(0f, 12f);
        valueRect.sizeDelta = new Vector2(-20f, 40f);
    }

    private void BuildMenu()
    {
        ApplyMainButtonRuntimeLayout();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        Transform oldRoot = canvas.transform.Find("MainMenuRoot");
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        GameObject rootObject = CreateUIObject("MainMenuRoot", canvas.transform);
        _menuRoot = rootObject.GetComponent<RectTransform>();
        _menuRoot.anchorMin = Vector2.zero;
        _menuRoot.anchorMax = Vector2.one;
        _menuRoot.pivot = new Vector2(0.5f, 0.5f);
        _menuRoot.offsetMin = Vector2.zero;
        _menuRoot.offsetMax = Vector2.zero;

        RawImage videoImage = rootObject.AddComponent<RawImage>();
        videoImage.color = Color.white;
        videoImage.raycastTarget = true;

        if (mainMenuVideo != null)
        {
            _mainMenuVideoTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
            _mainMenuVideoTexture.Create();
            videoImage.texture = _mainMenuVideoTexture;

            _mainMenuVideoPlayer = rootObject.AddComponent<VideoPlayer>();
            _mainMenuVideoPlayer.clip = mainMenuVideo;
            _mainMenuVideoPlayer.isLooping = true;
            _mainMenuVideoPlayer.playOnAwake = true;
            _mainMenuVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _mainMenuVideoPlayer.targetTexture = _mainMenuVideoTexture;
            _mainMenuAudioSource = rootObject.AddComponent<AudioSource>();
            _mainMenuAudioSource.playOnAwake = false;
            _mainMenuAudioSource.loop = true;
            _mainMenuAudioSource.volume = mainMenuVideoVolume;

            _mainMenuVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            _mainMenuVideoPlayer.SetTargetAudioSource(0, _mainMenuAudioSource);
            _mainMenuVideoPlayer.SetDirectAudioVolume(0, mainMenuVideoVolume);
            _mainMenuVideoPlayer.Play();
        }
        else
        {
            videoImage.color = Color.black;
        }

        BuildMainMenuMusic(rootObject);

        RectTransform panel = CreateMenuPanel(_menuRoot);

        RectTransform buttons = CreateUIObject("Buttons", panel).GetComponent<RectTransform>();
        buttons.anchorMin = new Vector2(0f, 1f);
        buttons.anchorMax = new Vector2(0f, 1f);
        buttons.pivot = new Vector2(0f, 1f);
        buttons.anchoredPosition = mainMenuButtonStart;
        buttons.sizeDelta = new Vector2(390f, 284f);

        _startButton = CreateSpriteMenuButton("StartButton", buttons, newGameButtonSprite, "YENI OYUN", 0f, false);
        _startButton.onClick.AddListener(StartGame);

        _continueButton = CreateSpriteMenuButton("ContinueButton", buttons, continueButtonSprite, "DEVAM ET", -mainMenuButtonSpacing, false);
        _continueButton.onClick.AddListener(ResumeGame);
        SetContinueButtonState(false);

        Button settingsButton = CreateSpriteMenuButton("SettingsButton", buttons, settingsButtonSprite, "AYARLAR", -mainMenuButtonSpacing * 2f, false);
        settingsButton.onClick.AddListener(OpenSettings);

        Button quitButton = CreateSpriteMenuButton("QuitButton", buttons, quitButtonSprite, "CIKIS", -mainMenuButtonSpacing * 3f, false);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void BuildVictoryScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        Transform oldRoot = canvas.transform.Find("VictoryRoot");
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        GameObject rootObject = CreateUIObject("VictoryRoot", canvas.transform);
        _victoryRoot = rootObject.GetComponent<RectTransform>();
        _victoryRoot.anchorMin = Vector2.zero;
        _victoryRoot.anchorMax = Vector2.one;
        _victoryRoot.offsetMin = Vector2.zero;
        _victoryRoot.offsetMax = Vector2.zero;

        Image overlay = rootObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.72f);
        overlay.raycastTarget = true;

        RectTransform panel = CreateUIObject("VictoryPanel", _victoryRoot).GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(560f, 300f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.018f, 0.014f, 0.94f);
        panelImage.raycastTarget = false;
        CreateMenuBoxFrame(panel, menuAccentColor);

        TMP_Text title = CreateText("Title", panel, "YOL TAMAMLANDI", 38f, menuAccentColor, TextAlignmentOptions.Center, FontStyles.Normal);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -44f);
        titleRect.sizeDelta = new Vector2(-56f, 54f);

        TMP_Text body = CreateText("Body", panel, "Kabile hedefe ulaştı.", 22f, valueColor, TextAlignmentOptions.Center, FontStyles.Normal);
        RectTransform bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.anchoredPosition = new Vector2(0f, -116f);
        bodyRect.sizeDelta = new Vector2(-56f, 40f);

        Button mainMenuButton = CreateVictoryButton(panel);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        _victoryRoot.gameObject.SetActive(false);
    }

    private void BuildDefeatScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        Transform oldRoot = canvas.transform.Find("DefeatRoot");
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        GameObject rootObject = CreateUIObject("DefeatRoot", canvas.transform);
        _defeatRoot = rootObject.GetComponent<RectTransform>();
        _defeatRoot.anchorMin = Vector2.zero;
        _defeatRoot.anchorMax = Vector2.one;
        _defeatRoot.offsetMin = Vector2.zero;
        _defeatRoot.offsetMax = Vector2.zero;

        Image overlay = rootObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.76f);
        overlay.raycastTarget = true;

        RectTransform panel = CreateUIObject("DefeatPanel", _defeatRoot).GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(560f, 300f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0.014f, 0.012f, 0.95f);
        panelImage.raycastTarget = false;
        CreateMenuBoxFrame(panel, menuAccentColor);

        TMP_Text title = CreateText("Title", panel, "GÖÇ BAŞARISIZ", 38f, menuAccentColor, TextAlignmentOptions.Center, FontStyles.Normal);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -44f);
        titleRect.sizeDelta = new Vector2(-56f, 54f);

        TMP_Text body = CreateText("Body", panel, "Kabile yolculuğu sürdüremedi.", 22f, valueColor, TextAlignmentOptions.Center, FontStyles.Normal);
        RectTransform bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.anchoredPosition = new Vector2(0f, -116f);
        bodyRect.sizeDelta = new Vector2(-56f, 40f);

        Button mainMenuButton = CreateVictoryButton(panel);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        _defeatRoot.gameObject.SetActive(false);
    }

    private Button CreateVictoryButton(RectTransform parent)
    {
        GameObject buttonObject = CreateUIObject("MainMenuButton", parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 42f);
        buttonRect.sizeDelta = new Vector2(280f, 64f);

        Image background = buttonObject.AddComponent<Image>();
        background.color = menuButtonColor;
        background.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;

        ColorBlock colors = button.colors;
        colors.normalColor = menuButtonColor;
        colors.highlightedColor = menuButtonHoverColor;
        colors.pressedColor = menuAccentColor;
        colors.selectedColor = menuButtonHoverColor;
        button.colors = colors;

        CreateMenuBoxFrame(buttonRect, menuAccentColor);

        TMP_Text label = CreateText("Label", buttonRect, "ANA MENÜ", 23f, menuAccentColor, TextAlignmentOptions.Center, FontStyles.Normal);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    private RectTransform CreateMenuPanel(RectTransform parent)
    {
        GameObject panelObject = CreateUIObject("MenuPanel", parent);
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(520f, 560f);
        return panel;
    }

    private Button CreateSpriteMenuButton(string objectName, Transform parent, Sprite sprite, string fallbackText, float y, bool alwaysDesaturated)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.anchoredPosition = new Vector2(0f, y);
        buttonRect.sizeDelta = mainMenuButtonImageSize;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Material runtimeMaterial = CreateSaturationMaterial();
        if (runtimeMaterial != null)
            image.material = runtimeMaterial;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        MenuButtonView view = buttonObject.AddComponent<MenuButtonView>();
        view.RuntimeMaterial = runtimeMaterial;
        view.AlwaysDesaturated = alwaysDesaturated;
        view.SetSaturation(0f);

        if (sprite == null)
        {
            TMP_Text labelText = CreateText("FallbackLabel", buttonRect, fallbackText, 21f, valueColor, TextAlignmentOptions.Center, FontStyles.Normal);
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        return button;
    }

    private void ApplyMainButtonRuntimeLayout()
    {
        mainMenuButtonImageSize = new Vector2(640f, 86f);
        mainMenuButtonStart = new Vector2(150f, -520f);
        mainMenuButtonSpacing = 104f;
    }

    private Material CreateSaturationMaterial()
    {
        Shader shader = saturationShader != null ? saturationShader : Shader.Find("UI/Saturation");
        if (shader == null)
            return null;

        Material material = new Material(shader);
        material.SetFloat("_Saturation", 0f);
        return material;
    }

    private void CreateTitleOrnament(RectTransform parent, float x, float y, float width, bool diamondAtEnd)
    {
        Image line = CreateImage("AccentLine", parent, menuAccentColor);
        RectTransform lineRect = line.rectTransform;
        lineRect.anchorMin = new Vector2(0f, 1f);
        lineRect.anchorMax = new Vector2(0f, 1f);
        lineRect.pivot = new Vector2(0f, 0.5f);
        lineRect.anchoredPosition = new Vector2(x, y);
        lineRect.sizeDelta = new Vector2(width, 1.5f);

        Image diamond = CreateImage("AccentDiamond", parent, menuAccentColor);
        RectTransform diamondRect = diamond.rectTransform;
        diamondRect.anchorMin = new Vector2(0f, 1f);
        diamondRect.anchorMax = new Vector2(0f, 1f);
        diamondRect.pivot = new Vector2(0.5f, 0.5f);
        diamondRect.anchoredPosition = new Vector2(diamondAtEnd ? x + width : x, y);
        diamondRect.sizeDelta = new Vector2(7f, 7f);
        diamondRect.localEulerAngles = new Vector3(0f, 0f, 45f);
    }

    private void CreateSymbol(RectTransform parent)
    {
        RectTransform symbol = CreateUIObject("MenuSymbol", parent).GetComponent<RectTransform>();
        symbol.anchorMin = new Vector2(0f, 1f);
        symbol.anchorMax = new Vector2(0f, 1f);
        symbol.pivot = new Vector2(0.5f, 0.5f);
        symbol.anchoredPosition = new Vector2(235f, -8f);
        symbol.sizeDelta = new Vector2(54f, 74f);

        Image stem = CreateImage("Stem", symbol, menuAccentColor);
        stem.rectTransform.anchorMin = new Vector2(0.5f, 0.08f);
        stem.rectTransform.anchorMax = new Vector2(0.5f, 0.86f);
        stem.rectTransform.sizeDelta = new Vector2(2f, 0f);

        Image cross = CreateImage("Cross", symbol, menuAccentColor);
        cross.rectTransform.anchorMin = new Vector2(0.22f, 0.48f);
        cross.rectTransform.anchorMax = new Vector2(0.78f, 0.48f);
        cross.rectTransform.sizeDelta = new Vector2(0f, 2f);

        Image ring = CreateImage("Ring", symbol, menuAccentColor);
        ring.rectTransform.anchorMin = new Vector2(0.5f, 0.8f);
        ring.rectTransform.anchorMax = new Vector2(0.5f, 0.8f);
        ring.rectTransform.sizeDelta = new Vector2(42f, 42f);
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillAmount = 0.86f;
    }

    private Button CreateMenuButton(string objectName, Transform parent, string label, float y, bool highlighted)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.anchoredPosition = new Vector2(0f, y);
        buttonRect.sizeDelta = new Vector2(386f, 58f);

        Image background = buttonObject.AddComponent<Image>();
        background.color = highlighted ? menuButtonHoverColor : menuButtonColor;
        background.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = highlighted ? menuButtonHoverColor : menuButtonColor;
        colors.highlightedColor = menuButtonHoverColor;
        colors.pressedColor = menuAccentColor;
        colors.selectedColor = menuButtonHoverColor;
        colors.disabledColor = new Color(menuButtonColor.r, menuButtonColor.g, menuButtonColor.b, 0.48f);
        button.colors = colors;
        button.targetGraphic = background;

        CreateMenuBoxFrame(buttonRect, highlighted ? menuAccentColor : menuFrameColor);
        CreateButtonIcon(buttonRect, highlighted);

        TMP_Text labelText = CreateText("Label", buttonRect, label, 21f, highlighted ? menuAccentColor : menuDisabledColor, TextAlignmentOptions.Left, FontStyles.Normal);
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(106f, 0f);
        labelRect.offsetMax = new Vector2(-44f, 0f);

        if (highlighted)
        {
            TMP_Text arrow = CreateText("Arrow", buttonRect, ">", 26f, menuAccentColor, TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform arrowRect = arrow.rectTransform;
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-36f, 0f);
            arrowRect.sizeDelta = new Vector2(24f, 34f);
        }

        return button;
    }

    private void CreateButtonIcon(RectTransform parent, bool highlighted)
    {
        RectTransform icon = CreateUIObject("Icon", parent).GetComponent<RectTransform>();
        icon.anchorMin = new Vector2(0f, 0.5f);
        icon.anchorMax = new Vector2(0f, 0.5f);
        icon.pivot = new Vector2(0.5f, 0.5f);
        icon.anchoredPosition = new Vector2(55f, 0f);
        icon.sizeDelta = new Vector2(38f, 38f);

        Color color = highlighted ? menuAccentColor : menuDisabledColor;
        Image ring = CreateImage("Ring", icon, color);
        ring.rectTransform.anchorMin = Vector2.zero;
        ring.rectTransform.anchorMax = Vector2.one;
        ring.rectTransform.offsetMin = Vector2.zero;
        ring.rectTransform.offsetMax = Vector2.zero;
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillAmount = 0.92f;

        Image markA = CreateImage("MarkA", icon, color);
        markA.rectTransform.anchorMin = new Vector2(0.5f, 0.18f);
        markA.rectTransform.anchorMax = new Vector2(0.5f, 0.82f);
        markA.rectTransform.sizeDelta = new Vector2(2f, 0f);
        markA.rectTransform.localEulerAngles = new Vector3(0f, 0f, 38f);

        Image markB = CreateImage("MarkB", icon, color);
        markB.rectTransform.anchorMin = new Vector2(0.5f, 0.18f);
        markB.rectTransform.anchorMax = new Vector2(0.5f, 0.82f);
        markB.rectTransform.sizeDelta = new Vector2(2f, 0f);
        markB.rectTransform.localEulerAngles = new Vector3(0f, 0f, -38f);
    }

    private void CreateMenuBoxFrame(RectTransform parent, Color color)
    {
        Color soft = new Color(color.r, color.g, color.b, Mathf.Max(color.a, 0.34f));
        CreateMenuFrameLine("FrameTop", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1.5f), soft);
        CreateMenuFrameLine("FrameBottom", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 1.5f), soft);
        CreateMenuFrameLine("FrameLeft", parent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(1.5f, 0f), soft);
        CreateMenuFrameLine("FrameRight", parent, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(1.5f, 0f), soft);
    }

    private void CreateSocialBar(RectTransform parent)
    {
        RectTransform bar = CreateUIObject("SocialBar", parent).GetComponent<RectTransform>();
        bar.anchorMin = new Vector2(0f, 0f);
        bar.anchorMax = new Vector2(0f, 0f);
        bar.pivot = new Vector2(0f, 0f);
        bar.anchoredPosition = new Vector2(72f, 30f);
        bar.sizeDelta = new Vector2(238f, 58f);

        Image background = bar.gameObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.018f, 0.016f, 0.65f);
        background.raycastTarget = false;

        CreateMenuBoxFrame(bar, menuFrameColor);

        string[] labels = { "D", "X", "f", ">" };
        for (int i = 0; i < labels.Length; i++)
        {
            TMP_Text item = CreateText("Social" + i, bar, labels[i], 20f, i == 3 ? menuAccentColor : menuDisabledColor, TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform itemRect = item.rectTransform;
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(0f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.anchoredPosition = new Vector2(32f + i * 55f, 0f);
            itemRect.sizeDelta = new Vector2(34f, 34f);

            if (i > 0)
            {
                Image divider = CreateImage("SocialDivider" + i, bar, menuFrameColor);
                RectTransform dividerRect = divider.rectTransform;
                dividerRect.anchorMin = new Vector2(0f, 0.5f);
                dividerRect.anchorMax = new Vector2(0f, 0.5f);
                dividerRect.pivot = new Vector2(0.5f, 0.5f);
                dividerRect.anchoredPosition = new Vector2(5f + i * 55f, 0f);
                dividerRect.sizeDelta = new Vector2(1f, 28f);
            }
        }
    }

    private void CreateCornerSettings(RectTransform parent)
    {
        GameObject settingsObject = CreateUIObject("BottomSettings", parent);
        RectTransform settingsRect = settingsObject.GetComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(1f, 0f);
        settingsRect.anchorMax = new Vector2(1f, 0f);
        settingsRect.pivot = new Vector2(1f, 0f);
        settingsRect.anchoredPosition = new Vector2(-46f, 30f);
        settingsRect.sizeDelta = new Vector2(58f, 58f);

        Image background = settingsObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.018f, 0.016f, 0.65f);
        background.raycastTarget = true;

        Button settings = settingsObject.AddComponent<Button>();
        settings.targetGraphic = background;
        settings.onClick.AddListener(() => ShowMenu(MenuMode.Pause));

        CreateMenuBoxFrame(settingsRect, menuFrameColor);
        CreateGearIcon(settingsRect, menuDisabledColor);
    }

    private void CreateGearIcon(RectTransform parent, Color color)
    {
        Image ring = CreateImage("GearRing", parent, color);
        RectTransform ringRect = ring.rectTransform;
        ringRect.anchorMin = new Vector2(0.5f, 0.5f);
        ringRect.anchorMax = new Vector2(0.5f, 0.5f);
        ringRect.pivot = new Vector2(0.5f, 0.5f);
        ringRect.anchoredPosition = Vector2.zero;
        ringRect.sizeDelta = new Vector2(28f, 28f);
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillAmount = 0.84f;

        for (int i = 0; i < 8; i++)
        {
            Image tooth = CreateImage("GearTooth" + i, parent, color);
            RectTransform toothRect = tooth.rectTransform;
            toothRect.anchorMin = new Vector2(0.5f, 0.5f);
            toothRect.anchorMax = new Vector2(0.5f, 0.5f);
            toothRect.pivot = new Vector2(0.5f, 0.5f);
            float angle = i * 45f;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            toothRect.anchoredPosition = direction * 16f;
            toothRect.sizeDelta = new Vector2(3f, 8f);
            toothRect.localEulerAngles = new Vector3(0f, 0f, angle);
        }
    }

    private void CreateScreenFrame(RectTransform parent)
    {
        _menuFrameRoot = CreateUIObject("ScreenFrame", parent).GetComponent<RectTransform>();
        _menuFrameRoot.anchorMin = Vector2.zero;
        _menuFrameRoot.anchorMax = Vector2.one;
        _menuFrameRoot.offsetMin = new Vector2(12f, 12f);
        _menuFrameRoot.offsetMax = new Vector2(-12f, -12f);

        CreateMenuFrameLine("Top", _menuFrameRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f), menuFrameColor);
        CreateMenuFrameLine("Bottom", _menuFrameRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 1f), menuFrameColor);
        CreateMenuFrameLine("Left", _menuFrameRoot, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(1f, 0f), menuFrameColor);
        CreateMenuFrameLine("Right", _menuFrameRoot, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(1f, 0f), menuFrameColor);
    }

    private void ShowMenu(MenuMode mode)
    {
        if (_menuRoot == null)
            return;

        if (_victoryRoot != null)
            _victoryRoot.gameObject.SetActive(false);

        if (_defeatRoot != null)
            _defeatRoot.gameObject.SetActive(false);

        bool isMainMenu = mode == MenuMode.Main;
        _menuRoot.gameObject.SetActive(true);
        if (_root != null)
            _root.gameObject.SetActive(!isMainMenu);

        if (_bottomRoot != null)
            _bottomRoot.gameObject.SetActive(!isMainMenu);

        SetContinueButtonState(!isMainMenu && _hasActiveRun && _gameStarted && !_gameEnded);

        if (_startButton != null)
            _startButton.gameObject.SetActive(true);

        PauseTime();
    }

    private void CheckGameEndConditions()
    {
        if (!_gameStarted || _gameEnded)
            return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            return;

        if (gameManager.Health <= 0f)
        {
            ShowDefeatScreen();
            Debug.Log("Oyun kaybedildi: Sağlık 0 oldu.");
            return;
        }

        if (gameManager.Population <= 0)
        {
            ShowDefeatScreen();
            Debug.Log("Oyun kaybedildi: Nüfus 0 oldu.");
        }
    }

    public void ShowVictoryScreen()
    {
        _gameStarted = false;
        _gameEnded = true;
        _hasActiveRun = false;

        if (_menuRoot != null)
            _menuRoot.gameObject.SetActive(false);

        if (_root != null)
            _root.gameObject.SetActive(false);

        if (_bottomRoot != null)
            _bottomRoot.gameObject.SetActive(false);

        if (_victoryRoot != null)
            _victoryRoot.gameObject.SetActive(true);

        PauseTime();
    }

    public void ShowDefeatScreen()
    {
        _gameStarted = false;
        _gameEnded = true;
        _hasActiveRun = false;

        if (_menuRoot != null)
            _menuRoot.gameObject.SetActive(false);

        if (_root != null)
            _root.gameObject.SetActive(false);

        if (_bottomRoot != null)
            _bottomRoot.gameObject.SetActive(false);

        if (_victoryRoot != null)
            _victoryRoot.gameObject.SetActive(false);

        if (_defeatRoot != null)
            _defeatRoot.gameObject.SetActive(true);

        PauseTime();
    }

    public void RestoreGameplayTimeScale()
    {
        if (!_gameStarted || _gameEnded)
            return;

        Time.timeScale = GetGameplayTimeScale();
    }

    private void ReturnToMainMenu()
    {
        ShowMenu(MenuMode.Main);
    }

    private void OpenSettings()
    {
        // Settings panel is not implemented yet; keep the current menu state intact.
    }

    private void SetContinueButtonState(bool canContinue)
    {
        if (_continueButton == null)
            return;

        _continueButton.interactable = canContinue;

        MenuButtonView view = _continueButton.GetComponent<MenuButtonView>();
        if (view != null)
        {
            view.AlwaysDesaturated = !canContinue;
            view.SetSaturation(0f);
        }
    }

    private void StartGame()
    {
        s_startGameAfterSceneReload = true;
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetGameState();

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
            return;
        }

        Debug.LogWarning("Aktif sahne Build Settings içinde değil. Yeni oyun sahne reload yapmadan sıfırlandı.");
        s_startGameAfterSceneReload = false;
        BeginNewGameSession();
    }

    private void BeginNewGameSession()
    {
        ResetGameForNewRun();
        _hasActiveRun = true;
        _gameStarted = true;
        _gameEnded = false;
        _gameplayTimeScaleIndex = 0;
        UpdateGameplaySpeedButtonText();
        HideMenu();
    }

    private void ResumeGame()
    {
        if (!_gameStarted || _gameEnded)
            return;

        HideMenu();
    }

    private void ResetGameForNewRun()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGameState();

        ResourceDrainManager resourceDrainManager = FindFirstObjectByType<ResourceDrainManager>();
        if (resourceDrainManager != null)
            resourceDrainManager.ResetResources();

        ClanMovement clanMovement = FindFirstObjectByType<ClanMovement>();
        if (clanMovement != null)
            clanMovement.ResetJourney();
    }

    private void HideMenu()
    {
        if (_menuRoot != null)
            _menuRoot.gameObject.SetActive(false);

        if (_victoryRoot != null)
            _victoryRoot.gameObject.SetActive(false);

        if (_defeatRoot != null)
            _defeatRoot.gameObject.SetActive(false);

        if (_root != null)
            _root.gameObject.SetActive(true);

        if (_bottomRoot != null)
            _bottomRoot.gameObject.SetActive(true);

        RestoreGameplayTimeScale();
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void CycleGameplaySpeed()
    {
        if (!_gameStarted || _gameEnded)
            return;

        _gameplayTimeScaleIndex = (_gameplayTimeScaleIndex + 1) % _gameplayTimeScales.Length;
        UpdateGameplaySpeedButtonText();
        RestoreGameplayTimeScale();
        Debug.Log("Oyun hızı: " + GetGameplaySpeedLabel());
    }

    private float GetGameplayTimeScale()
    {
        if (_gameplayTimeScales.Length == 0)
            return 1f;

        int safeIndex = Mathf.Clamp(_gameplayTimeScaleIndex, 0, _gameplayTimeScales.Length - 1);
        return _gameplayTimeScales[safeIndex];
    }

    private string GetGameplaySpeedLabel()
    {
        return Mathf.RoundToInt(GetGameplayTimeScale()) + "X";
    }

    private void UpdateGameplaySpeedButtonText()
    {
        if (_gameplaySpeedButtonText != null)
            _gameplaySpeedButtonText.text = GetGameplaySpeedLabel();
    }

    private void PauseTime()
    {
        Time.timeScale = 0f;
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
        iconRect.anchoredPosition = new Vector2(iconLeftPadding + iconSize * 0.5f, -1f);
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
        iconImage.enabled = icon != null;

        TMP_Text labelText = CreateText("Label", statRect, label, labelFontSize, labelColor, TextAlignmentOptions.Left, labelFontStyle);
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        float contentLeft = contentAfterIcon;
        float contentRight = horizontalPadding * 0.7f;

        const float textTopOffset = -18f;
        const float textHeight = 34f;

        labelRect.anchoredPosition = new Vector2(contentLeft, textTopOffset);
        labelRect.sizeDelta = new Vector2(156f, textHeight);

        TMP_Text valueText = CreateText("Value", statRect, "", valueFontSize, valueColor, TextAlignmentOptions.Right, valueFontStyle);
        RectTransform valueRect = valueText.rectTransform;
        valueRect.anchorMin = new Vector2(1f, 1f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 1f);
        valueRect.anchoredPosition = new Vector2(-contentRight, textTopOffset);
        valueRect.sizeDelta = new Vector2(166f, textHeight);

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
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        CreateBarOutline(barBackRect);

        if (index < 3f)
            CreateSmallLine(statRect);

        return new StatView
        {
            Stat = stat,
            ValueText = valueText,
            FillImage = fillImage,
            FillRect = fillRect
        };
    }

    private void UpdateHud()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
            return;

        if (_speedMetricValueText != null)
            _speedMetricValueText.text = Mathf.RoundToInt(gameManager.Speed) + " km/h";

        UpdateBottomHud(gameManager);

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

            if (view.FillRect != null)
            {
                view.FillRect.anchorMax = new Vector2(normalizedValue, 1f);
                view.FillRect.offsetMax = Vector2.zero;
            }
        }
    }

    private void UpdateBottomHud(GameManager gameManager)
    {
        SetBottomMetric(0, gameManager.Population + " / 20");
        SetBottomMetric(1, gameManager.Gold.ToString());
        SetBottomMetric(2, GetClimateLabel(gameManager.CurrentClimate));
    }

    private void SetBottomMetric(int index, string value)
    {
        if (index < 0 || index >= _bottomMetricViews.Length)
            return;

        BottomMetricView view = _bottomMetricViews[index];
        if (view == null)
            return;

        if (view.ValueText != null)
            view.ValueText.text = value;
    }

    private string GetClimateLabel(GameManager.Climate climate)
    {
        switch (climate)
        {
            case GameManager.Climate.Tundra:
                return "TUNDRA";
            case GameManager.Climate.Col:
                return "ÇÖL";
            case GameManager.Climate.Iliman:
            default:
                return "ILIMAN";
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

    private Sprite CreateBottomGradientSprite()
    {
        if (_bottomGradientSprite != null)
            return _bottomGradientSprite;

        const int width = 2;
        const int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            float t = y / (height - 1f);
            Color color;
            if (t < 0.42f)
                color = Color.Lerp(bottomPanelBottomColor, bottomPanelMiddleColor, Mathf.SmoothStep(0f, 1f, t / 0.42f));
            else
                color = Color.Lerp(bottomPanelMiddleColor, bottomPanelTopColor, Mathf.SmoothStep(0f, 1f, (t - 0.42f) / 0.58f));

            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, color);
        }

        texture.Apply();
        _bottomGradientSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f));
        return _bottomGradientSprite;
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

    private void CreateMenuFrame(RectTransform parent)
    {
        CreateMenuFrameLine("MenuFrameTop", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f));
        CreateMenuFrameLine("MenuFrameBottom", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 1f));
        CreateMenuFrameLine("MenuFrameLeft", parent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(1f, 0f));
        CreateMenuFrameLine("MenuFrameRight", parent, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(1f, 0f));
    }

    private void CreateMenuFrameLine(string objectName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        CreateMenuFrameLine(objectName, parent, anchorMin, anchorMax, pivot, sizeDelta, menuFrameColor);
    }

    private void CreateMenuFrameLine(string objectName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Color color)
    {
        Image line = CreateImage(objectName, parent, color);
        RectTransform lineRect = line.rectTransform;
        lineRect.anchorMin = anchorMin;
        lineRect.anchorMax = anchorMax;
        lineRect.pivot = pivot;
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = sizeDelta;
    }

    private void CreateMenuStripe(RectTransform parent, float y)
    {
        Image stripe = CreateImage("MenuStripe", parent, menuStripeColor);
        RectTransform stripeRect = stripe.rectTransform;
        stripeRect.anchorMin = new Vector2(0.5f, 0.5f);
        stripeRect.anchorMax = new Vector2(0.5f, 0.5f);
        stripeRect.pivot = new Vector2(0.5f, 0.5f);
        stripeRect.anchoredPosition = new Vector2(0f, y);
        stripeRect.sizeDelta = new Vector2(menuIconSize.x, 2f);
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

    private TMP_Text CreateText(string objectName, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment, FontStyles fontStyle)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.font = fontAsset;
        tmpText.fontSize = fontSize;
        tmpText.fontStyle = fontStyle;
        tmpText.color = color;
        tmpText.alignment = alignment;
        tmpText.characterSpacing = characterSpacing;
        tmpText.raycastTarget = false;
        tmpText.textWrappingMode = TextWrappingModes.NoWrap;
        return tmpText;
    }
}
