using UnityEngine;

/// <summary>
/// (プレイヤー専用)毎ターン、スタック数に応じてお邪魔ボールが降ってくる
/// </summary>
public class CurseEffect : StatusEffectBase
{
    protected override void OnTurnEndEffect()
    {
        var count = StackCount;
        for (var i = 0; i < count; i++)
        {
            MergeManager.Instance.CreateDisturbBall();
        }
        PlaySoundEffect();
    }
}