using UnityEngine;

/// <summary>
/// プレイヤーが受けたダメージを蓄積し、20ダメージ毎にボムボールを生成する
/// </summary>
public class CreateBombWhenDamage : RelicBase
{
    public override void Init(RelicUI relicUI)
    {
        IsCountable = true; // カウント表示を有効化
        base.Init(relicUI);
    }

    protected override void RegisterEffects()
    {
        // ダメージ20毎にボム生成
        RegisterDamageAccumulator(
            threshold: 20,
            onThresholdReached: () => MergeManager.Instance.CreateBombBall()
        );
    }
}