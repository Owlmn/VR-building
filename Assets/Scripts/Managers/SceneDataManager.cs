using System.Collections.Generic;
using UnityEngine;

public class SceneDataManager : MonoBehaviour
{
    // Здесь хранятся все объекты сцены
    private List<SceneObjectData> sceneObjects = new List<SceneObjectData>();
    
    [System.Serializable]
    public class SceneObjectData
    {
        public string prefabName;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        // Дополнительные параметры
    }
    
    [System.Serializable]
    public class SceneData
    {
        public List<SceneObjectData> objects = new List<SceneObjectData>();
    }
    
    public string GetSceneData()
    {
        SceneData data = new SceneData();
        data.objects = sceneObjects;
        return JsonUtility.ToJson(data);
    }
    
    public void LoadSceneData(string jsonData)
    {
        SceneData data = JsonUtility.FromJson<SceneData>(jsonData);
        sceneObjects = data.objects;
        
        // Восстанавливаем объекты на сцене
        // (здесь нужно добавить логику создания объектов)
    }
    
    // Метод для добавления объектов в сцену
    public void AddObject(SceneObjectData objectData)
    {
        sceneObjects.Add(objectData);
    }
}