using UnityEngine;
using System.Collections.Generic;

public class RoomCalculator : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public string name;
        public List<Vector3> corners = new List<Vector3>();
        public float area;
        public float perimeter;
        
        public Room(string roomName)
        {
            name = roomName;
        }
        
        public void CalculateArea()
        {
            if (corners.Count < 3) return;
            
            area = 0f;
            int j = corners.Count - 1;
            
            for (int i = 0; i < corners.Count; i++)
            {
                area += (corners[j].x + corners[i].x) * (corners[j].z - corners[i].z);
                j = i;
            }
            
            area = Mathf.Abs(area / 2f);
        }
        
        public void CalculatePerimeter()
        {
            perimeter = 0f;
            for (int i = 0; i < corners.Count; i++)
            {
                int next = (i + 1) % corners.Count;
                perimeter += Vector3.Distance(corners[i], corners[next]);
            }
        }
    }
    
    public List<Room> rooms = new List<Room>();
    
    public Room CreateRoomFromWalls(List<GameObject> wallObjects)
    {
        if (wallObjects.Count < 3) return null;
        
        Room newRoom = new Room($"Room_{rooms.Count + 1}");
        
        // Собираем углы комнаты из стен
        foreach (GameObject wall in wallObjects)
        {
            Vector3 start = wall.transform.position - wall.transform.forward * wall.transform.localScale.z / 2;
            Vector3 end = wall.transform.position + wall.transform.forward * wall.transform.localScale.z / 2;
            
            if (!newRoom.corners.Contains(start))
                newRoom.corners.Add(start);
            
            if (!newRoom.corners.Contains(end))
                newRoom.corners.Add(end);
        }
        
        // Сортируем углы по часовой стрелке
        SortCornersClockwise(newRoom.corners);
        
        // Рассчитываем площадь и периметр
        newRoom.CalculateArea();
        newRoom.CalculatePerimeter();
        
        rooms.Add(newRoom);
        
        Debug.Log($"Создана комната: {newRoom.name}, Площадь: {newRoom.area:F2}м², Периметр: {newRoom.perimeter:F2}м");
        
        return newRoom;
    }
    
    void SortCornersClockwise(List<Vector3> corners)
    {
        if (corners.Count == 0) return;
        
        // Находим центр
        Vector3 center = Vector3.zero;
        foreach (Vector3 corner in corners)
        {
            center += corner;
        }
        center /= corners.Count;
        
        // Сортируем по углу
        corners.Sort((a, b) => {
            float angleA = Mathf.Atan2(a.z - center.z, a.x - center.x);
            float angleB = Mathf.Atan2(b.z - center.z, b.x - center.x);
            return angleA.CompareTo(angleB);
        });
    }
    
    public float GetTotalArea()
    {
        float total = 0f;
        foreach (Room room in rooms)
        {
            total += room.area;
        }
        return total;
    }
    
    public void ClearRooms()
    {
        rooms.Clear();
    }
}