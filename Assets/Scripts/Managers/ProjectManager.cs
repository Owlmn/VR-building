using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectManager : MonoBehaviour
{
    public static ProjectManager Instance { get; private set; }
    
    [SerializeField] private string projectsFolder = "VRInteriorProjects";
    [SerializeField] private string projectsListFile = "projects.json";
    
    private ProjectList projectList;
    private string projectsPath;
    private ProjectData currentProject;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeProjectsSystem();
    }
    
    private void InitializeProjectsSystem()
    {
        // Определяем путь для сохранения проектов
        projectsPath = Path.Combine(Application.persistentDataPath, projectsFolder);
        
        if (!Directory.Exists(projectsPath))
        {
            Directory.CreateDirectory(projectsPath);
        }
        
        LoadProjectsList();
    }
    
    private void LoadProjectsList()
    {
        string listPath = Path.Combine(projectsPath, projectsListFile);
        
        if (File.Exists(listPath))
        {
            string json = File.ReadAllText(listPath);
            projectList = JsonUtility.FromJson<ProjectList>(json);
        }
        else
        {
            projectList = new ProjectList();
            SaveProjectsList();
        }
    }
    
    private void SaveProjectsList()
    {
        string listPath = Path.Combine(projectsPath, projectsListFile);
        string json = JsonUtility.ToJson(projectList, true);
        File.WriteAllText(listPath, json);
    }
    
    public void CreateNewProject(string projectName)
    {
        ProjectData newProject = new ProjectData
        {
            projectName = projectName,
            creationDate = DateTime.Now,
            lastModified = DateTime.Now
        };
        
        currentProject = newProject;
        projectList.projects.Add(newProject);
        SaveProjectsList();
        
        // Загружаем сцену редактора
        SceneManager.LoadScene("EditorScene");
    }
    
    public void SaveCurrentProject(string sceneData)
    {
        if (currentProject == null) return;
        
        currentProject.sceneData = sceneData;
        currentProject.lastModified = DateTime.Now;
        
        // Сохраняем проект в файл
        string projectPath = Path.Combine(projectsPath, $"{currentProject.projectName}_{currentProject.creationDate.Ticks}.json");
        string projectJson = JsonUtility.ToJson(currentProject, true);
        File.WriteAllText(projectPath, projectJson);
        
        // Обновляем список
        SaveProjectsList();
    }
    
    public void LoadProject(ProjectData project)
    {
        currentProject = project;
        SceneManager.LoadScene("EditorScene");
    }
    
    public List<ProjectData> GetRecentProjects(int count = 5)
    {
        projectList.projects.Sort((a, b) => b.lastModified.CompareTo(a.lastModified));
        return projectList.projects.GetRange(0, Mathf.Min(count, projectList.projects.Count));
    }
    
    public ProjectData GetCurrentProject() => currentProject;
    
    public void SetCurrentProject(ProjectData project) => currentProject = project;
}