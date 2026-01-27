#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class InspectorStylePreviewGenerator : EditorWindow
{
    private string prefabsRoot = "Assets/Resources/Furniture/Prefabs";
    private string previewsRoot = "Assets/Resources/Furniture/Previews";
    
    // Настройки
    private int previewSize = 256;
    private bool useUnityPreview = true; // Использовать встроенную систему Unity
    
    // Прогресс
    private int processedCount = 0;
    private int totalCount = 0;
    
    [MenuItem("Tools/Превью как в инспекторе")]
    public static void ShowWindow()
    {
        GetWindow<InspectorStylePreviewGenerator>("Превью как в инспекторе");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Генератор превью как в инспекторе Unity", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(10);
        
        // Пути
        prefabsRoot = EditorGUILayout.TextField("Папка с префабами", prefabsRoot);
        previewsRoot = EditorGUILayout.TextField("Папка для превью", previewsRoot);
        
        EditorGUILayout.Space(10);
        
        // Настройки
        previewSize = EditorGUILayout.IntSlider("Размер превью", previewSize, 64, 512);
        useUnityPreview = EditorGUILayout.Toggle("Использовать превью Unity", useUnityPreview);
        
        if (!useUnityPreview)
        {
            EditorGUILayout.HelpBox("При отключении будет использован кастомный рендер", MessageType.Info);
        }
        
        EditorGUILayout.Space(20);
        
        // Кнопки
        if (GUILayout.Button("Сгенерировать все превью", GUILayout.Height(40)))
        {
            GenerateAllPreviews();
        }
        
        if (GUILayout.Button("Сгенерировать для выбранных", GUILayout.Height(30)))
        {
            GenerateSelectedPreviews();
        }
        
        EditorGUILayout.Space(10);
        
        // Прогресс
        if (totalCount > 0)
        {
            EditorGUILayout.LabelField($"Прогресс: {processedCount} / {totalCount}");
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(rect, (float)processedCount / totalCount, "Генерация...");
        }
    }
    
    void GenerateAllPreviews()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsRoot });
        totalCount = guids.Length;
        processedCount = 0;
        
        Debug.Log($"Начинаем генерацию {totalCount} превью...");
        
        // Сначала заставляем Unity сгенерировать все превью
        ForceGenerateAllPreviews(guids);
        
        // Теперь сохраняем
        for (int i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            
            EditorUtility.DisplayProgressBar("Сохранение превью", 
                Path.GetFileName(prefabPath), (float)i / guids.Length);
            
            SavePreviewFromUnity(prefabPath);
            
            processedCount = i + 1;
            Repaint();
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ Готово! Сгенерировано {totalCount} превью");
    }
    
    void ForceGenerateAllPreviews(string[] guids)
    {
        // Этот метод заставляет Unity сгенерировать превью для всех объектов
        Debug.Log("Генерация превью в Unity...");
        
        for (int i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                // Запрашиваем превью - это заставляет Unity сгенерировать его
                AssetPreview.GetAssetPreview(prefab);
                
                if (i % 10 == 0)
                {
                    EditorUtility.DisplayProgressBar("Подготовка превью", 
                        $"Генерация: {i + 1}/{guids.Length}", (float)i / guids.Length);
                    System.Threading.Thread.Sleep(10); // Даем Unity время на генерацию
                }
            }
        }
        
        EditorUtility.ClearProgressBar();
    }
    
    void SavePreviewFromUnity(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;
        
        string category = GetCategoryFromPath(prefabPath);
        string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
        
        Texture2D preview = null;
        
        if (useUnityPreview)
        {
            // Получаем превью из системы Unity
            preview = AssetPreview.GetAssetPreview(prefab);
            
            // Если превью еще не готово, ждем
            if (preview == null)
            {
                for (int attempt = 0; attempt < 50; attempt++) // 5 секунд максимум
                {
                    System.Threading.Thread.Sleep(100);
                    preview = AssetPreview.GetAssetPreview(prefab);
                    if (preview != null) break;
                }
            }
            
            // Если все еще нет, пробуем миниатюру
            if (preview == null)
            {
                preview = AssetPreview.GetMiniThumbnail(prefab);
            }
        }
        
        // Если не удалось получить превью Unity или выбран кастомный рендер
        if (preview == null)
        {
            preview = GenerateCustomPreview(prefab);
        }
        
        if (preview != null)
        {
            SavePreviewTexture(preview, category, prefabName);
        }
        else
        {
            Debug.LogWarning($"Не удалось получить превью для {prefabName}");
        }
    }
    
    Texture2D GenerateCustomPreview(GameObject prefab)
    {
        // Создаем кастомное превью похожее на Unity
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null) return null;
        
        instance.hideFlags = HideFlags.HideAndDontSave;
        
        try
        {
            // Настройки как у Unity
            GameObject cameraGO = new GameObject("PreviewCamera");
            cameraGO.hideFlags = HideFlags.HideAndDontSave;
            Camera camera = cameraGO.AddComponent<Camera>();
            
            // Настройки камеры как в инспекторе Unity
            SetupUnityStyleCamera(camera, instance);
            
            // Создаем свет как в Unity
            CreateUnityStyleLighting();
            
            // Рендерим
            RenderTexture rt = new RenderTexture(previewSize, previewSize, 24);
            camera.targetTexture = rt;
            camera.Render();
            
            // Сохраняем
            RenderTexture.active = rt;
            Texture2D preview = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
            preview.ReadPixels(new Rect(0, 0, previewSize, previewSize), 0, 0);
            preview.Apply();
            
            // Очищаем
            RenderTexture.active = null;
            camera.targetTexture = null;
            
            DestroyImmediate(rt);
            DestroyImmediate(cameraGO);
            
            return preview;
        }
        finally
        {
            DestroyImmediate(instance);
        }
    }
    
    void SetupUnityStyleCamera(Camera camera, GameObject target)
    {
        // Настройки камеры как в инспекторе Unity
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1921569f, 0.1921569f, 0.1921569f, 0f); // Unity grey
        camera.orthographic = false; // Перспективная камера как в Unity
        camera.fieldOfView = 30f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1000f;
        
        // Рассчитываем положение камеры как в Unity
        Bounds bounds = CalculateBounds(target);
        float maxExtent = bounds.extents.magnitude;
        float minDistance = (maxExtent * 2.0f) / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        
        Vector3 cameraPosition = bounds.center;
        cameraPosition -= Vector3.forward * minDistance;
        
        // Немного смещаем вверх для лучшего обзора
        cameraPosition += Vector3.up * maxExtent * 0.3f;
        
        camera.transform.position = cameraPosition;
        camera.transform.LookAt(bounds.center);
    }
    
    void CreateUnityStyleLighting()
    {
        // Создаем освещение как в Unity превью
        GameObject lightGO = new GameObject("PreviewLight");
        lightGO.hideFlags = HideFlags.HideAndDontSave;
        Light light = lightGO.AddComponent<Light>();
        
        light.type = LightType.Directional;
        light.color = new Color(0.769f, 0.769f, 0.769f, 1f); // Unity default light color
        light.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        // Добавляем заполняющий свет
        GameObject fillLightGO = new GameObject("FillLight");
        fillLightGO.hideFlags = HideFlags.HideAndDontSave;
        Light fillLight = fillLightGO.AddComponent<Light>();
        
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.439f, 0.439f, 0.439f, 1f);
        fillLight.intensity = 0.5f;
        fillLight.transform.rotation = Quaternion.Euler(30f, 30f, 0f);
    }
    
    void SavePreviewTexture(Texture2D preview, string category, string prefabName)
    {
        // Создаем папку категории
        string categoryPath = Path.Combine(previewsRoot, category);
        if (!Directory.Exists(categoryPath))
            Directory.CreateDirectory(categoryPath);
        
        // Сохраняем PNG
        string savePath = Path.Combine(categoryPath, prefabName + ".png");
        byte[] bytes = preview.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        
        // Настраиваем импорт как Sprite
        AssetDatabase.ImportAsset(savePath);
        TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
        
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = previewSize;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        
        Debug.Log($"Сохранено: {savePath}");
        
        // Очищаем если это не превью Unity (они управляются Unity)
        if (useUnityPreview)
        {
            // Не удаляем, это системные текстуры
        }
        else
        {
            DestroyImmediate(preview);
        }
    }
    
    void GenerateSelectedPreviews()
    {
        GameObject[] selected = Selection.gameObjects;
        totalCount = selected.Length;
        processedCount = 0;
        
        if (totalCount == 0)
        {
            EditorUtility.DisplayDialog("Внимание", "Выберите префабы в Project окне", "OK");
            return;
        }
        
        Debug.Log($"Генерация превью для {totalCount} выбранных объектов...");
        
        // Сначала генерируем превью в Unity
        foreach (GameObject obj in selected)
        {
            AssetPreview.GetAssetPreview(obj);
            System.Threading.Thread.Sleep(50); // Даем время на генерацию
        }
        
        // Затем сохраняем
        for (int i = 0; i < selected.Length; i++)
        {
            GameObject obj = selected[i];
            string prefabPath = AssetDatabase.GetAssetPath(obj);
            
            EditorUtility.DisplayProgressBar("Сохранение превью", 
                obj.name, (float)i / selected.Length);
            
            SavePreviewFromUnity(prefabPath);
            
            processedCount = i + 1;
            Repaint();
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ Готово! Сгенерировано {totalCount} превью");
    }
    
    Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.one);
        
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);
        
        return bounds;
    }
    
    string GetCategoryFromPath(string path)
    {
        // Из "Assets/Resources/Furniture/Prefabs/Chairs/chair.prefab" получаем "Chairs"
        string relativePath = path.Replace(prefabsRoot + "/", "");
        string[] parts = relativePath.Split('/');
        
        if (parts.Length > 1)
            return parts[0];
        
        return Path.GetFileName(Path.GetDirectoryName(path));
    }
}
#endif