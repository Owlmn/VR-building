using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FurnitureManager : MonoBehaviour
{
    [Header("Top Category Buttons")]
    [SerializeField] private Button furnitureButton; // кнопка мебели
    [SerializeField] private Button decorButton; // кнопка декора
    [SerializeField] private Button lightingButton;

    [Header("Scroll View")]
    [SerializeField] private GameObject scrollView; // панель скролла (для сокрытия/показа)

    [Header("Type Panels (manual buttons)")]
    [SerializeField] private GameObject typeContent; // панель куда добавляются превью
    [SerializeField] private GameObject furnitureCategory; // панель с категориями мебели (заранее готовые)
    [SerializeField] private GameObject decorCategory;
    [SerializeField] private GameObject lightingCategory;

    [Header("Prefabs Content (dynamic)")]
    [SerializeField] private Transform prefabsContent; 
    [SerializeField] private GameObject itemButtonPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnDistance = 10f;

    [Header("Placement Ghost")]
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private LayerMask blockingMask;
    
    [Header("Snapping")]
    [SerializeField] private LayerMask floorMask; // Слой пола
    [SerializeField] private bool snapToFloor = true;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private float maxRaycastDistance = 50f;

    private GameObject currentCategoryPanel;
    private List<GameObject> currentButtons = new();
    private FurnitureItem selectedItem;

    private GameObject ghostInstance;
    private FurniturePlacementGhost ghost;

    [System.Serializable]
    public class FurnitureItem
    {
        public string name;
        public GameObject prefab;
        public Sprite preview;
        public string path;
    }

    private void Start()
    {
        scrollView.SetActive(false);
        typeContent.SetActive(false);

        HideAllCategories();

        furnitureButton.onClick.AddListener(() => ShowCategory(furnitureCategory));
        decorButton.onClick.AddListener(() => ShowCategory(decorCategory));
        lightingButton.onClick.AddListener(() => ShowCategory(lightingCategory));
    }

    private void HideAllCategories()
    {
        furnitureCategory.SetActive(false);
        decorCategory.SetActive(false);
        lightingCategory.SetActive(false);
    }

    private void ShowCategory(GameObject categoryPanel)
    {   // Прячем категорию, если это повторное нажатие
        if (currentCategoryPanel == categoryPanel)
        {
            scrollView.SetActive(false);
            HideAllCategories();
            currentCategoryPanel = null;
            return;
        }

        // Иначе очищаем весь scrollview и показываем нужную панель
        scrollView.SetActive(true);
        typeContent.SetActive(false);
        ClearPrefabs();

        HideAllCategories();

        currentCategoryPanel = categoryPanel;
        categoryPanel.SetActive(true);
    }

    // Вызывается кнопками в categories панелях. Скрывает текущую панель и вызвает LoadItemsResources для динамической загрузки кнопок
    public void LoadType(string resourcesPath)
    {
        if (currentCategoryPanel != null)
            currentCategoryPanel.SetActive(false);

        typeContent.SetActive(true);
        ClearPrefabs();
        LoadItemsFromResources(resourcesPath);
    }

    // Получает путь к папке, в которой находятся префабы и превью для выбранного типа объектов.
    private void LoadItemsFromResources(string path)
    {
        // Получаем все префабы из папки
        GameObject[] prefabs = Resources.LoadAll<GameObject>(path);

        if (prefabs.Length == 0)
        {
            Debug.LogWarning($"NO PREFABS FOUND at Resources/{path}");
            return;
        }
        
        // Ищем превью для префабов
        foreach (var prefab in prefabs)
        {
            // Они лежат по тому же пути только в папке Previews
            string previewPath = $"{path.Replace("Prefabs", "Previews")}/{prefab.name}";
            Sprite preview = Resources.Load<Sprite>(previewPath);

            if (preview == null)
            {
                Debug.LogWarning($"Preview NOT found for {prefab.name}");
                continue;
            }

            CreateItemButton(prefab, preview, path);
        }
    }

    private void CreateItemButton(GameObject prefab, Sprite preview, string path)
    {
        // Внутрь prefabsContent добавляем заранее подготовленную кнопку itemButtonPrefab
        GameObject btnObj = Instantiate(itemButtonPrefab, prefabsContent);
        btnObj.SetActive(true);

        // Корректируем itemButtonPrefab в соответствии с preview и именем префаба
        btnObj.transform.Find("Icon").GetComponent<Image>().sprite = preview;
        btnObj.transform.Find("Name").GetComponent<Text>().text = prefab.name;

        FurnitureItem item = new()
        {
            name = prefab.name,
            prefab = prefab,
            preview = preview,
            path = path
        };

        btnObj.GetComponent<Button>()
              .onClick.AddListener(() => SelectItem(item, btnObj));

        currentButtons.Add(btnObj);
    }

    // Выбираем кнопку, подсвечиваем ее.
    private void SelectItem(FurnitureItem item, GameObject btn)
    {
        selectedItem = item;

        foreach (var b in currentButtons)
            b.GetComponent<Image>().color = Color.white;

        btn.GetComponent<Image>().color = new Color(0.3f, 0.6f, 1f, 0.8f);

        CreateGhost();
    }

    // Создание объекта ghost - предпоказа объекта.
    private void CreateGhost()
    {
        // Удаляем объект ghost, если он уже существует
        if (ghostInstance)
            Destroy(ghostInstance);

        ghostInstance = Instantiate(selectedItem.prefab);
        ghostInstance.layer = 0;
        // Добавляем в объект скрипт с логикой постановки
        ghost = ghostInstance.AddComponent<FurniturePlacementGhost>();

        // Настраиваем ghost
        ghost.head = Camera.main.transform;
        ghost.distance = spawnDistance;
        ghost.blockingMask = blockingMask;
        
        // Привязка к полу/мебели
        ghost.floorMask = floorMask;
        ghost.snapToFloor = snapToFloor;
        ghost.heightOffset = heightOffset;
        ghost.maxRaycastDistance = maxRaycastDistance;  

        // Применяем материал ghost
        foreach (var r in ghostInstance.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = new Material(ghostMaterial);

            r.materials = mats;
        }
    }

    private void ConfigureGhost(FurniturePlacementGhost g)
    {
        g.head = Camera.main.transform;
        g.distance = spawnDistance;
        g.blockingMask = blockingMask;
        g.floorMask = floorMask;
        g.snapToFloor = snapToFloor;
        g.heightOffset = heightOffset;
        g.maxRaycastDistance = maxRaycastDistance;
    }

    private void ApplyGhostMaterial(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = new Material(ghostMaterial);

            r.materials = mats;
        }
    }

    // Привязан к кнопке "Добавить"
    public void AddSelectedObject()
    {
        if (selectedItem == null || ghost == null || !ghost.CanPlace)
            return;

        Transform parent = GetOrCreateFurnitureParent();

        Instantiate(
            selectedItem.prefab,
            ghost.transform.position,
            ghost.transform.rotation,
            parent
        );

        Destroy(ghostInstance);
        ghost = null;
    }

    // Ложим объет в environments
    public Transform GetOrCreateFurnitureParent()
    {
        // Найти или создать Environments
        GameObject env = GameObject.Find("Environments");
        if (env == null)
        {
            env = new GameObject("Environments");
            Debug.Log("Created Environments root");
        }

        // Найти или создать Furniture как ребёнка Environments
        Transform furniture = env.transform.Find("Furniture");
        if (furniture == null)
        {
            GameObject f = new GameObject("Furniture");
            f.transform.SetParent(env.transform);
            furniture = f.transform;
            Debug.Log("Created Furniture container under Environments");
        }

        return furniture;
    }

    
    private void ClearPrefabs()
    {
        foreach (Transform t in prefabsContent)
            Destroy(t.gameObject);

        currentButtons.Clear();
        selectedItem = null;
    }
}



