using UnityEngine;

public class ClanMovement : MonoBehaviour
{

    [SerializeField] private float clanSpeed = 2f;
    [SerializeField] private float journeyStartX = -25f;
    [SerializeField] private float journeyDistance = 4800f;
    [SerializeField] private TopHudController topHudController;

    private bool hasWon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateJourneyProgress();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasWon)
            return;

        float speedMultiplier = 1f;
        if (GameManager.Instance != null)
            speedMultiplier = Mathf.Max(GameManager.Instance.Speed / GameManager.DefaultSpeedKmh, 0f);

        Vector3 movement = Vector3.right * clanSpeed * speedMultiplier * Time.deltaTime;
        transform.position += movement;
        UpdateJourneyProgress();
    }

    private void UpdateJourneyProgress()
    {
        if (journeyDistance <= 0f)
            return;

        float travelledDistance = transform.position.x - journeyStartX;
        float journeyPercent = Mathf.Clamp01(travelledDistance / journeyDistance) * 100f;

        if (GameManager.Instance != null)
            GameManager.Instance.JourneyProgress = journeyPercent;

        if (journeyPercent >= 100f && !hasWon)
        {
            hasWon = true;
            Debug.Log("Oyun kazanildi.");

            if (topHudController == null)
                topHudController = FindFirstObjectByType<TopHudController>();

            if (topHudController != null)
                topHudController.ShowVictoryScreen();
        }
    }
}
