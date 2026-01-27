using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FloorPlannerManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject plannerPanel;
    public Button floorButton;
    public Button wallButton;
    public Button doorButton;
    public Button windowButton;
    public Button roomButton;
    public Button finishButton;
    public InputField widthInput;
    public InputField lengthInput;
    public Text statusText;
    
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject windowPrefab;
    
    [Header("Materials")]
    public Material floorMaterial;
    public Material wallMaterial;
    public Material highlightMaterial;
    
    [Header("Settings")]
    public float gridSize = 1f;
    public float wallHeight = 2.5f;
    public float wallThickness = 0.2f;
    
    // Режимы строительства
    private enum BuildMode { None, Floor, Wall, Door, Window, Room }
    private BuildMode currentMode = BuildMode.None;
    
    // Текущие объекты
    private GameObject currentFloor;
    private List<GameObject> walls = new List<GameObject>();
    private List<GameObject> doors = new List<GameObject>();
    private List<GameObject> windows = new List<GameObject>();
    
    // Временные объекты для предпросмотра
    private GameObject previewObject;
    private Vector3 startPoint;
    private bool isDrawing = false;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Настройка кнопок
        if (floorButton != null) floorButton.onClick.AddListener(() => SetMode(BuildMode.Floor));
        if (wallButton != null) wallButton.onClick.AddListener(() => SetMode(BuildMode.Wall));
        if (doorButton != null) doorButton.onClick.AddListener(() => SetMode(BuildMode.Door));
        if (windowButton != null) windowButton.onClick.AddListener(() => SetMode(BuildMode.Window));
        if (roomButton != null) roomButton.onClick.AddListener(() => SetMode(BuildMode.Room));
        if (finishButton != null) finishButton.onClick.AddListener(FinishPlanning);
        
        // Скрываем панель при старте
        if (plannerPanel != null) plannerPanel.SetActive(false);
        
        UpdateStatus("Готов к работе");
    }
    
    void Update()
    {
        if (currentMode == BuildMode.None) return;
        
        // Получаем позицию на полу
        Vector3 floorPoint = GetFloorPoint();
        
        // Режим пола (один клик)
        if (currentMode == BuildMode.Floor)
        {
            HandleFloorMode(floorPoint);
        }
        // Режим стены/двери/окна (два клика)
        else if (currentMode == BuildMode.Wall || currentMode == BuildMode.Door || currentMode == BuildMode.Window)
        {
            HandleWallMode(floorPoint);
        }
        // Режим комнаты (автоматическое создание стен)
        else if (currentMode == BuildMode.Room)
        {
            HandleRoomMode(floorPoint);
        }
        
        // Отмена по Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCurrentAction();
        }
    }
    
    Vector3 GetFloorPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Привязка к сетке
            Vector3 snappedPoint = new Vector3(
                Mathf.Round(hit.point.x / gridSize) * gridSize,
                0f,
                Mathf.Round(hit.point.z / gridSize) * gridSize
            );
            
            return snappedPoint;
        }
        
        return Vector3.zero;
    }
    
    void HandleFloorMode(Vector3 point)
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Создаем пол по введенным размерам
            CreateFloorFromInput();
        }
    }
    
    void HandleWallMode(Vector3 point)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isDrawing)
            {
                // Первый клик - начало стены
                startPoint = point;
                isDrawing = true;
                
                // Создаем превью
                CreatePreviewObject(point);
                
                UpdateStatus($"Начало стены в {point}");
            }
            else
            {
                // Второй клик - завершение стены
                CreateWall(startPoint, point);
                isDrawing = false;
                
                // Удаляем превью
                Destroy(previewObject);
                previewObject = null;
                
                UpdateStatus("Стена создана");
            }
        }
        
        // Обновляем превью при движении мыши
        if (isDrawing && previewObject != null)
        {
            UpdatePreview(startPoint, point);
        }
    }
    
    void HandleRoomMode(Vector3 point)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isDrawing)
            {
                startPoint = point;
                isDrawing = true;
                CreatePreviewObject(point);
                UpdateStatus($"Начало комнаты в {point}");
            }
            else
            {
                // Создаем комнату (пол + 4 стены)
                CreateRoom(startPoint, point);
                isDrawing = false;
                Destroy(previewObject);
                previewObject = null;
                UpdateStatus("Комната создана");
            }
        }
        
        if (isDrawing && previewObject != null)
        {
            UpdatePreview(startPoint, point);
        }
    }
    
    void CreateFloorFromInput()
    {
        if (currentFloor != null)
        {
            Destroy(currentFloor);
        }
        
        float width = 10f;
        float length = 10f;
        
        // Парсим ввод пользователя
        if (!string.IsNullOrEmpty(widthInput.text))
            float.TryParse(widthInput.text, out width);
        
        if (!string.IsNullOrEmpty(lengthInput.text))
            float.TryParse(lengthInput.text, out length);
        
        // Создаем пол
        currentFloor = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity);
        currentFloor.transform.localScale = new Vector3(width, 0.1f, length);
        
        if (floorMaterial != null)
        {
            currentFloor.GetComponent<Renderer>().material = floorMaterial;
        }
        
        // Создаем сетку на полу
        CreateFloorGrid(width, length);
        
        UpdateStatus($"Создан пол {width}x{length}м");
    }
    
    void CreateFloorGrid(float width, float length)
    {
        GameObject grid = new GameObject("FloorGrid");
        grid.transform.SetParent(currentFloor.transform);
        
        // Линии по X
        for (float x = -width/2; x <= width/2; x += gridSize)
        {
            CreateGridLine(
                new Vector3(x, 0.02f, -length/2),
                new Vector3(x, 0.02f, length/2),
                Color.gray,
                grid.transform
            );
        }
        
        // Линии по Z
        for (float z = -length/2; z <= length/2; z += gridSize)
        {
            CreateGridLine(
                new Vector3(-width/2, 0.02f, z),
                new Vector3(width/2, 0.02f, z),
                Color.gray,
                grid.transform
            );
        }
    }
    
    void CreateGridLine(Vector3 start, Vector3 end, Color color, Transform parent)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(parent);
        
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPositions(new Vector3[] { start, end });
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = color;
    }
    
    void CreateWall(Vector3 start, Vector3 end)
    {
        // Вычисляем середину и длину
        Vector3 middle = (start + end) / 2;
        float length = Vector3.Distance(start, end);
        
        // Вычисляем угол поворота
        Vector3 direction = end - start;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        GameObject wall = Instantiate(wallPrefab, middle, Quaternion.Euler(0, angle, 0));
        wall.transform.localScale = new Vector3(wallThickness, wallHeight, length);
        
        if (wallMaterial != null)
        {
            wall.GetComponent<Renderer>().material = wallMaterial;
        }
        
        walls.Add(wall);
        
        // Автоматически размещаем на полу
        wall.transform.position = new Vector3(wall.transform.position.x, wallHeight/2, wall.transform.position.z);
    }
    
    void CreateRoom(Vector3 corner1, Vector3 corner2)
    {
        // Определяем координаты углов
        float minX = Mathf.Min(corner1.x, corner2.x);
        float maxX = Mathf.Max(corner1.x, corner2.x);
        float minZ = Mathf.Min(corner1.z, corner2.z);
        float maxZ = Mathf.Max(corner1.z, corner2.z);
        
        float width = maxX - minX;
        float length = maxZ - minZ;
        Vector3 center = new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2);
        
        // Создаем пол комнаты
        GameObject roomFloor = Instantiate(floorPrefab, center, Quaternion.identity);
        roomFloor.transform.localScale = new Vector3(width, 0.1f, length);
        roomFloor.name = "RoomFloor";
        
        // Создаем 4 стены
        CreateWall(new Vector3(minX, 0, minZ), new Vector3(maxX, 0, minZ)); // Южная
        CreateWall(new Vector3(maxX, 0, minZ), new Vector3(maxX, 0, maxZ)); // Восточная
        CreateWall(new Vector3(maxX, 0, maxZ), new Vector3(minX, 0, maxZ)); // Северная
        CreateWall(new Vector3(minX, 0, maxZ), new Vector3(minX, 0, minZ)); // Западная
        
        UpdateStatus($"Создана комната {width:F1}x{length:F1}м");
    }
    
    void CreatePreviewObject(Vector3 position)
    {
        if (currentMode == BuildMode.Wall)
        {
            previewObject = Instantiate(wallPrefab, position, Quaternion.identity);
            previewObject.transform.localScale = new Vector3(wallThickness, wallHeight, 1f);
        }
        else if (currentMode == BuildMode.Room)
        {
            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.transform.position = position;
            previewObject.transform.localScale = Vector3.one;
        }
        
        if (previewObject != null)
        {
            Renderer renderer = previewObject.GetComponent<Renderer>();
            if (renderer != null && highlightMaterial != null)
            {
                renderer.material = highlightMaterial;
            }
        }
    }
    
    void UpdatePreview(Vector3 start, Vector3 end)
    {
        if (previewObject == null) return;
        
        if (currentMode == BuildMode.Wall)
        {
            Vector3 middle = (start + end) / 2;
            float length = Vector3.Distance(start, end);
            
            Vector3 direction = end - start;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            
            previewObject.transform.position = middle;
            previewObject.transform.rotation = Quaternion.Euler(0, angle, 0);
            previewObject.transform.localScale = new Vector3(wallThickness, wallHeight, length);
        }
        else if (currentMode == BuildMode.Room)
        {
            float width = Mathf.Abs(end.x - start.x);
            float length = Mathf.Abs(end.z - start.z);
            Vector3 center = (start + end) / 2;
            
            previewObject.transform.position = center;
            previewObject.transform.localScale = new Vector3(width, 0.1f, length);
        }
    }
    
    void SetMode(BuildMode mode)
    {
        currentMode = mode;
        CancelCurrentAction();
        
        string modeName = "";
        switch (mode)
        {
            case BuildMode.Floor: modeName = "Пол"; break;
            case BuildMode.Wall: modeName = "Стена"; break;
            case BuildMode.Door: modeName = "Дверь"; break;
            case BuildMode.Window: modeName = "Окно"; break;
            case BuildMode.Room: modeName = "Комната"; break;
        }
        
        UpdateStatus($"Режим: {modeName}");
    }
    
    void CancelCurrentAction()
    {
        isDrawing = false;
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }
    
    void FinishPlanning()
    {
        // Объединяем все стены в один объект
        GameObject wallsParent = new GameObject("Walls");
        foreach (GameObject wall in walls)
        {
            wall.transform.SetParent(wallsParent.transform);
        }
        
        // Объединяем все двери
        GameObject doorsParent = new GameObject("Doors");
        foreach (GameObject door in doors)
        {
            door.transform.SetParent(doorsParent.transform);
        }
        
        // Объединяем все окна
        GameObject windowsParent = new GameObject("Windows");
        foreach (GameObject window in windows)
        {
            window.transform.SetParent(windowsParent.transform);
        }
        
        UpdateStatus("Планировка завершена");
        currentMode = BuildMode.None;
    }
    
    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }
    
    // Публичные методы для UI
    public void OpenPlanner()
    {
        if (plannerPanel != null)
        {
            plannerPanel.SetActive(true);
            UpdateStatus("Планировщик открыт");
        }
    }
    
    public void ClosePlanner()
    {
        if (plannerPanel != null)
        {
            plannerPanel.SetActive(false);
            UpdateStatus("Планировщик закрыт");
        }
    }
    
    public void ClearAll()
    {
        if (currentFloor != null) Destroy(currentFloor);
        foreach (GameObject wall in walls) Destroy(wall);
        foreach (GameObject door in doors) Destroy(door);
        foreach (GameObject window in windows) Destroy(window);
        
        walls.Clear();
        doors.Clear();
        windows.Clear();
        
        UpdateStatus("Вся планировка очищена");
    }
}