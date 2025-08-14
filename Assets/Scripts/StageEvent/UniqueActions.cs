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

/// <summary>
/// ギャンブラーイベント用の投資アクション
/// 全コインを預けて投資する
/// </summary>
[Serializable]
public class GamblerInvestmentAction : StageEventActionBase
{
    private const string GAMBLER_COIN_KEY = "gamblerCoin";
    
    public override void Execute()
    {
        var currentCoin = (int)GameManager.Instance.Coin.Value;
        
        // 既存の投資額を取得（初回は0）
        var previousInvestment = Register.GetInt(GAMBLER_COIN_KEY) ?? 0;
        
        // 全コインを投資
        GameManager.Instance.SubCoin(currentCoin);
        
        // 投資額を累積して保存
        Register.RegisterInt(GAMBLER_COIN_KEY, previousInvestment + currentCoin);
    }
    
    public override bool CanExecute()
    {
        return GameManager.Instance.Coin.Value > 0;
    }
}

/// <summary>
/// ギャンブラーイベント用の回収アクション
/// 50%の確率で投資額の2倍を返すか、何も返さない
/// </summary>
[Serializable]
public class GamblerCollectAction : StageEventActionBase
{
    private const string GAMBLER_COIN_KEY = "gamblerCoin";
    
    public override void Execute()
    {
        var investmentAmount = Register.GetInt(GAMBLER_COIN_KEY) ?? 0;
        
        // 50%の確率で成功/失敗を決定
        var isSuccess = UnityEngine.Random.Range(0f, 1f) >= 0.5f;
        
        if (isSuccess)
        {
            // 成功：投資額の2倍を返す
            GameManager.Instance.AddCoin(investmentAmount * 2);
        }
        // 失敗時は何も返さない
        
        // 投資情報をリセット
        Register.RemoveInt(GAMBLER_COIN_KEY);
    }
    
    public override bool CanExecute()
    {
        // 投資履歴があるかチェック
        return Register.ContainsInt(GAMBLER_COIN_KEY) && (Register.GetInt(GAMBLER_COIN_KEY) ?? 0) > 0;
    }
}