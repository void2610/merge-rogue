using System;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Cysharp.Threading.Tasks;


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
    /// マージ時のボールの近くにある別のボールを取得する(2つのボールを無視)
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
    
    /// <summary>
    /// 1つのボールの近くにある別のボールを取得する(1つのボールを無視)
    /// </summary>
    public static List<BallBase> GetNearbyBalls(GameObject obj, float radius)
    {
        var result = new List<BallBase>();
        var hitColliders = Physics2D.OverlapCircleAll(obj.transform.position, radius);
        // 取得したコライダーをリストに変換
        foreach (var col in hitColliders)
        {
            // 自身を無視
            if (col.gameObject == obj) continue;
            
            var ball = col.gameObject.GetComponent<BallBase>();
            if (ball == null) continue;
            if (ball.IsFrozen || ball.isDestroyed) continue;
            
            result.Add(ball);
        }
        return result;
    }
    
    /// <summary>
    /// 指定時間 await するが、途中で指定した条件が満たされた場合は即座に終了する
    /// </summary>
    /// <param name="delayTime">待機時間(ms)</param>
    /// <param name="condition">途中で終了させる条件</param>
    /// <param name="cancellationToken">キャンセル用トークン（オプション）</param>
    public static async UniTask WaitOrSkip(int delayTime, Func<bool> condition, CancellationToken cancellationToken = default)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            var delayTask = UniTask.Delay(delayTime, cancellationToken: cts.Token);
            var conditionTask = UniTask.WaitUntil(condition, cancellationToken: cts.Token);
            
            var result = await UniTask.WhenAny(delayTask, conditionTask);

            // 待機をスキップするためキャンセル
            if (result == 1) cts.Cancel();
        }
    }

    private void Awake()
    {
        wordDictionary.SetInstance();
    }
}
