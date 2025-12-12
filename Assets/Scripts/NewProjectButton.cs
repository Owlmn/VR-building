using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewProjectButton : MonoBehaviour
{
    [Header("Настройки")]
    public string newProjectSceneName = "NewProjectScene";
    
    void Start()
    {
        // Подключаем метод к кнопке
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(LoadNewProjectScene);
        }
    }
    
    public void LoadNewProjectScene()
    {
        Debug.Log("Загружаем сцену нового проекта...");
        
        // ПРЯМАЯ ЗАГРУЗКА СЦЕНЫ (текущая сцена выгружается)
        SceneManager.LoadScene(newProjectSceneName, LoadSceneMode.Single);
    }
}