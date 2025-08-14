using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// エディタ拡張でローカライゼーションテーブルを読み込むためのヘルパークラス
/// </summary>
public static class LocalizationEditorHelper
{
    /// <summary>
    /// ローカライゼーションテーブルから日本語テキストを読み込む
    /// </summary>
    /// <param name="tableName">テーブル名（例: "StageEvent", "Relics"など）</param>
    /// <returns>キーと日本語テキストの辞書</returns>
    public static Dictionary<string, string> LoadJapaneseLocalization(string tableName)
    {
        var japaneseLocalizations = new Dictionary<string, string>();
        
        try
        {
            // 日本語テーブルファイルを直接読み込み
            var jaTablePath = $"Assets/Localization/StringTable/{tableName}/{tableName}_ja.asset";
            var sharedDataPath = $"Assets/Localization/StringTable/{tableName}/{tableName} Shared Data.asset";
            
            var jaTableText = System.IO.File.ReadAllText(jaTablePath);
            var sharedDataText = System.IO.File.ReadAllText(sharedDataPath);
            
            // 共有データからキーとIDの対応を取得
            var keyIdMap = ParseSharedData(sharedDataText);
            
            // 日本語テーブルからIDと文字列の対応を取得
            var idTextMap = ParseJapaneseTable(jaTableText);
            
            // キーと日本語文字列の対応を構築
            foreach (var keyId in keyIdMap)
            {
                if (idTextMap.TryGetValue(keyId.Value, out var localizedText))
                {
                    japaneseLocalizations[keyId.Key] = localizedText;
                }
                else
                {
                    // IDが存在しない場合は空文字列として登録
                    japaneseLocalizations[keyId.Key] = "";
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ローカライゼーションデータの読み込みに失敗: {ex.Message}");
        }
        
        return japaneseLocalizations;
    }
    
    /// <summary>
    /// 共有データファイルを解析してキー→ID対応を取得
    /// </summary>
    private static Dictionary<string, string> ParseSharedData(string yamlText)
    {
        var keyIdMap = new Dictionary<string, string>();
        
        // YAMLからエントリを抽出
        var entryMatches = Regex.Matches(yamlText, @"- m_Id: (\d+)\s+m_Key: (\w+)");
        
        foreach (Match match in entryMatches)
        {
            var id = match.Groups[1].Value;
            var key = match.Groups[2].Value;
            keyIdMap[key] = id;
        }
        
        return keyIdMap;
    }
    
    /// <summary>
    /// 日本語テーブルファイルを解析してID→テキスト対応を取得
    /// </summary>
    private static Dictionary<string, string> ParseJapaneseTable(string yamlText)
    {
        var idTextMap = new Dictionary<string, string>();
        
        // YAMLからローカライズされたテキストを抽出
        // 引用符あり・なし両方のパターンに対応、空文字列も含む
        var entryMatches = Regex.Matches(yamlText, @"- m_Id: (\d+)\s+m_Localized: (?:""(.*?)""|(.*))\s*$", RegexOptions.Multiline);
        
        foreach (Match match in entryMatches)
        {
            var id = match.Groups[1].Value;
            // 引用符ありパターン（グループ2）または引用符なしパターン（グループ3）から取得
            var localizedText = match.Groups[2].Success 
                ? match.Groups[2].Value 
                : match.Groups[3].Value;
            // Unicodeエスケープシーケンスをデコード
            localizedText = Regex.Unescape(localizedText);
            idTextMap[id] = localizedText;
        }
        
        return idTextMap;
    }
    
    /// <summary>
    /// 指定されたキーからローカライズされたテキストを取得
    /// </summary>
    public static string GetLocalizedText(Dictionary<string, string> localizations, string key)
    {
        if (localizations == null || string.IsNullOrEmpty(key))
            return null;
            
        localizations.TryGetValue(key, out var localizedText);
        return localizedText;
    }
}