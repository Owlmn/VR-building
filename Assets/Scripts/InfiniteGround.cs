using UnityEngine;

public class InfiniteGround : MonoBehaviour
{
    [Header("Настройки площадки")]
    public Transform player;
    public float groundHeight = -1f;
    public float updateInterval = 0.1f;
    
    private Vector3 lastPlayerPosition;
    private float timer;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("XR Origin").transform;
        }
        
        if (player != null)
        {
            UpdateGroundPosition();
            lastPlayerPosition = player.position;
        }
        else
        {
            Debug.LogError("XR Origin не найден! Перетащите XR Origin в поле Player.");
        }
    }

    void Update()
    {
        if (player == null) return;
        
        timer += Time.deltaTime;
        
        if (timer >= updateInterval && Vector3.Distance(player.position, lastPlayerPosition) > 0.1f)
        {
            UpdateGroundPosition();
            lastPlayerPosition = player.position;
            timer = 0f;
        }
    }

    void UpdateGroundPosition()
    {
        Vector3 playerPos = player.position;
        transform.position = new Vector3(playerPos.x, groundHeight, playerPos.z);
    }
}