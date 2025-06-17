using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class StatusEffectManager : SingletonMonoBehaviour<StatusEffectManager>
{
    private static StatusEffectDataList StatusEffectDataList => ContentProvider.Instance.StatusEffectList;
    
    /// <summary>
    /// エンティティに状態異常を追加する
    /// </summary>
    public void AddStatusEffect(IEntity target, StatusEffectType type, int stackCount = 1)
    {
        // イベント発火
        if (target is Player)
            EventManager.OnPlayerStatusEffectAdded.OnNext(Unit.Default);
        else if (target is EnemyBase)
            EventManager.OnEnemyStatusEffectAdded.OnNext(type);
        
        StatusEffectProcessor.AddStatusEffect(target, type, stackCount);
    }
    
    /// <summary>
    /// エンティティから状態異常を削除する
    /// </summary>
    public void RemoveStatusEffect(IEntity target, StatusEffectType type, int stackCount = 1)
    {
        StatusEffectProcessor.RemoveStatusEffect(target, type, stackCount);
    }
    
    /// <summary>
    /// 状態異常のエフェクトテキストを表示する
    /// </summary>
    public void ShowEffectText(StatusEffectType type, Vector3 position, bool isPlayer, int priority = 0)
    {
        var effectColor = StatusEffectDataList.GetStatusEffectData(type).effectColor;
        var effectText = GetLocalizedName(type) + "!";
        var playerOffset = isPlayer ? 1 : -1;
        var offset = new Vector3(-priority * 0.1f, priority * 0.25f, 0);
        var displayPosition = position + new Vector3(0.8f * playerOffset, 0.2f, 0) + offset;

        ParticleManager.Instance.WavyText(effectText, displayPosition, effectColor);
        
        // イベント発火
        if (isPlayer) EventManager.OnPlayerStatusEffectTriggered.OnNext(type);
        else EventManager.OnEnemyStatusEffectTriggered.OnNext(type);
    }
    
    /// <summary>
    /// 状態異常の色を取得する
    /// </summary>
    public Color GetStatusEffectColor(StatusEffectType type)
    {
        return StatusEffectDataList.GetStatusEffectData(type).effectColor;
    }
    
    /// <summary>
    /// 状態異常のローカライズされた名前を取得する
    /// </summary>
    public string GetLocalizedName(StatusEffectType type)
    {
        // TODO: ローカライゼーションシステムと連携
        // return LocalizationManager.GetLocalizedValue(data.localizationKeyName);
        
        return StatusEffectDataList.GetStatusEffectData(type)?.name ?? type.ToString();
    }
    
    /// <summary>
    /// 状態異常の説明を取得する
    /// </summary>
    public string GetDescription(StatusEffectType type)
    {
        // TODO: ローカライゼーションシステムと連携
        // return LocalizationManager.GetLocalizedValue(data.localizationKeyDesc);
        
        return StatusEffectDataList.GetStatusEffectData(type)?.description ?? "";
    }
}