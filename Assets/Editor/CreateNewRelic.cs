using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateNewRelic : EditorWindow
{
    private string relicName = "newRelic"; // レリックの名前
    private string templatePath = "Assets/Editor/Templates/RelicTemplate.txt";
    private string relicDataPath = "Assets/ScriptableObjects/RelicData/";
    private string relicScriptPath = "Assets/Scripts/Relic/";

    [MenuItem("Tools/Create New Relic")]
    public static void ShowWindow()
    {
        GetWindow<CreateNewRelic>("Create New Relic");
    }

    private void OnGUI()
    {
        GUILayout.Label("Relic Creator", EditorStyles.boldLabel);

        relicName = EditorGUILayout.TextField("Relic Name", relicName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);
        templatePath = EditorGUILayout.TextField("Template Path", templatePath);
        relicDataPath = EditorGUILayout.TextField("Relic Data Path", relicDataPath);
        relicScriptPath = EditorGUILayout.TextField("Relic Script Path", relicScriptPath);

        if (GUILayout.Button("Create Relic"))
        {
            CreateRelicAssets(relicName);
        }
    }

    private void CreateRelicAssets(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Relic name is required!");
            return;
        }

        // 1. ScriptableObjectの作成
        var relicDataFullPath = Path.Combine(relicDataPath, name + ".asset");
        var relicData = ScriptableObject.CreateInstance<RelicData>();
        relicData.className = name;
        AssetDatabase.CreateAsset(relicData, relicDataFullPath);

        Debug.Log($"RelicData asset created at: {relicDataFullPath}");

        // 2. csファイルの作成
        var scriptFullPath = Path.Combine(relicScriptPath, name + ".cs");

        if (File.Exists(templatePath))
        {
            var templateContent = File.ReadAllText(templatePath);
            var scriptContent = templateContent.Replace("#SCRIPT_NAME#", name);
            File.WriteAllText(scriptFullPath, scriptContent);

            Debug.Log($"Script file created at: {scriptFullPath}");
        }
        else
        {
            Debug.LogError($"Template file not found at: {templatePath}");
        }

        // アセットデータベースをリフレッシュ
        AssetDatabase.Refresh();
    }
}
