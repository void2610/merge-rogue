using UnityEngine;

/// <summary>
/// プレイヤーが受けたダメージを蓄積し、20ダメージ毎にボムボールを生成する
/// </summary>
public class CreateBombWhenDamage : RelicBase
{
    public override void RegisterEffects()
    {
        IsCountable = true; // カウント表示を有効化
        // ダメージ20毎にボム生成
        RegisterDamageAccumulator(
            threshold: 20,
            onThresholdReached: () => MergeManager.Instance.CreateBombBall()
        );
    }
}