using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ExtendedMethods
{
    /// <summary>
    /// stringのListの値を全てLogする
    /// </summary>
    public static void Print<T>(this List<T> list)
    {
        foreach (var s in list)
        {
            Debug.Log(s);
        }
    }
    
    /// <summary>
    /// enumを順番に切り替えた結果を返す 
    /// </summary>
    public static T Toggle<T>(this T current) where T : System.Enum
    {
        var values = (T[])System.Enum.GetValues(current.GetType());
        var index = System.Array.IndexOf(values, current);
        var nextIndex = (index + 1) % values.Length;
        return values[nextIndex];
    }

    /// <summary>
    /// Selectableのリストに対してナビゲーションを設定する
    /// </summary>
    public static void SetNavigation(this List<Selectable> selectables, bool isHorizontal = true)
    {
        for (var i = 0; i < selectables.Count; i++)
        {
            var selectable = selectables[i];
            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;

            if (isHorizontal)
            {
                navigation.selectOnLeft = i == 0 ? null : selectables[i - 1];
                navigation.selectOnRight = i == selectables.Count - 1 ? null : selectables[i + 1];
            }
            else
            {
                navigation.selectOnUp = i == 0 ? null : selectables[i - 1];
                navigation.selectOnDown = i == selectables.Count - 1 ? null : selectables[i + 1];
            }

            selectable.navigation = navigation;
        }
    }
    
    /// <summary>
    /// DOTweenの警告を無視する拡張メソッド
    /// </summary>
    public static void Forget(this Tween tween)
    {
        // 何もせず警告を無視
    }
    
    /// <summary>
    /// TextMeshProUGUIの文字をフェードインさせる
    /// </summary>
    public static async UniTask<TextMeshProUGUI> ShowTextTween(this TextMeshProUGUI text, float duration = 0.1f, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text.text)) return text;
        
        text.alpha = 0;
        var animator = new DOTweenTMPAnimator(text);
        
        if (animator.textInfo.characterCount == 0) return text;
        
        var totalChars = animator.textInfo.characterCount;
        
        for (var i = 0; i < totalChars; i++)
        {
            // キャンセルが要求されていたらループを抜ける
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            // 各文字に初期オフセットと回転を設定
            animator?.SetCharOffset(i, new Vector3(0, -5, 0));
            animator?.SetCharRotation(i, new Vector3(0, 0, 10));

            // 改行の場合は待機時間を長めに
            if (text.text[i] == '\n')
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(duration * 5), cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            // Tweenを生成：透明度、位置（オフセット）、回転を最終状態（alpha=1, offset=0, rotation=0）にアニメーション
            var fadeTween = animator.DOFadeChar(i, 1, duration);
            var moveTween = animator.DOOffsetChar(i, Vector3.zero, duration);
            var rotateTween = animator.DORotateChar(i, Vector3.zero, duration);
            
            if (fadeTween == null || moveTween == null || rotateTween == null) continue;
            
            try
            {
                await UniTask.WhenAll(
                    fadeTween.ToUniTask(cancellationToken: cancellationToken),
                    moveTween.ToUniTask(cancellationToken: cancellationToken),
                    rotateTween.ToUniTask(cancellationToken: cancellationToken)
                );
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        
        // キャンセルが要求された場合は、残りの文字に対して即時に最終状態を適用する
        if (cancellationToken.IsCancellationRequested)
        {
            // 現在のTweenを全て停止
            DOTween.Kill(text);
            animator.ResetAllChars();
        }
        return text;
    }

    public static void AddDescriptionWindowEvent(this GameObject g, object data, int level = 0)
    {
        if (!g || data == null) Debug.LogError("AddDescriptionWindowEvent: g or data is null");
        if(data is not BallData && data is not RelicData) Debug.LogError("AddDescriptionWindowEvent: data is not BallData or RelicData");

        ShowDescription d;
        if (!g.TryGetComponent<ShowDescription>(out d))
            d = g.AddComponent<ShowDescription>();
        
        if (data is BallData ballData)
        {
            Utils.AddEventToObject(g, () => { 
                UIManager.Instance.ShowBallDescriptionWindow(ballData, g, level);
            }, EventTriggerType.PointerEnter);
            
            d.isBall = true;
            d.ballData = ballData; 
            d.level = level;
        }
        else if (data is RelicData relicData)
        {
            Utils.AddEventToObject(g, () => { 
                UIManager.Instance.ShowRelicDescriptionWindow(relicData, g);
            }, EventTriggerType.PointerEnter);
            
            d.isBall = false;
            d.relicData = relicData;
        }
    }
    
    public static void AddSubDescriptionWindowEvent(this GameObject g, string word)
    {
        if (!g || string.IsNullOrEmpty(word)) Debug.LogError("AddSubDescriptionWindowEvent: g or word is null");
        
        ShowSubDescription d;
        if (!g.TryGetComponent<ShowSubDescription>(out d))
            d = g.AddComponent<ShowSubDescription>();
        
        Utils.AddEventToObject(g, () => { 
            DescriptionWindow.Instance.ShowSubWindow(g, word);
        }, EventTriggerType.PointerEnter);
        
        d.word = word;
    }
    
    public static void RemoveAllEventTrigger(this GameObject g)
    {
        if (!g) return;
        var eventTrigger = g.GetComponent<EventTrigger>();
        if (!eventTrigger) return;
        
        foreach (var entry in eventTrigger.triggers)
        {
            eventTrigger.triggers.Remove(entry);
        }
    }
    
    /// <summary>
    /// 辞書の全ての要素に掛け算を行う
    /// </summary>
    public static Dictionary<AttackType, int> MultiplyAll(this Dictionary<AttackType, int> dict, float value)
    {
        foreach (var key in dict.Keys.ToList())
        {
            dict[key] = (int)(dict[key] * value);
        }
        return dict;
    }

    /// <summary>
    /// DOTweenTMPAnimatorの全ての文字のアニメーションをリセットする
    /// </summary>
    public static DOTweenTMPAnimator ResetAllChars(this DOTweenTMPAnimator animator)
    {
        animator.SetAllCharsAlpha(1);
        animator.SetAllCharOffsets(Vector2.zero);
        animator.SetAllCharRotations(Vector3.zero);
        return animator;
    }
    
    /// <summary>
    /// DOTweenTMPAnimatorの文字の透明度を設定する
    /// </summary>
    public static DOTweenTMPAnimator SetCharAlpha(this DOTweenTMPAnimator animator, int index, float alpha)
    {
        var color = animator.GetCharColor(index);
        color.a = alpha;
        animator.SetCharColor(index, color);
        return animator;
    }

    /// <summary>
    /// DOTweenTMPAnimatorの全ての文字の透明度を設定する
    /// </summary>
    public static DOTweenTMPAnimator SetAllCharsAlpha(this DOTweenTMPAnimator animator, float alpha)
    {
        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
            var color = animator.GetCharColor(i);
            color.a = alpha;
            animator.SetCharColor(i, color);
        }

        return animator;
    }
    
    /// <summary>
    /// DOTweenTMPAnimatorの全ての文字の位置を設定する
    /// </summary>
    public static DOTweenTMPAnimator SetAllCharOffsets(this DOTweenTMPAnimator animator, Vector2 offset)
    {
        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
            animator.SetCharOffset(i, offset);
        }

        return animator;
    }

    /// <summary>
    /// DOTweenTMPAnimatorの全ての文字の角度を設定する
    /// </summary>
    public static DOTweenTMPAnimator SetAllCharRotations(this DOTweenTMPAnimator animator, Vector2 rotation)
    {
        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
            animator.SetCharRotation(i, rotation);
        }

        return animator;
    }

    /// <summary>
    /// DOTweenTMPAnimatorの全ての文字の大きさを設定する
    /// </summary>
    public static DOTweenTMPAnimator SetAllCharScales(this DOTweenTMPAnimator animator, float scale)
    {
        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
            animator.SetCharScale(i, Vector3.one * scale);
        }

        return animator;
    }
    
    /// <summary>
    /// DOTweenTMPAnimatorの全ての文字を揺らす
    /// </summary>
    public static void WobbleChars(this DOTweenTMPAnimator animator, Vector2 sinAmplitude, Vector2 sinFrequency)
    {
        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
            var times = sinFrequency * ((Time.timeSinceLevelLoad + i) * 2 * Mathf.PI);
            var offset = sinAmplitude * new Vector2(Mathf.Sin(times.x), Mathf.Sin(times.y));
            animator.SetCharOffset(i, offset);
        }
    }
}
