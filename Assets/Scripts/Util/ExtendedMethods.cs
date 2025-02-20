using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public static class ExtendedMethods
{
    /// <summary>
    /// stringのListの値を全てLogする
    /// </summary>
    public static void Print(this System.Collections.Generic.List<string> list)
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
    public static async UniTask<TextMeshProUGUI> ShowTextTween(this TextMeshProUGUI text, float duration = 0.1f)
    {
        if (string.IsNullOrEmpty(text.text)) return text;
        
        text.alpha = 0;
        var animator = new DOTweenTMPAnimator(text);
        
        if (animator.textInfo.characterCount == 0) return text;

        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            // 初期オフセット（少し下にずらす）
            animator.SetCharOffset(i, new Vector3(0, -5, 0));
            animator.SetCharRotation(i, new Vector3(0, 0, 10));

            // 改行の場合は待機時間を長くする
            if (text.text[i] == '\n') await UniTask.Delay(TimeSpan.FromSeconds(duration * 5));

            // 透明度とY座標を上昇させるアニメーションを同時に実行
            var fadeTween = animator.DOFadeChar(i, 1, duration);
            var moveTween = animator.DOOffsetChar(i, Vector3.zero, duration);
            var rotateTween = animator.DORotateChar(i, Vector3.zero, duration);
            
            if (fadeTween == null || moveTween == null || rotateTween == null) continue;

            await UniTask.WhenAll(fadeTween.ToUniTask(), moveTween.ToUniTask(), rotateTween.ToUniTask());
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
}
