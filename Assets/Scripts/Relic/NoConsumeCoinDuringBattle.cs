using UnityEngine;

/// <summary>
/// バトル中（MergeとEnemyAttack状態）はコイン消費が0になる
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class NoConsumeCoinDuringBattle : RelicBase
{
    protected override void RegisterEffects()
    {
        // バトル中のコイン消費を0にブロック
        RegisterCoinConsumeBlock(
            condition: GameStateCondition(
                GameManager.GameState.Merge,
                GameManager.GameState.EnemyAttack
            )
        );
    }
}