using UnityEngine;

public class ResourceDrainManager : MonoBehaviour
{
    [Header("Current Climate")]
    public ClimateType currentClimate = ClimateType.Iliman;

    [Header("Resources")]
    [Range(0, 100)] public int health = 100;
    [Range(0, 100)] public int food = 100;
    [Range(0, 100)] public int stamina = 100;
    [Range(0, 100)] public int money = 100;

    private float healthTimer;
    private float foodTimer;
    private float staminaTimer;

    private void Start()
    {
        SyncResourcesToGameManager();
        SyncClimateToGameManager();
    }

    private void Update()
    {
        healthTimer += Time.deltaTime;
        foodTimer += Time.deltaTime;
        staminaTimer += Time.deltaTime;

        if (foodTimer >= GetFoodInterval())
        {
            foodTimer = 0f;
            DrainFood();
        }

        if (staminaTimer >= GetStaminaInterval())
        {
            staminaTimer = 0f;
            DrainStamina();
        }

        if (healthTimer >= GetEffectiveHealthInterval())
        {
            healthTimer = 0f;
            DrainHealth();
        }
    }

    public void SetClimate(ClimateType newClimate)
    {
        currentClimate = newClimate;
        healthTimer = 0f;
        foodTimer = 0f;
        staminaTimer = 0f;

        SyncClimateToGameManager();
    }

    public void ApplyChallengeEffects(int healthDelta, int foodDelta)
    {
        health = Mathf.Clamp(health + healthDelta, 0, 100);
        food = Mathf.Clamp(food + foodDelta, 0, 100);
        SyncResourcesToGameManager();
    }

    public void ResetResources()
    {
        health = 100;
        food = 100;
        stamina = 100;
        money = 100;
        currentClimate = ClimateType.Iliman;
        healthTimer = 0f;
        foodTimer = 0f;
        staminaTimer = 0f;

        SyncResourcesToGameManager();
        SyncClimateToGameManager();
    }

    public float GetBaseHealthInterval()
    {
        switch (currentClimate)
        {
            case ClimateType.Col:
                return 4f;
            case ClimateType.Tundra:
                return 3.5f;
            case ClimateType.Iliman:
            default:
                return 5f;
        }
    }

    public float GetFoodInterval()
    {
        switch (currentClimate)
        {
            case ClimateType.Col:
                return 1.5f;
            case ClimateType.Tundra:
                return 2.5f;
            case ClimateType.Iliman:
            default:
                return 3f;
        }
    }

    public float GetStaminaInterval()
    {
        switch (currentClimate)
        {
            case ClimateType.Col:
                return 2.5f;
            case ClimateType.Tundra:
                return 1.5f;
            case ClimateType.Iliman:
            default:
                return 4f;
        }
    }

    public float GetEffectiveHealthInterval()
    {
        if (food <= 0 || stamina <= 0)
            return 2f;

        float effectiveHealthInterval = GetBaseHealthInterval() - GetFoodPenalty() - GetStaminaPenalty();
        return Mathf.Max(effectiveHealthInterval, 2f);
    }

    public int GetFoodPenalty()
    {
        if (food >= 60)
            return 0;

        if (food >= 30)
            return 2;

        if (food >= 10)
            return 4;

        if (food > 0)
            return 6;

        return 0;
    }

    public int GetStaminaPenalty()
    {
        if (stamina >= 60)
            return 0;

        if (stamina >= 30)
            return 2;

        if (stamina >= 10)
            return 4;

        if (stamina > 0)
            return 6;

        return 0;
    }

    public void DrainHealth()
    {
        int oldHealth = health;
        health = Mathf.Clamp(health - 1, 0, 100);

        if (health != oldHealth)
        {
            SyncResourcesToGameManager();
            Debug.Log("Health azaldı: " + health);
        }
    }

    public void DrainFood()
    {
        int oldFood = food;
        food = Mathf.Clamp(food - 1, 0, 100);

        if (food != oldFood)
        {
            SyncResourcesToGameManager();
            Debug.Log("Food azaldı: " + food);
        }
    }

    public void DrainStamina()
    {
        int oldStamina = stamina;
        stamina = Mathf.Clamp(stamina - 1, 0, 100);

        if (stamina != oldStamina)
        {
            SyncResourcesToGameManager();
            Debug.Log("Stamina azaldı: " + stamina);
        }
    }

    private void SyncResourcesToGameManager()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.Health = health;
        GameManager.Instance.FoodSupplies = food;
        GameManager.Instance.Durability = stamina;
    }

    private void SyncClimateToGameManager()
    {
        if (GameManager.Instance == null)
            return;

        switch (currentClimate)
        {
            case ClimateType.Tundra:
                GameManager.Instance.CurrentClimate = GameManager.Climate.Tundra;
                break;
            case ClimateType.Col:
                GameManager.Instance.CurrentClimate = GameManager.Climate.Col;
                break;
            case ClimateType.Iliman:
            default:
                GameManager.Instance.CurrentClimate = GameManager.Climate.Iliman;
                break;
        }
    }
}
