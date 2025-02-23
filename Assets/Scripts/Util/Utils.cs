using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.EventSystems;

public class Utils : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    
    /// <summary>
    /// オブジェクトにイベントを追加する
    /// </summary>
    public static void AddEventToObject(GameObject obj, System.Action action, EventTriggerType type, bool removeExisting = true)
    {
        var trigger = obj.GetComponent<EventTrigger>();
        if (!trigger)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }
        
        if(removeExisting) trigger.triggers.RemoveAll(x => x.eventID == type);
        
        var entry = new EventTrigger.Entry {eventID = type};
        entry.callback.AddListener((data) => action());
        trigger.triggers.Add(entry);
    }
    
    /// <summary>
    /// オブジェクトから全てのイベントを削除する
    /// </summary>
    public static void RemoveAllEventFromObject(GameObject obj)
    {
        var trigger = obj.GetComponent<EventTrigger>();
        if (trigger)
            trigger.triggers.Clear();
        var button = obj.GetComponent<UnityEngine.UI.Button>();
        if (button)
            button.onClick.RemoveAllListeners();
    }
    
    /// <summary>
    /// 辞書に基づいたハイライト処理を行う
    /// </summary>
    public static string GetHighlightWords(string description)
    {
        // 短い単語から順に処理するためソート
        var sortedWords = WordDictionary.Instance.words.OrderBy(entry => entry.word.Length).Where(entry => description.Contains(entry.word)).ToList();

        foreach (var entry in sortedWords)
        {
            if (string.IsNullOrEmpty(entry.word)) continue;

            // 既存のハイライトを解除
            var containedWords = WordDictionary.Instance.words.Where(e => entry.word.Contains(e.word)).ToList();
            containedWords.ForEach(e => description = RemoveExistingHighlights(description, e.word));

            // ハイライトを適用
            var replacement = $"<link=\"{entry.word}\"><color=#{ColorUtility.ToHtmlStringRGB(entry.textColor)}><nobr>{entry.word}</nobr></color></link>";
            description = description.Replace(entry.word, replacement);
        }

        return description;
    }

    /// <summary>
    /// 既存のハイライトを解除する
    /// </summary>
    private static string RemoveExistingHighlights(string description, string word)
    {
        // ハイライト形式の正規表現を作成
        string pattern = $@"<link=""{word}""><color=#\w+><nobr>{word}</nobr></color></link>";

        // ハイライト部分を元の文字列に戻す
        return Regex.Replace(description, pattern, word);
    }

    /// <summary>
    /// マージ時に近くにある別のボールを取得する
    /// </summary>
    public static List<BallBase> GetNearbyBalls(GameObject obj, GameObject other, float radius)
    {
        var result = new List<BallBase>();
        var hitColliders = Physics2D.OverlapCircleAll(obj.transform.position, radius);
        // 取得したコライダーをリストに変換
        foreach (var col in hitColliders)
        {
            // 自身を無視
            if (col.gameObject == obj) continue;
            // merge相手を無視
            if (col.gameObject == other) continue;
            
            var ball = col.gameObject.GetComponent<BallBase>();
            if (ball == null) continue;
            if (ball.IsFrozen || ball.isDestroyed) continue;
            
            result.Add(ball);
        }
        return result;
    }

    private void Awake()
    {
        wordDictionary.SetInstance();
    }
}
