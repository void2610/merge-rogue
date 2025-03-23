using UnityEditor.Build;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class RemoveDemoDefine
{
    // Unityのバッチモードで呼び出すための静的メソッド
    public static void RemoveDemoPlayDefine()
    {
        var target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var defines = PlayerSettings.GetScriptingDefineSymbols(target);

        if (defines.Contains("DEMO_PLAY"))
        {
            // セミコロン区切りで分割し、「DEMO_PLAY」を除外して再結合
            var newDefines = string.Join(";", defines.Split(';').Where(s => s.Trim() != "DEMO_PLAY"));
            Debug.Log("Removing DEMO_PLAY. New defines: " + newDefines);
            PlayerSettings.SetScriptingDefineSymbols(target, newDefines);
        }
        else
        {
            Debug.Log("DEMO_PLAY is not defined.");
        }
    }
}