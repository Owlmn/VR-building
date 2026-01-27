using UnityEngine;
using UnityEngine.UI;

public class PlannerUIController : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject plannerUI;
    public Button startButton;
    public Button closeButton;
    public Button saveButton;
    public Button loadButton;
    
    [Header("Room Info")]
    public Text roomInfoText;
    public InputField roomNameInput;
    
    [Header("Furniture Buttons")]
    public Button sofaButton;
    public Button tableButton;
    public Button chairButton;
    public Button bedButton;
    
    private FloorPlannerManager planner;
    private RoomCalculator calculator;
    private FurniturePlacer placer;
    
    void Start()
    {
        // Получаем компоненты
        planner = FindObjectOfType<FloorPlannerManager>();
        calculator = FindObjectOfType<RoomCalculator>();
        placer = FindObjectOfType<FurniturePlacer>();
        
        // Настройка кнопок
        if (startButton != null) startButton.onClick.AddListener(StartPlanning);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePlanning);
        if (saveButton != null) saveButton.onClick.AddListener(SaveLayout);
        if (loadButton != null) loadButton.onClick.AddListener(LoadLayout);
        
        if (sofaButton != null) sofaButton.onClick.AddListener(() => PlaceSofa());
        if (tableButton != null) tableButton.onClick.AddListener(() => PlaceTable());
        if (chairButton != null) chairButton.onClick.AddListener(() => PlaceChair());
        if (bedButton != null) bedButton.onClick.AddListener(() => PlaceBed());
        
        // Скрываем UI при старте
        if (plannerUI != null) plannerUI.SetActive(false);
    }
    
    void Update()
    {
        // Обновляем информацию о комнате
        UpdateRoomInfo();
    }
    
    void StartPlanning()
    {
        if (planner != null)
        {
            planner.OpenPlanner();
        }
        
        if (plannerUI != null)
        {
            plannerUI.SetActive(true);
        }
    }
    
    void ClosePlanning()
    {
        if (planner != null)
        {
            planner.ClosePlanner();
        }
        
        if (plannerUI != null)
        {
            plannerUI.SetActive(false);
        }
    }
    
    void SaveLayout()
    {
        // Сохранение планировки
        Debug.Log("Планировка сохранена");
        // Здесь можно добавить систему сохранения
    }
    
    void LoadLayout()
    {
        // Загрузка планировки
        Debug.Log("Планировка загружена");
        // Здесь можно добавить систему загрузки
    }
    
    void UpdateRoomInfo()
    {
        if (roomInfoText == null || calculator == null) return;
        
        if (calculator.rooms.Count > 0)
        {
            RoomCalculator.Room lastRoom = calculator.rooms[calculator.rooms.Count - 1];
            roomInfoText.text = $"Комната: {lastRoom.name}\n" +
                               $"Площадь: {lastRoom.area:F2}м²\n" +
                               $"Периметр: {lastRoom.perimeter:F2}м";
        }
        else
        {
            roomInfoText.text = "Нет созданных комнат";
        }
    }
    
    // Методы для расстановки мебели
    void PlaceSofa()
    {
        if (placer != null && placer.sofas.Length > 0)
        {
            placer.PlaceFurnitureInRoom(placer.sofas[0]);
        }
    }
    
    void PlaceTable()
    {
        if (placer != null && placer.tables.Length > 0)
        {
            placer.PlaceFurnitureInRoom(placer.tables[0]);
        }
    }
    
    void PlaceChair()
    {
        if (placer != null && placer.chairs.Length > 0)
        {
            placer.PlaceFurnitureInRoom(placer.chairs[0]);
        }
    }
    
    void PlaceBed()
    {
        if (placer != null && placer.beds.Length > 0)
        {
            placer.PlaceFurnitureInRoom(placer.beds[0]);
        }
    }
    
    public void OnRoomNameChanged()
    {
        if (calculator != null && calculator.rooms.Count > 0 && roomNameInput != null)
        {
            calculator.rooms[calculator.rooms.Count - 1].name = roomNameInput.text;
        }
    }
}