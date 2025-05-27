//こちらのスクリプトはEditorフォルダに配置します。
using UnityEngine;
using UnityToolbarExtender;
using UnityEngine.Localization;

using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneSwitchLeftButton
{
    //作成したStringTableCollectionList.assetのパスをコピーして貼り付ける。
    public const string LOCALIZATION_ASSET_PATH     = "Assets/Localization/MyStringTableCollectionList.asset";
    // ついでに任意のシーンを開く機能もつけるなら
    public const string TITLE_SCENE_PATH         = "Assets/Scenes/TitleScene.unity";
    public const string MAIN_SCENE_PATH            = "Assets/Scenes/MainScene.unity";
	
    static SceneSwitchLeftButton()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUILeft);
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUIRight);
    }

    private static void OnToolbarGUILeft()
    {
        //プレイボタン左側の拡張はこちらに記述
        GUILayout.FlexibleSpace();

        // ついでに任意のシーンを開く機能もつけるなら
        if (GUILayout.Button(new GUIContent("TITLE", "")))
            EditorSceneManager.OpenScene(TITLE_SCENE_PATH, OpenSceneMode.Single);
        if (GUILayout.Button(new GUIContent("MAIN", "")))
        	EditorSceneManager.OpenScene(MAIN_SCENE_PATH, OpenSceneMode.Single);

        //Pull Allボタンの実装
        if (GUILayout.Button(new GUIContent("LCLZ_PULL", "")))
        {
            var stcl = AssetDatabase.LoadAssetAtPath<StringTableCollectionList>(LOCALIZATION_ASSET_PATH);
            stcl.PullAll();
        }
        //Openの実装
        if (GUILayout.Button(new GUIContent("LCLZ_OPEN", "")))
        {
            var stcl = AssetDatabase.LoadAssetAtPath<StringTableCollectionList>(LOCALIZATION_ASSET_PATH);
            stcl.OpenSheet();
        }

    }

    private static void OnToolbarGUIRight()
    {
    }
}