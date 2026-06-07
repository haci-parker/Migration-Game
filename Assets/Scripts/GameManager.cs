using UnityEngine;
using TMPro;

/// <summary>
/// GameManager is a singleton that holds all core game values
/// and keeps the top UI panel in sync whenever a value changes.
/// Attach this script to an empty GameObject in your scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public const float DefaultSpeedKmh = 40f;

    // ──────────────────────────────────────────────
    //  Singleton – lets any script access GameManager.Instance
    // ──────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ──────────────────────────────────────────────
    //  Climate enum – possible climate types
    //  You can easily add more values here in the
    //  future (e.g. Kurak, Tropik) as the journey
    //  progresses through different regions.
    // ──────────────────────────────────────────────
    public enum Climate
    {
        Iliman,  // Temperate (Ilıman)
        Sert     // Harsh
    }

    // ──────────────────────────────────────────────
    //  UI References (assign these in the Inspector)
    //  Each one points to a TextMeshPro text element
    //  on the top UI panel.
    // ──────────────────────────────────────────────
    [Header("UI Text References")]
    [Tooltip("Displays the current health value.")]
    public TMP_Text healthText;

    [Tooltip("Displays the current food supplies value.")]
    public TMP_Text foodSuppliesText;

    [Tooltip("Displays the current durability value.")]
    public TMP_Text durabilityText;

    [Tooltip("Displays the current gold amount.")]
    public TMP_Text goldText;

    [Tooltip("Displays the current population count.")]
    public TMP_Text populationText;

    [Tooltip("Displays the current speed value.")]
    public TMP_Text speedText;

    [Tooltip("Displays the current climate.")]
    public TMP_Text climateText;

    [Tooltip("Displays the journey progress percentage.")]
    public TMP_Text journeyProgressText;

    // ──────────────────────────────────────────────
    //  Backing fields – the actual stored values.
    //  We keep these private so nothing can change
    //  them without going through the properties,
    //  which handle clamping and UI updates.
    // ──────────────────────────────────────────────
    private float _health           = 100f;
    private float _foodSupplies     = 100f;
    private float _durability       = 100f;
    private float _journeyProgress  = 0f;
    private int   _gold             = 0;
    private int   _population       = 10;
    private float _speed            = DefaultSpeedKmh;
    private Climate _climate        = Climate.Iliman;

    // ──────────────────────────────────────────────
    //  Public Properties
    //  • Percentage values (Health, FoodSupplies,
    //    Durability, JourneyProgress) are clamped
    //    between 0 and 100.
    //  • Every setter updates the UI automatically.
    // ──────────────────────────────────────────────

    /// <summary>Health (0–100). Clamped automatically.</summary>
    public float Health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0f, 100f);
            UpdateHealthUI();
        }
    }

    /// <summary>Food supplies (0–100). Clamped automatically.</summary>
    public float FoodSupplies
    {
        get => _foodSupplies;
        set
        {
            _foodSupplies = Mathf.Clamp(value, 0f, 100f);
            UpdateFoodSuppliesUI();
        }
    }

    /// <summary>Durability (0–100). Clamped automatically.</summary>
    public float Durability
    {
        get => _durability;
        set
        {
            _durability = Mathf.Clamp(value, 0f, 100f);
            UpdateDurabilityUI();
        }
    }

    /// <summary>Journey progress (0–100). Clamped automatically.</summary>
    public float JourneyProgress
    {
        get => _journeyProgress;
        set
        {
            _journeyProgress = Mathf.Clamp(value, 0f, 100f);
            UpdateJourneyProgressUI();
        }
    }

    /// <summary>Gold amount. Can be any non-negative integer.</summary>
    public int Gold
    {
        get => _gold;
        set
        {
            _gold = Mathf.Max(value, 0);
            UpdateGoldUI();
        }
    }

    /// <summary>Population count. Minimum is 0.</summary>
    public int Population
    {
        get => _population;
        set
        {
            _population = Mathf.Max(value, 0);
            UpdatePopulationUI();
        }
    }

    /// <summary>Travel speed in km/h.</summary>
    public float Speed
    {
        get => _speed;
        set
        {
            _speed = Mathf.Max(value, 0f);
            UpdateSpeedUI();
        }
    }

    /// <summary>Current climate. Change this as the journey progresses.</summary>
    public Climate CurrentClimate
    {
        get => _climate;
        set
        {
            _climate = value;
            UpdateClimateUI();
        }
    }

    // ──────────────────────────────────────────────
    //  Unity Lifecycle
    // ──────────────────────────────────────────────

    private void Awake()
    {
        // --- Singleton setup ---
        // If an instance already exists and it's not this one, destroy
        // this duplicate so only one GameManager ever exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep the GameManager alive between scene loads.
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Show the starting values on the UI as soon as the game begins.
        UpdateAllUI();
    }

    // ──────────────────────────────────────────────
    //  Individual UI Update Methods
    //  Each method safely checks for a null reference
    //  so the game won't crash if a text field hasn't
    //  been assigned in the Inspector yet.
    // ──────────────────────────────────────────────

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Sağlık: " + Mathf.RoundToInt(_health) + "%";
    }

    private void UpdateFoodSuppliesUI()
    {
        if (foodSuppliesText != null)
            foodSuppliesText.text = "Erzak: " + Mathf.RoundToInt(_foodSupplies) + "%";
    }

    private void UpdateDurabilityUI()
    {
        if (durabilityText != null)
            durabilityText.text = "Direnç: " + Mathf.RoundToInt(_durability) + "%";
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = "Altın: " + _gold;
    }

    private void UpdatePopulationUI()
    {
        if (populationText != null)
            populationText.text = "Nüfus: " + _population;
    }

    private void UpdateSpeedUI()
    {
        if (speedText != null)
            speedText.text = "Hız: " + Mathf.RoundToInt(_speed) + " km/h";
    }

    private void UpdateClimateUI()
    {
        if (climateText != null)
        {
            // Display the climate name in Turkish
            string climateName = _climate == Climate.Iliman ? "Ilıman" : "Sert";
            climateText.text = "İklim: " + climateName;
        }
    }

    private void UpdateJourneyProgressUI()
    {
        if (journeyProgressText != null)
            journeyProgressText.text = "Yol: " + Mathf.RoundToInt(_journeyProgress) + "%";
    }

    /// <summary>
    /// Refreshes every UI element at once.
    /// Called once in Start() and can be called manually if needed.
    /// </summary>
    public void UpdateAllUI()
    {
        UpdateHealthUI();
        UpdateFoodSuppliesUI();
        UpdateDurabilityUI();
        UpdateGoldUI();
        UpdatePopulationUI();
        UpdateSpeedUI();
        UpdateClimateUI();
        UpdateJourneyProgressUI();
    }
}
