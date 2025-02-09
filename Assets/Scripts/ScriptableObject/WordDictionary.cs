using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordDictionary", menuName = "Scriptable Objects/WordDictionary")]
public class WordDictionary : ScriptableObject
{
    [System.Serializable]
    public class WordEntry
    {
        public string word;       // 単語
        [TextArea(1, 10)]
        public string description; // 説明文
        public Color textColor;   // 表示色
    }
    
    public static WordDictionary Instance; // シングルトンインスタンス

    public List<WordEntry> words; // 辞書データ
    
    public void SetInstance() => Instance = this;
    
    public WordEntry GetWordEntry(string word)
    {
        foreach (var entry in words)
        {
            if (entry.word == word)
            {
                return entry;
            }
        }
        return null; // 見つからなかった場合
    }
}