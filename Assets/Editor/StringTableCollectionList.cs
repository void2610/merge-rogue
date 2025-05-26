//StringTableCollectionを一括管理するScriptableObject
//このスクリプトは任意のEditorフォルダ以下に配置すること。
#if ODIN_INSPECTOR
// Odin Inspectorを使う前提ですが、なくても動きます。
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Localization拡張/StringCollectionList")]
public class StringTableCollectionList : ScriptableObject
{
    public string spreadSheetID = "任意のSpreadSheetID";
    public SheetsServiceProvider ssp;
    public List<StringTableCollection> collections = new List<StringTableCollection>();

    // OdinのButtonアトリビュートは標準Editorでは使用しない
    //シートをPullするやつ
    public void PullAll()
    {
        var gss = new GoogleSheets(ssp);
        gss.SpreadSheetId = spreadSheetID;
        //ループでそれぞれ取得（取得の是非の判定は割愛）
        foreach (var collection in collections)
        {
            var sheetsExtension = collection.Extensions.OfType<GoogleSheetsExtension>().FirstOrDefault();
            gss.PullIntoStringTableCollection(
                sheetsExtension.SheetId,
                collection,
                sheetsExtension.Columns,
                createUndo: true);
        }
        Debug.Log("Pull All completed.");
    }

    //シートを開くやつ
    public void OpenSheet()
    {
        GoogleSheets.OpenSheetInBrowser(spreadSheetID);
        Debug.Log("Opened Google Sheets in browser.");
    }
}

// カスタムエディタを作成し、標準のボタンを実装
[CustomEditor(typeof(StringTableCollectionList))]
public class StringTableCollectionListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 元のInspectorを描画
        DrawDefaultInspector();
        
        // ターゲットとなるScriptableObjectを取得
        var myTarget = (StringTableCollectionList)target;
        
        // スペースを追加
        EditorGUILayout.Space();
        
        // ボタンを表示
        if (GUILayout.Button("Pull All"))
        {
            myTarget.PullAll();
        }
        
        if (GUILayout.Button("Open Sheet"))
        {
            myTarget.OpenSheet();
        }
    }
}

