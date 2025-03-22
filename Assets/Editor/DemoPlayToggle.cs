using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class DemoPlayToggle
{
    [MenuItem("Tools/Toggle DEMO_PLAY")]
    public static void ToggleDemoPlay()
    {
        // 現在のBuildTargetGroupを取得
        var target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        // 現在のシンボル一覧を取得
        var defines = PlayerSettings.GetScriptingDefineSymbols(target);
        
        // DEMO_PLAYが定義されているかチェック
        if (defines.Contains("DEMO_PLAY"))
        {
            // 定義済みの場合は削除
            defines = defines.Replace("DEMO_PLAY", "");
            Debug.Log("DEMO_PLAYを無効にしました。");
        }
        else
        {
            // 定義されていない場合は追加
            if (!string.IsNullOrEmpty(defines))
                defines += ";";
            defines += "DEMO_PLAY";
            Debug.Log("DEMO_PLAYを有効にしました。");
        }
        
        // 更新後のシンボルを設定
        PlayerSettings.SetScriptingDefineSymbols(target, defines);
    }
}