using UnityEngine;

public class ClanClimateDetector : MonoBehaviour
{
    public ClimateType currentClimate;
    public float raycastDistance = 5f;
    public LayerMask groundLayer = ~0;
    public ResourceDrainManager resourceDrainManager;
    public ClimateSpawner climateSpawner;

    private bool hasDetectedClimate;

    private void Update()
    {
        DetectClimateBelow();
    }

    private void DetectClimateBelow()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.down;

        Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.cyan);

        if (!Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, raycastDistance, groundLayer))
            return;

        ClimateGround climateGround = hit.collider.GetComponent<ClimateGround>();
        if (climateGround == null)
            climateGround = hit.collider.GetComponentInParent<ClimateGround>();

        if (climateGround == null)
            return;

        ClimateType detectedClimate = climateGround.climateType;
        if (hasDetectedClimate && detectedClimate == currentClimate)
            return;

        currentClimate = detectedClimate;
        hasDetectedClimate = true;

        Debug.Log("İklim değişti: " + currentClimate);

        if (resourceDrainManager == null)
            resourceDrainManager = FindFirstObjectByType<ResourceDrainManager>();

        if (resourceDrainManager != null)
            resourceDrainManager.SetClimate(currentClimate);

        if (ChallengeManager.Instance == null)
            new GameObject("ChallengeManager").AddComponent<ChallengeManager>();

        if (climateSpawner == null)
            climateSpawner = FindFirstObjectByType<ClimateSpawner>();

        float segmentDistance = climateSpawner != null ? climateSpawner.segmentDistance : ChallengeManager.Instance.fallbackSegmentDistance;
        ChallengeManager.Instance.ScheduleChallenge(currentClimate, transform.position.x, segmentDistance);
    }
}
