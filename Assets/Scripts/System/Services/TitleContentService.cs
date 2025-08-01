using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TitleScene専用の軽量ContentServiceの実装
/// DescriptionWindowで必要な最小限の機能のみを提供
/// </summary>
public class TitleContentService : IContentService
{
    private readonly ContentProviderData _data;

    public int Act => 0;
    public StatusEffectDataList StatusEffectList => _data?.StatusEffectList;
    public float GlobalEnemyDifficultyMultiplier => 1.0f;
    public float BaseEnemyHealthMultiplier => _data?.BaseEnemyHealthMultiplier ?? 1.0f;
    public float BaseEnemyAttackMultiplier => _data?.BaseEnemyAttackMultiplier ?? 1.0f;
    public bool IsDemoPlay { get; } = false;

    public TitleContentService(ContentProviderData data)
    {
        _data = data;
        _data?.InitializeData();
        
        # if DEMO_PLAY
        IsDemoPlay = true;
        # endif
    }

    // DescriptionWindowで使用される唯一のメソッド
    public int GetShopPrice(Shop.ShopItemType type, Rarity rarity)
    {
        return _data?.GetShopPrice(type, rarity) ?? 0;
    }

    public int GetBallRemovePrice() => _data?.BallRemovePrice ?? 0;
    public int GetBallUpgradePrice() => _data?.BallUpgradePrice ?? 0;

    // 以下は未実装（TitleSceneでは使用されない）
    public Type GetRandomEventType() => throw new NotSupportedException("TitleScene does not support random events");
    public EnemyData GetRandomEnemy() => throw new NotSupportedException("TitleScene does not support random enemies");
    public EnemyData GetRandomBoss() => throw new NotSupportedException("TitleScene does not support random bosses");
    public RelicData GetRandomRelic() => throw new NotSupportedException("TitleScene does not support random relics");
    public Rarity GetRandomRarity() => throw new NotSupportedException("TitleScene does not support random rarity");
    public List<BallData> GetBallListExceptNormal() => throw new NotSupportedException("TitleScene does not support ball lists");
    public BallData GetNormalBallData() => throw new NotSupportedException("TitleScene does not support ball data");
    public BallData GetBallDataFromClassName(string className) => throw new NotSupportedException("TitleScene does not support ball data");
    public RelicData GetRelicByClassName(string className) => throw new NotSupportedException("TitleScene does not support relic data");
    public List<RelicData> GetRelicDataByRarity(Rarity rarity) => throw new NotSupportedException("TitleScene does not support relic data");
    public void AddAct() => throw new NotSupportedException("TitleScene does not support act progression");
    public void SetShopPriceMultiplier(float multiplier) => throw new NotSupportedException("TitleScene does not support price multiplier");
    public void SetGlobalEnemyDifficultyMultiplier(float multiplier) => throw new NotSupportedException("TitleScene does not support difficulty multiplier");
}