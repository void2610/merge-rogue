using UnityEngine;
using R3;

/// <summary>
/// プレイヤーのHPが20以下になったときに全ボールを合成するレリック
/// </summary>
public class MergeAllWhenLowHealth : RelicBase
{
    public override void RegisterEffects()
    {
        // プレイヤーダメージ時の条件付き効果を登録
        RelicHelpers.RegisterPlayerDamageModifier(this,
            current =>
            {
                if (GameManager.Instance?.Player != null && 
                    GameManager.Instance.Player.Health.Value <= 20)
                {
                    MergeManager.Instance?.MergeAll();
                    UI?.ActivateUI();
                }
                return current; // ダメージ値は変更しない
            }
        );
    }
}
