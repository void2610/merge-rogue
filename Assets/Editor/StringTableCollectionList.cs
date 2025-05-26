//StringTableCollectionを一括管理するScriptableObject
//このスクリプトは任意のEditorフォルダ以下に配置すること。
#if ODIN_INSPECTOR
// Odin Inspectorを使う前提ですが、なくても動きます。
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using System.Linq;
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

#if ODIN_INSPECTOR
	[Button]
#endif
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

            //後で実装するやつ
            //StringTableCollection col = collection;
            //LocalizeUtil.SetSmart(ref col, false);
        }
    }
#if ODIN_INSPECTOR
	[Button]
#endif
//シートを開くやつ
    public void OpenSheet()
    {
        GoogleSheets.OpenSheetInBrowser(spreadSheetID);
    }
}