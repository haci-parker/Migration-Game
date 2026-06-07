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
    }

    public void ApplyChallengeEffects(int healthDelta, int foodDelta)
    {
        health = Mathf.Clamp(health + healthDelta, 0, 100);
        food = Mathf.Clamp(food + foodDelta, 0, 100);
    }

    public float GetBaseHealthInterval()
    {
        switch (currentClimate)
        {
            case ClimateType.Col:
                return 14f;
            case ClimateType.Tundra:
                return 12f;
            case ClimateType.Iliman:
            default:
                return 18f;
        }
    }

    public float GetFoodInterval()
    {
        switch (currentClimate)
        {
            case ClimateType.Col:
                return 5f;
            case ClimateType.Tundra:
                return 8f;
            case ClimateType.Iliman:
            default:
                return 10f;
        }
    }

    public float GetStaminaInterval()
    {
        switch (currentClimate)
        {
            case ClimateType.Col:
                return 8f;
            case ClimateType.Tundra:
                return 5f;
            case ClimateType.Iliman:
            default:
                return 12f;
        }
    }

    public float GetEffectiveHealthInterval()
    {
        if (food <= 0 || stamina <= 0)
            return 3f;

        float effectiveHealthInterval = GetBaseHealthInterval() - GetFoodPenalty() - GetStaminaPenalty();
        return Mathf.Max(effectiveHealthInterval, 3f);
    }

    public int GetFoodPenalty()
    {
        if (food >= 50)
            return 0;

        if (food >= 20)
            return 2;

        if (food > 0)
            return 4;

        return 0;
    }

    public int GetStaminaPenalty()
    {
        if (stamina >= 50)
            return 0;

        if (stamina >= 20)
            return 2;

        if (stamina > 0)
            return 4;

        return 0;
    }

    public void DrainHealth()
    {
        int oldHealth = health;
        health = Mathf.Clamp(health - 1, 0, 100);

        if (health != oldHealth)
            Debug.Log("Health azaldı: " + health);
    }

    public void DrainFood()
    {
        int oldFood = food;
        food = Mathf.Clamp(food - 1, 0, 100);

        if (food != oldFood)
            Debug.Log("Food azaldı: " + food);
    }

    public void DrainStamina()
    {
        int oldStamina = stamina;
        stamina = Mathf.Clamp(stamina - 1, 0, 100);

        if (stamina != oldStamina)
            Debug.Log("Stamina azaldı: " + stamina);
    }
}
