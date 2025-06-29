using UnityEngine;

/// <summary>
/// プレイヤーが受けたダメージを蓄積し、5ダメージ毎にコイン1枚を獲得する
/// </summary>
public class ReverseAlchemy : RelicBase
{
    public override void RegisterEffects()
    {
        IsCountable = true; // カウント表示を有効化
        // ダメージ5毎にコイン1枚獲得
        RegisterDamageAccumulator(
            threshold: 5,
            onThresholdReached: () => GameManager.Instance.AddCoin(1)
        );
    }
}