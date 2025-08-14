using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 次の戦闘を休憩に変更するアクション
/// </summary>
[Serializable]
public class NextBattleToRestAction : StageEventActionBase
{
    public override void Execute()
    {
        // 一度だけ実行されるプロセッサーを登録
        var oneTimeUse = false;
        var registrationKey = this;
        
        EventManager.OnStageTypeDecision.AddProcessor(registrationKey, stage =>
        {
            if (!oneTimeUse && stage == StageType.Enemy)
            {
                oneTimeUse = true;
                return StageType.Rest;
            }
            return stage;
        });
    }
    
    public override bool CanExecute()
    {
        return true; // 常に実行可能として扱う
    }
}

/// <summary>
/// ランダムな指輪レリック入手アクション
/// 名前に"Ring"を含むレリックからランダムに1つ取得
/// </summary>
[Serializable]
public class AddRandomRingRelicAction : StageEventActionBase
{
    public override void Execute()
    {
        var contentService = GameManager.Instance.ContentService;
        var relicService = GameManager.Instance.RelicService;
        
        // 全レリックデータから"Ring"を含むものを抽出
        var allRelics = contentService.GetAllRelicData();
        var ringRelics = allRelics.Where(relic => relic.className.Contains("Ring")).ToList();
        
        if (ringRelics.Count > 0)
        {
            var randomIndex = UnityEngine.Random.Range(0, ringRelics.Count);
            var randomRing = ringRelics[randomIndex];
            relicService.AddRelic(randomRing);
        }
    }
    
    public override bool CanExecute()
    {
        var relicService = GameManager.Instance.RelicService;
        return relicService.Relics.Count < relicService.MaxRelics;
    }
}

/// <summary>
/// マージエリアの全ボールをクリアするアクション
/// </summary>
[Serializable]
public class ClearAllBallsAction : StageEventActionBase
{
    public override void Execute()
    {
        MergeManager.Instance.RemoveAllBalls();
    }
    
    public override bool CanExecute()
    {
        return MergeManager.Instance.GetBallCount() > 0;
    }
}
}