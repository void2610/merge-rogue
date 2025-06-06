using UnityEngine;
using R3;

/// <summary>
/// 休憩を3回キャンセルできる、ボールレベルアップ量を増加するレリック
/// </summary>
public class MonsterEnergy : RelicBase
{
    public override void Init(RelicUI relicUI)
    {
        IsCountable = true;
        Count.Value = 3;
        base.Init(relicUI);
    }

    protected override void RegisterEffects()
    {
        // ボールレベルアップ量を増加
        MergeManager.Instance?.LevelUpBallAmount();
        
        // 休憩量を0にする（カウントが残っている場合）
        EventManager.RegisterRestModifier(this, ValueProcessors.SetZero(), () => Count.Value > 0);
        
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
