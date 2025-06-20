using UnityEngine;
using R3;

/// <summary>
/// 休憩を3回キャンセルできる、ボールレベルアップ量を増加するレリック
/// </summary>
public class MonsterEnergy : RelicBase
{
    public override void RegisterEffects()
    {
        IsCountable = true;
        Count.Value = 3;
        // ボールレベルアップ量を増加
        MergeManager.Instance?.LevelUpBallAmount();
        
        // 休憩量を0にする（カウントが残っている場合）
        EventManager.OnRest.AddProcessor(this, ValueProcessors.SetZero(), () => Count.Value > 0);
        
        // 休憩時にカウント減少
        AddSubscription(RelicHelpers.SubscribeRestEnter(() =>
        {
            if (Count.Value > 0)
            {
                Count.Value--;
                UI?.ActivateUI();
            }
        }));
    }
}
