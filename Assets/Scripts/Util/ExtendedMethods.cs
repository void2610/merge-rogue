using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

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
        
        int totalChars = animator.textInfo.characterCount;
        
        for (var i = 0; i < totalChars; i++)
        {
            // キャンセルが要求されていたらループを抜ける
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            
            // 各文字に初期オフセットと回転を設定
            animator.SetCharOffset(i, new Vector3(0, -5, 0));
            animator.SetCharRotation(i, new Vector3(0, 0, 10));

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
            for (var i = 0; i < totalChars; i++)
            {
                animator.SetCharAlpha(i, 1);
                animator.SetCharOffset(i, Vector3.zero);
                animator.SetCharRotation(i, Vector3.zero);
            }
        }
        return text;
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
    /// DOTweenTMPAnimatorの文字の透明度を設定する
    /// </summary>
    public static DOTweenTMPAnimator SetCharAlpha(this DOTweenTMPAnimator animator, int index, float alpha)
    {
        var color = animator.GetCharColor(index);
        color.a = alpha;
        animator.SetCharColor(index, color);
        return animator;
    }
}
