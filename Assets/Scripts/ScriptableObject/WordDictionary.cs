using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(fileName = "WordDictionary", menuName = "Scriptable Objects/WordDictionary")]
public class WordDictionary : ScriptableObject
{
    [System.Serializable]
    public class WordEntry
    {
        public string localizationKey; // ローカライズキー（例: MAX_HEALTH）
        public Color textColor;   // 表示色
        
        // ローカライズされた単語名を取得
        public string GetLocalizedWord()
        {
            if (LocalizeStringLoader.Instance != null)
            {
                return LocalizeStringLoader.Instance.Get($"{localizationKey}_N");
            }
            
            // フォールバック：直接取得
            var table = LocalizationSettings.StringDatabase.GetTable("WordDictionary");
            return table.GetEntry($"{localizationKey}_N")?.GetLocalizedString() ?? $"[{localizationKey}]";
        }
        
        // ローカライズされた説明文を取得
        public string GetLocalizedDescription()
        {
            if (LocalizeStringLoader.Instance != null)
            {
                return LocalizeStringLoader.Instance.Get($"{localizationKey}_D");
            }
            
            // フォールバック：直接取得
            var table = LocalizationSettings.StringDatabase.GetTable("WordDictionary");
            return table.GetEntry($"{localizationKey}_D")?.GetLocalizedString() ?? $"[{localizationKey}]";
        }
    }
    
    public List<WordEntry> words; // 辞書データ
    
    // ローカライズキーからWordEntryを取得
    public WordEntry GetWordEntryByKey(string localizationKey)
    {
        foreach (var entry in words)
        {
            if (entry.localizationKey == localizationKey)
            {
                return entry;
            }
        }
        return null; // 見つからなかった場合
    }
    
    // 文字列がWordDictionaryに存在するかチェック（キーまたはローカライズされた単語名で検索）
    public bool ContainsWord(string searchText)
    {
        return GetWordEntryByAny(searchText) != null;
    }
    
    // キーまたはローカライズされた単語名からWordEntryを取得する汎用メソッド
    public WordEntry GetWordEntryByAny(string searchText)
    {
        if (string.IsNullOrEmpty(searchText)) return null;
        
        // まずキーで検索
        var entryByKey = GetWordEntryByKey(searchText);
        if (entryByKey != null) return entryByKey;
        
        // 次にローカライズされた単語名で検索
        return GetWordEntryByLocalizedWord(searchText);
    }
    
    // ローカライズされた単語名からWordEntryを取得
    public WordEntry GetWordEntryByLocalizedWord(string localizedWord)
    {
        if (string.IsNullOrEmpty(localizedWord)) return null;
        
        foreach (var entry in words)
        {
            if (entry.GetLocalizedWord().Equals(localizedWord, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }
        
        return null; // 見つからなかった場合
    }
}