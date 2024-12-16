using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateNewBall : EditorWindow
{
    private string ballName = "newBall"; // 名前
    private string templatePath = "Assets/Editor/Templates/BallTemplate.txt";
    private string ballDataPath = "Assets/ScriptableObjects/BallData/";
    private string ballScriptPath = "Assets/Scripts/Ball/";

    [MenuItem("Tools/Create New Ball")]
    public static void ShowWindow()
    {
        GetWindow<CreateNewBall>("Create New Ball");
    }

    private void OnGUI()
    {
        GUILayout.Label("Ball Creator", EditorStyles.boldLabel);

        ballName = EditorGUILayout.TextField("Ball Name", ballName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);
        templatePath = EditorGUILayout.TextField("Template Path", templatePath);
        ballDataPath = EditorGUILayout.TextField("Ball Data Path", ballDataPath);
        ballScriptPath = EditorGUILayout.TextField("Ball Script Path", ballScriptPath);

        if (GUILayout.Button("Create Ball !"))
        {
            CreateBallAssets(ballName);
        }
    }

    private void CreateBallAssets(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Ball name is required!");
            return;
        }

        // 1. ScriptableObjectの作成
        var ballDataFullPath = Path.Combine(ballDataPath, name + ".asset");
        var ballData = ScriptableObject.CreateInstance<BallData>();
        ballData.className = name;
        AssetDatabase.CreateAsset(ballData, ballDataFullPath);

        Debug.Log($"BallData asset created at: {ballDataFullPath}");

        // 2. csファイルの作成
        var scriptFullPath = Path.Combine(ballScriptPath, name + ".cs");

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
