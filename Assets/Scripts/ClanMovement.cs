using UnityEngine;

public class ClanMovement : MonoBehaviour
{

    [SerializeField] private float clanSpeed = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.right * clanSpeed * Time.deltaTime;
        transform.position += movement;
    }
}
