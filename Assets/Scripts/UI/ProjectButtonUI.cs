using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ProjectButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI projectNameText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button loadButton;
    
    private ProjectData projectData;
    
    public void Initialize(ProjectData project)
    {
        projectData = project;
        projectNameText.text = project.projectName;
        dateText.text = project.lastModified.ToString("dd.MM.yyyy HH:mm");
        
        loadButton.onClick.AddListener(LoadProject);
    }
    
    private void LoadProject()
    {
        ProjectManager.Instance.LoadProject(projectData);
    }
}