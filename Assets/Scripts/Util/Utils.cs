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
    public static string GetHighlightWords(WordDictionary wordDictionary, string description)
    {
        if (wordDictionary?.words == null || string.IsNullOrEmpty(description)) 
            return description;
        
        // 説明文に含まれる単語のみを取得し、長い単語から順に処理（重複を避けるため）
        var matchedEntries = wordDictionary.words
            .Select(entry => new { Entry = entry, Word = entry.GetLocalizedWord() })
            .Where(item => !string.IsNullOrEmpty(item.Word) && description.Contains(item.Word))
            .OrderByDescending(item => item.Word.Length) // 長い単語から処理
            .ToList();

        foreach (var item in matchedEntries)
        {
            var word = item.Word;
            var entry = item.Entry;
            
            // 既にハイライト済みの部分は処理しない
            if (description.Contains($"<link=\"{entry.localizationKey}\">"))
                continue;
            
            // ハイライトを適用（リンクIDにはlocalizationKeyを使用）
            var replacement = $"<link=\"{entry.localizationKey}\"><color=#{ColorUtility.ToHtmlStringRGB(entry.textColor)}><nobr>{word}</nobr></color></link>";
            description = description.Replace(word, replacement);
        }

        return description;
    }


    /// <summary>
    /// マージ時のボールの近くにある別のボールを取得する(2つのボールを無視)
    /// </summary>
    public static List<BallBase> GetNearbyBalls(GameObject obj, GameObject other, float radius)
    {
        var result = new List<BallBase>();
        // Ballレイヤーのみをチェックして物理演算負荷を軽減
        int ballLayer = LayerMask.GetMask("Ball");
        var hitColliders = Physics2D.OverlapCircleAll(obj.transform.position, radius, ballLayer);
        // 取得したコライダーをリストに変換
        foreach (var col in hitColliders)
        {
            // 自身を無視
            if (col.gameObject == obj) continue;
            // merge相手を無視
            if (col.gameObject == other) continue;
            
            var ball = col.gameObject.GetComponent<BallBase>();
            if (!ball) continue;
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
        // Ballレイヤーのみをチェックして物理演算負荷を軽減
        int ballLayer = LayerMask.GetMask("Ball");
        var hitColliders = Physics2D.OverlapCircleAll(obj.transform.position, radius, ballLayer);
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
}
