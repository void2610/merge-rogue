using UnityEngine;
using R3;

public class IncreaseBallAmountButRandomizeWalls : RelicBase
{
    public override void RegisterEffects()
    {
        // ボール数を1個増加
        MergeManager.Instance?.LevelUpBallAmount();
        
        // ターン開始時に壁をランダムに変更
        AddSubscription(EventManager.OnMergePhaseStart.Subscribe(_ =>
        {
            if (RandomService.RandomRange(0f, 1f) < 0.5f) return;
            MergeManager.Instance?.RandomizeWallWidth();
        }));
    }
}