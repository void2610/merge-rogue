using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordDictionary", menuName = "Scriptable Objects/WordDictionary")]
public class WordDictionary : ScriptableObject
{
    [System.Serializable]
    public class WordEntry
    {
        public string word;       // 単語
        public string description; // 説明文
        public Color textColor;   // 表示色
    }

    public List<WordEntry> words; // 辞書データ
    
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