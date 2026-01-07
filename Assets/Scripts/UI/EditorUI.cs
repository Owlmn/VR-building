using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditorUI : MonoBehaviour
{
    [Header("Верхняя панель")]
    [SerializeField] private Button backButton;
    [SerializeField] private InputField projectNameInput;
    [SerializeField] private Button saveButton;
    
    [Header("Ссылки")]
    [SerializeField] private SceneDataManager sceneDataManager;
    
    private void Start()
    {
        InitializeUI();
        LoadCurrentProject();
    }
    
    private void InitializeUI()
    {
        // Кнопка назад
        backButton.onClick.AddListener(() => {
            SceneManager.LoadScene("StartScene");
        });
        
        // Сохранение проекта
        saveButton.onClick.AddListener(SaveProject);
        
        // Изменение названия проекта
        projectNameInput.onEndEdit.AddListener(UpdateProjectName);
    }
    
    private void LoadCurrentProject()
    {
        ProjectData currentProject = ProjectManager.Instance.GetCurrentProject();
        if (currentProject != null)
        {
            projectNameInput.text = currentProject.projectName;
            
            // Загружаем данные сцены, если они есть
            if (!string.IsNullOrEmpty(currentProject.sceneData))
            {
                sceneDataManager.LoadSceneData(currentProject.sceneData);
            }
        }
    }
    
    private void SaveProject()
    {
        // Сохраняем данные сцены
        string sceneData = sceneDataManager.GetSceneData();
        
        // Сохраняем проект
        ProjectManager.Instance.SaveCurrentProject(sceneData);
        
        // Визуальная обратная связь
        ShowSaveNotification();
    }
    
    private void UpdateProjectName(string newName)
    {
        ProjectData currentProject = ProjectManager.Instance.GetCurrentProject();
        if (currentProject != null)
        {
            currentProject.projectName = newName;
        }
    }
    
    private void ShowSaveNotification()
    {
        // Можно добавить анимацию или звук сохранения
        Debug.Log("Проект сохранен!");
    }
}