using UnityEngine;
using System.Collections.Generic;

public class FurniturePlacer : MonoBehaviour
{
    [Header("Furniture Prefabs")]
    public GameObject[] sofas;
    public GameObject[] tables;
    public GameObject[] chairs;
    public GameObject[] beds;
    public GameObject[] cabinets;
    
    [Header("Placement Settings")]
    public float minDistanceFromWall = 0.5f;
    public bool autoArrange = false;
    
    private RoomCalculator.Room currentRoom;
    private List<GameObject> placedFurniture = new List<GameObject>();
    
    public void SetCurrentRoom(RoomCalculator.Room room)
    {
        currentRoom = room;
    }
    
    public void PlaceFurnitureInRoom(GameObject furniturePrefab)
    {
        if (currentRoom == null || currentRoom.corners.Count < 3 || furniturePrefab == null)
        {
            Debug.LogWarning("Нельзя разместить мебель: комната не выбрана");
            return;
        }
        
        // Находим центр комнаты
        Vector3 center = Vector3.zero;
        foreach (Vector3 corner in currentRoom.corners)
        {
            center += corner;
        }
        center /= currentRoom.corners.Count;
        center.y = 0;
        
        // Размещаем мебель
        GameObject furniture = Instantiate(furniturePrefab, center, Quaternion.identity);
        placedFurniture.Add(furniture);
        
        Debug.Log($"Мебель размещена в центре комнаты {currentRoom.name}");
    }
    
    public void AutoArrangeRoom()
    {
        if (currentRoom == null || currentRoom.corners.Count < 3) return;
        
        ClearFurnitureFromRoom();
        
        // Простой алгоритм авторасстановки
        Vector3 roomSize = GetRoomSize();
        
        // Размещаем диван у стены
        if (sofas.Length > 0)
        {
            PlaceAlongWall(sofas[0], 0);
        }
        
        // Размещаем стол в центре
        if (tables.Length > 0)
        {
            Vector3 center = GetRoomCenter();
            Instantiate(tables[0], center, Quaternion.identity);
        }
        
        // Размещаем стулья вокруг стола
        if (chairs.Length > 0)
        {
            PlaceChairsAroundTable();
        }
    }
    
    Vector3 GetRoomSize()
    {
        if (currentRoom.corners.Count < 2) return Vector3.zero;
        
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        
        foreach (Vector3 corner in currentRoom.corners)
        {
            if (corner.x < minX) minX = corner.x;
            if (corner.x > maxX) maxX = corner.x;
            if (corner.z < minZ) minZ = corner.z;
            if (corner.z > maxZ) maxZ = corner.z;
        }
        
        return new Vector3(maxX - minX, 0, maxZ - minZ);
    }
    
    Vector3 GetRoomCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 corner in currentRoom.corners)
        {
            center += corner;
        }
        return center / currentRoom.corners.Count;
    }
    
    void PlaceAlongWall(GameObject furniture, int wallIndex)
    {
        if (currentRoom.corners.Count == 0) return;
        
        int startIdx = wallIndex % currentRoom.corners.Count;
        int endIdx = (wallIndex + 1) % currentRoom.corners.Count;
        
        Vector3 wallCenter = (currentRoom.corners[startIdx] + currentRoom.corners[endIdx]) / 2;
        Vector3 wallDirection = (currentRoom.corners[endIdx] - currentRoom.corners[startIdx]).normalized;
        
        // Поворачиваем мебель лицом к комнате
        Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(wallDirection, Vector3.up));
        
        Instantiate(furniture, wallCenter, rotation);
    }
    
    void PlaceChairsAroundTable()
    {
        Vector3 tablePos = GetRoomCenter();
        float radius = 2f;
        int chairCount = 4;
        
        for (int i = 0; i < chairCount; i++)
        {
            float angle = i * (360f / chairCount);
            Vector3 chairPos = tablePos + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
            Quaternion chairRot = Quaternion.Euler(0, angle + 180, 0);
            
            Instantiate(chairs[0], chairPos, chairRot);
        }
    }
    
    public void ClearFurnitureFromRoom()
    {
        foreach (GameObject furniture in placedFurniture)
        {
            Destroy(furniture);
        }
        placedFurniture.Clear();
    }
}