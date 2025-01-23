using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateNewStageEvent : EditorWindow
{
    private string _stageEventName = "newStageEvent"; // レリックの名前
    private string _templatePath = "Assets/Editor/Templates/StageEventTemplate.txt";
    private string _stageEventDataPath = "Assets/ScriptableObjects/StageEventData/";
    private string _stageEventScriptPath = "Assets/Scripts/StageEvent/";

    [MenuItem("Tools/Create New StageEvent")]
    public static void ShowWindow()
    {
        GetWindow<CreateNewStageEvent>("Create New StageEvent");
    }

    private void OnGUI()
    {
        GUILayout.Label("StageEvent Creator", EditorStyles.boldLabel);

        _stageEventName = EditorGUILayout.TextField("StageEvent Name", _stageEventName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);
        _templatePath = EditorGUILayout.TextField("Template Path", _templatePath);
        _stageEventDataPath = EditorGUILayout.TextField("StageEvent Data Path", _stageEventDataPath);
        _stageEventScriptPath = EditorGUILayout.TextField("StageEvent Script Path", _stageEventScriptPath);

        if (GUILayout.Button("Create StageEvent !"))
        {
            CreateStageEventAssets(_stageEventName);
        }
    }

    private void CreateStageEventAssets(string n)
    {
        if (string.IsNullOrEmpty(n))
        {
            Debug.LogError("StageEvent name is required!");
            return;
        }

        // 1. ScriptableObjectの作成
        var stageEventDataFullPath = Path.Combine(_stageEventDataPath, n + ".asset");
        var stageEventData = ScriptableObject.CreateInstance<StageEventData>();
        stageEventData.className = n;
        AssetDatabase.CreateAsset(stageEventData, stageEventDataFullPath);

        Debug.Log($"StageEventData asset created at: {stageEventDataFullPath}");

        // 2. csファイルの作成
        var scriptFullPath = Path.Combine(_stageEventScriptPath, n + ".cs");

        if (File.Exists(_templatePath))
        {
            var templateContent = File.ReadAllText(_templatePath);
            var scriptContent = templateContent.Replace("#SCRIPT_NAME#", n);
            File.WriteAllText(scriptFullPath, scriptContent);

            Debug.Log($"Script file created at: {scriptFullPath}");
        }
        else
        {
            Debug.LogError($"Template file not found at: {_templatePath}");
        }

        // アセットデータベースをリフレッシュ
        AssetDatabase.Refresh();
    }
}
