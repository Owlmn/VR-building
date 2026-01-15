using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FurnitureManager : MonoBehaviour
{
    [Header("Top Category Buttons")]
    [SerializeField] private Button furnitureButton;
    [SerializeField] private Button decorButton;
    [SerializeField] private Button lightingButton;

    [Header("Scroll View")]
    [SerializeField] private GameObject scrollView;

    [Header("Type Panels (manual buttons)")]
    [SerializeField] private GameObject typeContent;
    [SerializeField] private GameObject furnitureCategory;
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
    [SerializeField] private bool alignToFloorNormal = false;

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

        furnitureCategory.SetActive(false);
        decorCategory.SetActive(false);
        lightingCategory.SetActive(false);

        furnitureButton.onClick.AddListener(() => ShowCategory(furnitureCategory));
        decorButton.onClick.AddListener(() => ShowCategory(decorCategory));
        lightingButton.onClick.AddListener(() => ShowCategory(lightingCategory));
    }

    // ===== TOP CATEGORY =====
    private void ShowCategory(GameObject categoryPanel)
    {
        scrollView.SetActive(true);
        typeContent.SetActive(false);
        ClearPrefabs();

        furnitureCategory.SetActive(false);
        decorCategory.SetActive(false);
        lightingCategory.SetActive(false);

        if (currentCategoryPanel == categoryPanel)
        {
            scrollView.SetActive(false);
            currentCategoryPanel = null;
            return;
        }

        currentCategoryPanel = categoryPanel;
        categoryPanel.SetActive(true);
    }

    // ===== TYPE BUTTON (Chair / Table / etc) =====
    public void LoadType(string resourcesPath)
    {
        Debug.Log("=== LoadType CALLED ===");
        
        if (currentCategoryPanel != null)
        {
            currentCategoryPanel.SetActive(false);
            Debug.Log($"Hiding category panel: {currentCategoryPanel.name}");
        }

        typeContent.SetActive(true);
        ClearPrefabs();
        LoadItemsFromResources(resourcesPath);
    }

    private void LoadItemsFromResources(string path)
    {
        Debug.Log("=== LoadItemsFromResources ===");
        Debug.Log($"Loading prefabs from: Resources/{path}/Prefabs");

        GameObject[] prefabs = Resources.LoadAll<GameObject>($"{path}/Prefabs");
        Debug.Log($"Prefabs found: {prefabs.Length}");

        if (prefabs.Length == 0)
        {
            Debug.LogWarning($"NO PREFABS FOUND at Resources/{path}/Prefabs");
        }

        foreach (GameObject prefab in prefabs)
        {
            Debug.Log($"Prefab found: {prefab.name}");

            string previewPath = $"{path}/Previews/{prefab.name}";
            Debug.Log($"Trying to load preview at: Resources/{previewPath}");

            Sprite preview = Resources.Load<Sprite>(previewPath);

            if (preview == null)
            {
                Debug.LogWarning($"❌ Preview NOT found for {prefab.name}");
                continue;
            }

            Debug.Log($"✅ Preview loaded for {prefab.name}");
            CreateItemButton(prefab, preview, path);
        }
    }

    private void CreateItemButton(GameObject prefab, Sprite preview, string path)
    {
        GameObject btnObj = Instantiate(itemButtonPrefab, prefabsContent);
        btnObj.SetActive(true);

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

    private void SelectItem(FurnitureItem item, GameObject btn)
    {
        selectedItem = item;

        foreach (var b in currentButtons)
            b.GetComponent<Image>().color = Color.white;

        btn.GetComponent<Image>().color = new Color(0.3f, 0.6f, 1f, 0.8f);

        CreateGhost();
    }

    private void CreateGhost()
    {
        if (ghostInstance)
            Destroy(ghostInstance);

        ghostInstance = Instantiate(selectedItem.prefab);
        ghostInstance.layer = 0;
        ghost = ghostInstance.AddComponent<FurniturePlacementGhost>();

        // Настраиваем ghost
        ghost.head = Camera.main.transform;
        ghost.distance = spawnDistance;
        ghost.blockingMask = blockingMask;
        
        // ===== НАСТРОЙКИ ПРИВЯЗКИ К ПОЛУ =====
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

    public void AddSelectedObject()
    {
        if (selectedItem == null || ghost == null)
            return;

        if (!ghost.CanPlace)
            return;

        Instantiate(
            selectedItem.prefab,
            ghost.transform.position,
            ghost.transform.rotation
        );

        Destroy(ghostInstance);
        ghost = null;
    }

    private void ClearPrefabs()
    {
        foreach (Transform t in prefabsContent)
            Destroy(t.gameObject);

        currentButtons.Clear();
        selectedItem = null;
    }
    
    public void RotateGhostRight()
    {
        if (ghostInstance != null)
        {
            ghostInstance.transform.Rotate(0, 90f, 0);
        }
    }

    public void RotateGhostLeft()
    {
        if (ghostInstance != null)
        {
            ghostInstance.transform.Rotate(0, -90f, 0);
        }
    }
}