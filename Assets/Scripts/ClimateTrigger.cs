using UnityEngine;

public class ClimateTrigger : MonoBehaviour
{
    public ClimateSpawner climateSpawner;

    private bool hasTriggered;

    private void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TrySpawnClimate(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrySpawnClimate(other.gameObject);
    }

    private void TrySpawnClimate(GameObject other)
    {
        if (hasTriggered || !other.CompareTag("Player"))
            return;

        if (climateSpawner == null)
            climateSpawner = FindFirstObjectByType<ClimateSpawner>();

        if (climateSpawner == null)
        {
            Debug.LogWarning("ClimateTrigger could not find a ClimateSpawner.");
            return;
        }

        hasTriggered = true;
        climateSpawner.SpawnRandomClimate();
    }
}
