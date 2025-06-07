using UnityEngine;

/// <summary>
/// バトル中（MergeとEnemyAttack状態）はコイン消費が0になる
/// </summary>
public class NoConsumeCoinDuringBattle : RelicBase
{
    protected override void RegisterEffects()
    {
        // バトル中のコイン消費を0にブロック
        EventManager.OnCoinConsume.AddProcessor(this, current =>
        {
            if (RelicHelpers.GameStateCondition(
                GameManager.GameState.Merge,
                GameManager.GameState.EnemyAttack
            )())
            {
                if (current > 0) // コイン消費をブロックする時のみUIをアクティベート
                {
                    ActivateUI();
                }
                return 0;
            }
            return current;
        });
    }
}