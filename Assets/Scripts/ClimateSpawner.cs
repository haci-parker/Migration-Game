using UnityEngine;

public class ClimateSpawner : MonoBehaviour
{
    public GameObject defaultClimate;
    public string defaultClimateName = "iliman_objects";
    public GameObject ilimanPrefab;
    public GameObject tundraPrefab;
    public GameObject colPrefab;
    public float segmentDistance = 300f;
    public float fixedY = 1f;
    public float fixedZ;

    private float lastSpawnX;
    private GameObject previousClimate;
    private GameObject currentClimate;
    private bool initialized;
    private bool hasSpawnedClimate;

    private void Awake()
    {
        InitializeDefaultClimate();
    }

    public void SpawnRandomClimate()
    {
        InitializeDefaultClimate();

        int randomClimate = Random.Range(1, 4);
        GameObject selectedPrefab = null;
        string climateName = "";

        if (randomClimate == 1)
        {
            selectedPrefab = ilimanPrefab;
            climateName = "Ilıman iklim";
        }
        else if (randomClimate == 2)
        {
            selectedPrefab = tundraPrefab;
            climateName = "Tundra iklimi";
        }
        else if (randomClimate == 3)
        {
            selectedPrefab = colPrefab;
            climateName = "Çöl iklimi";
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning("ClimateSpawner selected an empty prefab slot: " + randomClimate);
            return;
        }

        Vector3 spawnPosition = new Vector3(lastSpawnX + segmentDistance, fixedY, fixedZ);
        GameObject spawnedClimate = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        lastSpawnX = spawnPosition.x;
        Debug.Log(climateName + " oluşturuldu.");

        SaveClimateHistory(spawnedClimate);
    }

    private void InitializeDefaultClimate()
    {
        if (initialized)
            return;

        initialized = true;
        previousClimate = defaultClimate;

        if (previousClimate == null && !string.IsNullOrEmpty(defaultClimateName))
            previousClimate = GameObject.Find(defaultClimateName);

        if (previousClimate != null)
            lastSpawnX = previousClimate.transform.position.x;
    }

    private void SaveClimateHistory(GameObject spawnedClimate)
    {
        if (!hasSpawnedClimate)
        {
            currentClimate = spawnedClimate;
            hasSpawnedClimate = true;
            return;
        }

        if (previousClimate != null)
        {
            Debug.Log(previousClimate.name + " yok edildi.");
            Destroy(previousClimate);
        }

        previousClimate = currentClimate;
        currentClimate = spawnedClimate;
    }
}
