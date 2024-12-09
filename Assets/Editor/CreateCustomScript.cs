using System;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;

public class CreateCustomScript
{
    private const string SCRIPTS_FOLDER_PATH = "Assets/Scripts/";

    [MenuItem("Assets/Create/Custom C# Script", false, -9999)]
    public static void CreateNewScript()
    {
        var selectedPath = GetSelectedPath();
        var filePath = ShowSaveFilePanel(selectedPath);

        if (string.IsNullOrEmpty(filePath))
        {
            // User cancelled the save operation
            return;
        }

        var scriptName = Path.GetFileNameWithoutExtension(filePath);
        var namespaceName = GetNamespaceFromPath(selectedPath);

        CreateScriptFile(filePath, scriptName, namespaceName);
        AssetDatabase.Refresh();
    }

    private static string GetSelectedPath()
    {
        var path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

    private static string ShowSaveFilePanel(string initialPath)
    {
        return EditorUtility.SaveFilePanelInProject(
            "Save Script",
            "NewScript.cs",
            "cs",
            "Please enter a file name to save the script to.",
            initialPath
        );
    }

    private static string GetNamespaceFromPath(string path)
    {
        if (!path.Contains(SCRIPTS_FOLDER_PATH))
        {
            return string.Empty;
        }

        var startIndex = path.IndexOf(SCRIPTS_FOLDER_PATH, StringComparison.Ordinal) + SCRIPTS_FOLDER_PATH.Length;
        return path[startIndex..].Replace("/", ".");
    }

    private static void CreateScriptFile(string filePath, string scriptName, string namespaceName)
    {
        var templateContent = File.ReadAllText("Assets/Editor/CustomScriptTemplates/CustomScriptTemplate.txt");
        // templateContent = templateContent.Replace("#NAMESPACE#", namespaceName);
        templateContent = templateContent.Replace("#SCRIPT_NAME#", scriptName);

        File.WriteAllText(filePath, templateContent);
    }
}
