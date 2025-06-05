using UnityEngine;
using R3;

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
        
        // 休憩時のイベント購読
        SubscribeRestEnter(OnRestEnter);
    }

    private void OnRestEnter()
    {
        if (Count.Value <= 0) return;
        
        EventManager.OnRest.SetValue(0);
        Count.Value--;
        UI?.ActivateUI();
    }
}
