using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [Range(0.3f, 1f)]
    public float statsWidthRatio = 0.6f;
    public float horizontalPadding = 42f;
    public float statWidth = 260f;
    public float statSpacing = 28f;
    public float iconSize = 34f;
<<<<<<< Updated upstream
    public float barHeight = 14f;
    public float barTopOffset = -62f;
=======
    public float iconLeftPadding = 18f;
    public float contentAfterIcon = 48f;
    public float barHeight = 8f;
    public float barTopOffset = -47f;
>>>>>>> Stashed changes

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
    public Color labelColor = new Color(0.78f, 0.74f, 0.68f, 1f);
    public Color valueColor = new Color(0.84f, 0.82f, 0.78f, 1f);
    public Color barBackColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
    public Color barFillColor = new Color(0.72f, 0.12f, 0.12f, 1f);
    public Color barOutlineColor = new Color(0.72f, 0.72f, 0.72f, 0.16f);
    public Color menuOverlayColor = new Color(0f, 0f, 0f, 0.04f);
    public Color menuAccentColor = new Color(0.72f, 0.1f, 0.08f, 1f);
    public Color menuButtonColor = new Color(0.025f, 0.023f, 0.021f, 0.74f);
    public Color menuButtonHoverColor = new Color(0.08f, 0.04f, 0.035f, 0.88f);
    public Color menuDisabledColor = new Color(0.55f, 0.55f, 0.55f, 0.48f);

    [Header("Typography")]
    public TMP_FontAsset fontAsset;
    public float labelFontSize = 22f;
    public float valueFontSize = 21f;
    public FontStyles labelFontStyle = FontStyles.Normal;
    public FontStyles valueFontStyle = FontStyles.Normal;
    public float characterSpacing = 0f;

    private readonly StatView[] _statViews = new StatView[4];
    private RectTransform _root;
    private RectTransform _menuRoot;
    private RectTransform _menuFrameRoot;
    private Button _continueButton;
    private Button _startButton;
    private VideoPlayer _mainMenuVideoPlayer;
    private AudioSource _mainMenuAudioSource;
    private AudioSource _mainMenuMusicSource;
    private RenderTexture _mainMenuVideoTexture;
    private bool _gameStarted;
    private Sprite _panelGradientSprite;

    private void Awake()
    {
        BuildHud();
        BuildMenu();
        ShowMenu(MenuMode.Main);
    }

    private void Update()
    {
        if (_gameStarted && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ShowMenu(MenuMode.Pause);

        UpdateHud();
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

        _continueButton = CreateSpriteMenuButton("ContinueButton", buttons, continueButtonSprite, "DEVAM ET", -mainMenuButtonSpacing, true);
        _continueButton.interactable = false;

        Button settingsButton = CreateSpriteMenuButton("SettingsButton", buttons, settingsButtonSprite, "AYARLAR", -mainMenuButtonSpacing * 2f, false);
        settingsButton.onClick.AddListener(OpenSettings);

        Button quitButton = CreateSpriteMenuButton("QuitButton", buttons, quitButtonSprite, "CIKIS", -mainMenuButtonSpacing * 3f, false);
        quitButton.onClick.AddListener(QuitGame);
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

        bool isMainMenu = mode == MenuMode.Main;
        _menuRoot.gameObject.SetActive(true);
        if (_root != null)
            _root.gameObject.SetActive(!isMainMenu);

        if (_continueButton != null)
            _continueButton.interactable = false;

        if (_startButton != null)
            _startButton.gameObject.SetActive(true);

        Time.timeScale = 0f;
    }

    private void OpenSettings()
    {
        // Settings panel is not implemented yet; keep the current menu state intact.
    }

    private void StartGame()
    {
        _gameStarted = true;
        HideMenu();
    }

    private void ResumeGame()
    {
        if (!_gameStarted)
            return;

        HideMenu();
    }

    private void HideMenu()
    {
        if (_menuRoot != null)
            _menuRoot.gameObject.SetActive(false);

        if (_root != null)
            _root.gameObject.SetActive(true);

        Time.timeScale = 1f;
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
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

        labelRect.anchoredPosition = new Vector2(contentLeft, -28f);
        labelRect.sizeDelta = new Vector2(126f, 34f);

        TMP_Text valueText = CreateText("Value", statRect, "", valueFontSize, valueColor, TextAlignmentOptions.Right, valueFontStyle);
        RectTransform valueRect = valueText.rectTransform;
        valueRect.anchorMin = new Vector2(1f, 1f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 1f);
        valueRect.anchoredPosition = new Vector2(-contentRight, -28f);
        valueRect.sizeDelta = new Vector2(118f, 34f);

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
