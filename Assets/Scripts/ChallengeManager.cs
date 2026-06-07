using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChallengeManager : MonoBehaviour
{
    private class ChallengeOption
    {
        public string Title;
        public string Description;
        public int SpeedDelta;
        public int HealthDelta;
        public int FoodDelta;

        public ChallengeOption(string title, string description, int speedDelta, int healthDelta, int foodDelta)
        {
            Title = title;
            Description = description;
            SpeedDelta = speedDelta;
            HealthDelta = healthDelta;
            FoodDelta = foodDelta;
        }
    }

    private class ClimateChallenge
    {
        public ClimateType Climate;
        public string RegionName;
        public string Title;
        public string Description;
        public ChallengeOption[] Options;

        public ClimateChallenge(ClimateType climate, string regionName, string title, string description, ChallengeOption[] options)
        {
            Climate = climate;
            RegionName = regionName;
            Title = title;
            Description = description;
            Options = options;
        }
    }

    private class ChallengeCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform RectTransform;
        public Image Background;
        public Image Highlight;
        public Vector2 BasePosition;
        public Color NormalColor;
        public Color HoverColor;
        public float HoverLift = 24f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (RectTransform != null)
                RectTransform.anchoredPosition = BasePosition + Vector2.up * HoverLift;

            if (Background != null)
                Background.color = HoverColor;

            if (Highlight != null)
                Highlight.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (RectTransform != null)
                RectTransform.anchoredPosition = BasePosition;

            if (Background != null)
                Background.color = NormalColor;

            if (Highlight != null)
                Highlight.enabled = false;
        }
    }

    public static ChallengeManager Instance { get; private set; }

    [Header("Trigger")]
    [Range(0f, 1f)] public float minSegmentPercent = 0.3f;
    [Range(0f, 1f)] public float maxSegmentPercent = 0.9f;
    public float fallbackSegmentDistance = 300f;

    [Header("Card Layout")]
    public Vector2 cardSize = new Vector2(310f, 430f);
    public float cardSpacing = 34f;
    public float cardHoverLift = 24f;
    public Color overlayColor = new Color(0f, 0f, 0f, 0.72f);
    public Color cardColor = new Color(0.075f, 0.065f, 0.052f, 0.96f);
    public Color cardHoverColor = new Color(0.12f, 0.095f, 0.065f, 0.98f);
    public Color highlightColor = new Color(0.95f, 0.68f, 0.25f, 0.86f);
    public Color textColor = new Color(0.94f, 0.9f, 0.82f, 1f);
    public Color mutedTextColor = new Color(0.74f, 0.69f, 0.62f, 1f);
    public TMP_FontAsset fontAsset;

    private readonly ClimateChallenge[] _challenges = BuildChallenges();
    private RectTransform _root;
    private TMP_Text _regionText;
    private TMP_Text _titleText;
    private TMP_Text _descriptionText;
    private ClimateType _activeClimate;
    private float _triggerX;
    private bool _hasSchedule;
    private bool _hasShownCurrentClimateChallenge;
    private bool _isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUI();
        HideChallenge(false);
    }

    private void Update()
    {
        if (!_hasSchedule || _hasShownCurrentClimateChallenge || _isShowing)
            return;

        Transform player = FindPlayerTransform();
        if (player == null)
            return;

        if (player.position.x >= _triggerX)
            ShowRandomChallenge(_activeClimate);
    }

    public void ScheduleChallenge(ClimateType climate, float climateStartX, float segmentDistance)
    {
        _activeClimate = climate;
        _triggerX = climateStartX + Mathf.Max(segmentDistance, 1f) * Random.Range(minSegmentPercent, maxSegmentPercent);
        _hasSchedule = true;
        _hasShownCurrentClimateChallenge = false;
    }

    private void ShowRandomChallenge(ClimateType climate)
    {
        ClimateChallenge[] availableChallenges = GetChallengesForClimate(climate);
        if (availableChallenges.Length == 0)
            return;

        ClimateChallenge challenge = availableChallenges[Random.Range(0, availableChallenges.Length)];
        ShowChallenge(challenge);
    }

    private void ShowChallenge(ClimateChallenge challenge)
    {
        if (_root == null)
            BuildUI();

        if (_root == null)
            return;

        ClearCards();

        _hasShownCurrentClimateChallenge = true;
        _isShowing = true;
        _root.gameObject.SetActive(true);

        if (_regionText != null)
            _regionText.text = challenge.RegionName;

        if (_titleText != null)
            _titleText.text = challenge.Title;

        if (_descriptionText != null)
            _descriptionText.text = challenge.Description;

        CreateCards(challenge);
        Time.timeScale = 0f;
    }

    private void ApplyOption(ChallengeOption option)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.Speed = Mathf.Clamp(gameManager.Speed + option.SpeedDelta, 0f, 100f);
            gameManager.Health += option.HealthDelta;
            gameManager.FoodSupplies += option.FoodDelta;
        }

        ResourceDrainManager resourceDrainManager = FindFirstObjectByType<ResourceDrainManager>();
        if (resourceDrainManager != null)
            resourceDrainManager.ApplyChallengeEffects(option.HealthDelta, option.FoodDelta);

        HideChallenge(true);
    }

    private void HideChallenge(bool resumeTime)
    {
        _isShowing = false;

        if (_root != null)
            _root.gameObject.SetActive(false);

        if (resumeTime)
        {
            TopHudController topHudController = FindFirstObjectByType<TopHudController>();
            if (topHudController != null)
                topHudController.RestoreGameplayTimeScale();
            else
                Time.timeScale = 1f;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform oldRoot = canvas.transform.Find("ChallengeRoot");
        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        GameObject rootObject = CreateUIObject("ChallengeRoot", canvas.transform);
        _root = rootObject.GetComponent<RectTransform>();
        _root.anchorMin = Vector2.zero;
        _root.anchorMax = Vector2.one;
        _root.offsetMin = Vector2.zero;
        _root.offsetMax = Vector2.zero;

        Image overlay = rootObject.AddComponent<Image>();
        overlay.color = overlayColor;
        overlay.raycastTarget = true;

        _regionText = CreateText("Region", _root, "", 23f, mutedTextColor, TextAlignmentOptions.Center, FontStyles.SmallCaps);
        SetRect(_regionText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -106f), new Vector2(900f, 38f));

        _titleText = CreateText("Title", _root, "", 38f, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(_titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(980f, 56f));

        _descriptionText = CreateText("Description", _root, "", 23f, mutedTextColor, TextAlignmentOptions.Center, FontStyles.Normal);
        SetRect(_descriptionText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -208f), new Vector2(980f, 72f));
        _descriptionText.textWrappingMode = TextWrappingModes.Normal;
    }

    private void CreateCards(ClimateChallenge challenge)
    {
        RectTransform cardsRoot = CreateUIObject("Cards", _root).GetComponent<RectTransform>();
        cardsRoot.anchorMin = new Vector2(0.5f, 0.5f);
        cardsRoot.anchorMax = new Vector2(0.5f, 0.5f);
        cardsRoot.pivot = new Vector2(0.5f, 0.5f);
        cardsRoot.anchoredPosition = new Vector2(0f, -70f);
        cardsRoot.sizeDelta = new Vector2(cardSize.x * 3f + cardSpacing * 2f, cardSize.y + cardHoverLift);

        for (int i = 0; i < challenge.Options.Length; i++)
            CreateCard(cardsRoot, challenge.Options[i], i);
    }

    private void CreateCard(RectTransform parent, ChallengeOption option, int index)
    {
        GameObject cardObject = CreateUIObject("Card" + index, parent);
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        Vector2 basePosition = new Vector2((index - 1f) * (cardSize.x + cardSpacing), 0f);
        cardRect.anchoredPosition = basePosition;
        cardRect.sizeDelta = cardSize;

        Image background = cardObject.AddComponent<Image>();
        background.color = cardColor;
        background.raycastTarget = true;

        Button button = cardObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => ApplyOption(option));

        Image highlight = CreateImage("Highlight", cardRect, highlightColor);
        highlight.rectTransform.anchorMin = Vector2.zero;
        highlight.rectTransform.anchorMax = Vector2.one;
        highlight.rectTransform.offsetMin = new Vector2(-5f, -5f);
        highlight.rectTransform.offsetMax = new Vector2(5f, 5f);
        highlight.transform.SetAsFirstSibling();
        highlight.enabled = false;

        ChallengeCardView cardView = cardObject.AddComponent<ChallengeCardView>();
        cardView.RectTransform = cardRect;
        cardView.Background = background;
        cardView.Highlight = highlight;
        cardView.BasePosition = basePosition;
        cardView.NormalColor = cardColor;
        cardView.HoverColor = cardHoverColor;
        cardView.HoverLift = cardHoverLift;

        TMP_Text title = CreateText("CardTitle", cardRect, option.Title, 25f, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(-34f, 70f));
        title.textWrappingMode = TextWrappingModes.Normal;

        TMP_Text description = CreateText("CardDescription", cardRect, option.Description, 19f, mutedTextColor, TextAlignmentOptions.Center, FontStyles.Normal);
        SetRect(description.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(-40f, 160f));
        description.textWrappingMode = TextWrappingModes.Normal;

        TMP_Text effects = CreateText("CardEffects", cardRect, FormatEffects(option), 22f, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
        SetRect(effects.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(-34f, 82f));
        effects.textWrappingMode = TextWrappingModes.Normal;
    }

    private string FormatEffects(ChallengeOption option)
    {
        return FormatDelta(option.SpeedDelta) + " km/h HIZ\n" + FormatDelta(option.HealthDelta) + " SGL\n" + FormatDelta(option.FoodDelta) + " ERZ";
    }

    private string FormatDelta(int value)
    {
        if (value > 0)
            return "+" + value;

        return value.ToString();
    }

    private ClimateChallenge[] GetChallengesForClimate(ClimateType climate)
    {
        int count = 0;
        for (int i = 0; i < _challenges.Length; i++)
        {
            if (_challenges[i].Climate == climate)
                count++;
        }

        ClimateChallenge[] result = new ClimateChallenge[count];
        int index = 0;
        for (int i = 0; i < _challenges.Length; i++)
        {
            if (_challenges[i].Climate == climate)
            {
                result[index] = _challenges[i];
                index++;
            }
        }

        return result;
    }

    private Transform FindPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private void ClearCards()
    {
        if (_root == null)
            return;

        Transform oldCards = _root.Find("Cards");
        if (oldCards != null)
            Destroy(oldCards.gameObject);
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
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
        tmpText.characterSpacing = 0f;
        tmpText.raycastTarget = false;
        tmpText.textWrappingMode = TextWrappingModes.NoWrap;
        return tmpText;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static ClimateChallenge[] BuildChallenges()
    {
        return new[]
        {
            new ClimateChallenge(
                ClimateType.Iliman,
                "Bozkir Iklimi - Ucsuz Bucaksiz Duzlukler",
                "Surude Salgin Hastalik",
                "Obanin en degerli kaynagi olan buyukbas hayvanlarda olumcul bir hastalik bas gosterdi.",
                new[]
                {
                    new ChallengeOption("Karantina Alani Kur", "Hasta hayvanlari ayirmak icin gocu yavaslat ve ahsap citler insa et.", -15, 5, -5),
                    new ChallengeOption("Hastalari Gom", "Gocu durdurmamak icin hastalananlari hizla itlaf edip gomerek yola devam et.", -10, -10, 0),
                    new ChallengeOption("Hastalikli Eti Al", "Olmek uzere olan hayvanlari kesip etlerini ve kanini erzaga kat.", 0, -15, 15)
                }),
            new ClimateChallenge(
                ClimateType.Iliman,
                "Bozkir Iklimi - Ucsuz Bucaksiz Duzlukler",
                "Kavurucu Bozkir Kurakligi",
                "Haftalardir tek damla yagmur dusmedi ve rotadaki nehir yataklari tamamen kurumus durumda.",
                new[]
                {
                    new ChallengeOption("Uzak Kuyuya Sap", "Ana rotadan saparak eskilerin bahsettigi derin kuyuya dogru gunlerce ilerle.", -20, 10, -5),
                    new ChallengeOption("Sadece Gece Yuru", "Kavurucu sicaktan korunmak icin gunduzleri golgelik kur, sadece geceleri yol al.", 0, -15, -5),
                    new ChallengeOption("Izcileri Dagit", "Su bulmalari umuduyla en yetenekli avcilari ve kopekleri onden tehlikeye yolla.", -10, 5, 5)
                }),
            new ClimateChallenge(
                ClimateType.Tundra,
                "Tundra Iklimi - Dondurucu Soguklar",
                "Yolu Kapatan Kar Yiginlari",
                "Dar bir dag gecidinde yogun kar birikintisi goc yolunu tamamen tikamis durumda.",
                new[]
                {
                    new ChallengeOption("Kurekle Yolu Ac", "Insan gucuyle saatlerce kar kureyerek yolu temizle. Donma riski var.", 0, -10, -5),
                    new ChallengeOption("Etrafindan Dolas", "Yolu temizlemek yerine uzun ama acik olan vadi yolunu sec.", -15, 5, 0),
                    new ChallengeOption("Suruyu Onden Sur", "Iri hayvanlari karda yurutup ezdire ezdire yolu actir.", 10, 0, -10)
                }),
            new ClimateChallenge(
                ClimateType.Tundra,
                "Tundra Iklimi - Dondurucu Soguklar",
                "Goz Gozu Gormeyen Tipi",
                "Aniden bastiran beyaz karanlik yon duygunuzu tamamen kaybettiriyor.",
                new[]
                {
                    new ChallengeOption("Korlemesine Ilerle", "Gocu durdurma, kayiplari goze alarak tahmini yonde ilerle.", 10, -15, -10),
                    new ChallengeOption("Cadirlari Birlestir", "Hareketi tamamen kes, tum cadirlari birbirine baglayip isiyi koru.", -20, 10, -5),
                    new ChallengeOption("Hayvanlari Izle", "Atlarin ve kopeklerin icgudulerine guvenerek firtinada yolu bulmaya calis.", -5, 5, -10)
                }),
            new ClimateChallenge(
                ClimateType.Col,
                "Col Iklimi - Kavurucu Kumlar",
                "Yaklasan Kum Firtinasi",
                "Ufukta devasa, sicak bir kum firtinasi yaklasiyor. Yakinda nefes almak bile zorlasacak.",
                new[]
                {
                    new ChallengeOption("Kumun Icine Gomul", "Hayvanlari ve insanlari kum siperleri kazarak firtinanin gecmesini beklet.", -15, 10, -5),
                    new ChallengeOption("Geceye Kadar Zorla", "Firtinanin icinden gecip gece serinligine bir an once ulasmaya calis.", 15, -15, -10),
                    new ChallengeOption("Esyalari Feda Et", "Agir yukleri geride birakarak hizlanip firtina alanindan kac.", 10, 5, -20)
                }),
            new ClimateChallenge(
                ClimateType.Col,
                "Col Iklimi - Kavurucu Kumlar",
                "Yumusak Kum Cokuntusu",
                "Yumusak kum tepelerinden gecerken hayvanlar ve arabalar kuma saplanip ilerlemeyi engelliyor.",
                new[]
                {
                    new ChallengeOption("Yukleri Bosalt ve Itele", "Tum yuku indir, arabalari kuma batmaktan kurtarip yeniden yukle.", -15, -5, 0),
                    new ChallengeOption("Agir Esyalari Gom", "Kuma batan yukleri cikartmakla ugrasma, geride birakip yola devam et.", 5, 0, -15),
                    new ChallengeOption("Geceyi Bekle", "Gunduz sicaginda ugrasma, kumun soguyup sertlestigi gece vaktini bekle.", -10, 5, -5)
                })
        };
    }
}
