using UnityEngine;
using R3;
using SafeEventSystem;

/// <summary>
/// 休憩を3回キャンセルできる、ボールレベルアップ量を増加するレリック
/// 新しい安全なイベントシステムを使用したバージョン
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
        var mod = new OverrideModifier(0, this, () => Count.Value > 0);
        _intModifiers.Add(mod);
        SafeEventManager.RegisterRestModifier(mod);
        
        // 休憩時にカウント減少
        SubscribeRestEnter(() =>
        {
            if (Count.Value > 0)
            {
                Count.Value--;
                UI?.ActivateUI();
            }
        });
    }
}
