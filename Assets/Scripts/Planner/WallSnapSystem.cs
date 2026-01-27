using UnityEngine;
using System.Collections.Generic;

public class WallSnapSystem : MonoBehaviour
{
    [Header("Snap Settings")]
    public float snapDistance = 0.5f;
    public LayerMask wallLayer;
    
    private List<GameObject> snapPoints = new List<GameObject>();
    
    void Start()
    {
        FindAllSnapPoints();
    }
    
    void FindAllSnapPoints()
    {
        // Находим все существующие стены
        GameObject[] existingWalls = GameObject.FindGameObjectsWithTag("Wall");
        
        foreach (GameObject wall in existingWalls)
        {
            // Добавляем концы стены как точки привязки
            AddSnapPoint(wall.transform.position - wall.transform.forward * wall.transform.localScale.z / 2);
            AddSnapPoint(wall.transform.position + wall.transform.forward * wall.transform.localScale.z / 2);
        }
    }
    
    void AddSnapPoint(Vector3 position)
    {
        GameObject point = new GameObject("SnapPoint");
        point.transform.position = position;
        point.transform.SetParent(transform);
        snapPoints.Add(point);
    }
    
    public Vector3 GetSnappedPosition(Vector3 position)
    {
        foreach (GameObject point in snapPoints)
        {
            if (Vector3.Distance(position, point.transform.position) < snapDistance)
            {
                return point.transform.position;
            }
        }
        return position;
    }
    
    public void AddWallSnapPoints(GameObject wall)
    {
        if (wall == null) return;
        
        // Добавляем точки привязки для новой стены
        AddSnapPoint(wall.transform.position - wall.transform.forward * wall.transform.localScale.z / 2);
        AddSnapPoint(wall.transform.position + wall.transform.forward * wall.transform.localScale.z / 2);
    }
    
    public void ClearSnapPoints()
    {
        foreach (GameObject point in snapPoints)
        {
            Destroy(point);
        }
        snapPoints.Clear();
    }
}