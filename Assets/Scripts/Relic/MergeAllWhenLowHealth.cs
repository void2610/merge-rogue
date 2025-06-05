using UnityEngine;
using R3;

/// <summary>
/// プレイヤーのHPが20以下になったときに全ボールを合成するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class MergeAllWhenLowHealth : RelicBase
{
    protected override void RegisterEffects()
    {
        // プレイヤーダメージ時の条件付き効果を登録
        RegisterPlayerDamageModifier(
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
