using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StartMenuUI : MonoBehaviour
{
    [Header("Основные кнопки")]
    [SerializeField] private Button newProjectButton;
    [SerializeField] private Button loadTemplateButton;
    
    [Header("Панель предыдущих проектов")]
    [SerializeField] private Transform projectsPanel;
    [SerializeField] private GameObject projectButtonPrefab;
    
    [Header("Окно создания проекта")]
    [SerializeField] private GameObject createProjectWindow;
    [SerializeField] private InputField projectNameInput;
    [SerializeField] private Button createConfirmButton;
    [SerializeField] private Button createCancelButton;
    
    private List<GameObject> projectButtons = new List<GameObject>();
    
    private void Start()
    {
        InitializeButtons();
        LoadRecentProjects();
    }
    
    private void InitializeButtons()
    {
        // Кнопка нового проекта
        newProjectButton.onClick.AddListener(() => {
            createProjectWindow.SetActive(true);
            projectNameInput.text = "Новый проект " + (ProjectManager.Instance.GetRecentProjects().Count + 1);
        });
        
        // Кнопка загрузки шаблона
        loadTemplateButton.onClick.AddListener(LoadTemplate);
        
        // Кнопки окна создания проекта
        createConfirmButton.onClick.AddListener(CreateNewProject);
        createCancelButton.onClick.AddListener(() => createProjectWindow.SetActive(false));
        
        // Фокус на поле ввода
        createProjectWindow.SetActive(false);
    }
    
    private void CreateNewProject()
    {
        string projectName = projectNameInput.text;
        if (string.IsNullOrEmpty(projectName))
        {
            projectName = "Безымянный проект";
        }
        
        ProjectManager.Instance.CreateNewProject(projectName);
    }
    
    private void LoadTemplate()
    {
        // Здесь можно добавить логику загрузки шаблонов
        Debug.Log("Загрузка шаблона...");
        // ProjectManager.Instance.CreateNewProject("Проект из шаблона");
    }
    
    private void LoadRecentProjects()
    {
        // Очищаем старые кнопки
        foreach (var button in projectButtons)
        {
            Destroy(button);
        }
        projectButtons.Clear();
        
        // Загружаем список проектов
        List<ProjectData> recentProjects = ProjectManager.Instance.GetRecentProjects();
        
        // Создаем кнопки для каждого проекта
        foreach (var project in recentProjects)
        {
            GameObject buttonObj = Instantiate(projectButtonPrefab, projectsPanel);
            ProjectButtonUI projectButton = buttonObj.GetComponent<ProjectButtonUI>();
            
            if (projectButton != null)
            {
                projectButton.Initialize(project);
            }
            
            projectButtons.Add(buttonObj);
        }
    }
}