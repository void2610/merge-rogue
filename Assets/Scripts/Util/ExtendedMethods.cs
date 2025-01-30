using DG.Tweening;
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
}
