using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    [Header("Настройки")]
    public string SceneName = "StartScene";
    
    void Start()
    {
        // Подключаем метод к кнопке
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(BackToStartScene);
        }
    }
    
    public void BackToStartScene()
    {
        Debug.Log("Возвращаемся к стартовой сцене...");      
        // ПРЯМАЯ ЗАГРУЗКА СЦЕНЫ (текущая сцена выгружается)
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
    }
}
