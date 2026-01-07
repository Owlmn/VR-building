using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectData
{
    public string projectName;
    public DateTime creationDate;
    public DateTime lastModified;
    public string sceneData; // JSON с данными сцены
    public string thumbnailPath; // Путь к скриншоту
}

[System.Serializable]
public class ProjectList
{
    public List<ProjectData> projects = new List<ProjectData>();
}