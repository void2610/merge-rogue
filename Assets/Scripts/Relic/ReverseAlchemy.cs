using UnityEngine;

/// <summary>
/// プレイヤーが受けたダメージを蓄積し、5ダメージ毎にコイン1枚を獲得する
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class ReverseAlchemy : RelicBase
{
    public override void Init(RelicUI relicUI)
    {
        IsCountable = true; // カウント表示を有効化
        base.Init(relicUI);
    }

    protected override void RegisterEffects()
    {
        // ダメージ5毎にコイン1枚獲得
        RegisterDamageAccumulator(
            threshold: 5,
            onThresholdReached: () => GameManager.Instance.AddCoin(1)
        );
    }
}