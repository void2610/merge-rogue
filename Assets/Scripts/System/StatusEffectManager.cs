using System;
using System.Collections.Generic;
using UnityEngine;
using R3;

public class StatusEffectManager : SingletonMonoBehaviour<StatusEffectManager>
{
    private StatusEffectDataList StatusEffectDataList => ContentProvider.Instance.StatusEffectList;
    
    /// <summary>
    /// 状態異常を作成する
    /// </summary>
    public StatusEffectBase CreateStatusEffect(StatusEffectType type, int stackCount = 1)
    {
        var data = StatusEffectDataList.GetStatusEffectData(type);
        if (data == null)
        {
            Debug.LogError($"StatusEffectData not found for type: {type}");
            return null;
        }
        
        var effectType = Type.GetType(data.className);
        if (effectType == null || !effectType.IsSubclassOf(typeof(StatusEffectBase)))
        {
            Debug.LogError($"Invalid StatusEffect class: {data.className}");
            return null;
        }
        
        var effect = (StatusEffectBase)Activator.CreateInstance(effectType);
        effect.Initialize(data, stackCount);
        
        return effect;
    }
    
    /// <summary>
    /// エンティティに状態異常を追加する
    /// </summary>
    public void AddStatusEffect(IEntity target, StatusEffectType type, int stackCount = 1)
    {
        // イベント発火
        if (target is Player)
        {
            EventManager.OnPlayerStatusEffectAdded.OnNext(Unit.Default);
        }
        else if (target is EnemyBase)
        {
            EventManager.OnEnemyStatusEffectAdded.OnNext(type);
        }
        
        var effect = CreateStatusEffect(type, stackCount);
        if (effect != null)
        {
            effect.SetOwner(target);
            target.AddStatusEffect(effect);
        }
    }
    
    /// <summary>
    /// エンティティから状態異常を削除する
    /// </summary>
    public void RemoveStatusEffect(IEntity target, StatusEffectType type, int stackCount = 1)
    {
        target.RemoveStatusEffect(type, stackCount);
    }
    
    /// <summary>
    /// 状態異常のエフェクトテキストを表示する
    /// </summary>
    public void ShowEffectText(StatusEffectType type, Vector3 position, bool isPlayer, int priority = 0)
    {
        var data = StatusEffectDataList.GetStatusEffectData(type);
        if (data == null) return;
        
        var effectText = GetLocalizedName(type) + "!";
        var playerOffset = isPlayer ? 1 : -1;
        var offset = new Vector3(-priority * 0.1f, priority * 0.25f, 0);
        var displayPosition = position + new Vector3(0.8f * playerOffset, 0.2f, 0) + offset;
        
        ParticleManager.Instance.WavyText(effectText, displayPosition, data.effectColor);
        
        // イベント発火
        if (isPlayer)
        {
            EventManager.OnPlayerStatusEffectTriggered.OnNext(type);
        }
        else
        {
            EventManager.OnEnemyStatusEffectTriggered.OnNext(type);
        }
    }
    
    /// <summary>
    /// 状態異常の色を取得する
    /// </summary>
    public Color GetStatusEffectColor(StatusEffectType type)
    {
        if (StatusEffectDataList == null) return GetFallbackColor(type);
        
        var data = StatusEffectDataList.GetStatusEffectData(type);
        return data != null ? data.effectColor : GetFallbackColor(type);
    }
    
    /// <summary>
    /// StatusEffectDataが利用できない場合のフォールバック色取得
    /// </summary>
    private Color GetFallbackColor(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Burn => new Color(1, 0.4f, 0),
            StatusEffectType.Regeneration => new Color(0, 1, 0.3f),
            StatusEffectType.Shield => new Color(0, 0.8f, 1f),
            StatusEffectType.Freeze => new Color(0, 0.5f, 1),
            StatusEffectType.Invincible => new Color(1, 1, 0),
            StatusEffectType.Shock => new Color(0.7f, 0, 0.7f),
            StatusEffectType.Power => new Color(1, 0.3f, 0),
            StatusEffectType.Rage => new Color(1, 0.2f, 0.5f),
            StatusEffectType.Curse => new Color(0.3f, 0.1f, 0.3f),
            StatusEffectType.Confusion => new Color(1, 0.8f, 0.2f),
            _ => Color.white
        };
    }
    
    /// <summary>
    /// 状態異常のローカライズされた名前を取得する
    /// </summary>
    public string GetLocalizedName(StatusEffectType type)
    {
        // StatusEffectDataListが初期化されていない場合の安全な処理
        if (StatusEffectDataList == null)
        {
            return GetFallbackName(type);
        }
        
        var data = StatusEffectDataList.GetStatusEffectData(type);
        if (data == null) return GetFallbackName(type);
        
        // TODO: ローカライゼーションシステムと連携
        // return LocalizationManager.GetLocalizedValue(data.localizationKeyName);
        
        return GetFallbackName(type);
    }
    
    /// <summary>
    /// StatusEffectDataが利用できない場合のフォールバック名前取得
    /// </summary>
    private string GetFallbackName(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Burn => "火傷",
            StatusEffectType.Regeneration => "再生",
            StatusEffectType.Shield => "シールド",
            StatusEffectType.Freeze => "凍結",
            StatusEffectType.Invincible => "無敵",
            StatusEffectType.Shock => "感電",
            StatusEffectType.Power => "パワー",
            StatusEffectType.Rage => "怒り",
            StatusEffectType.Curse => "呪い",
            StatusEffectType.Confusion => "混乱",
            _ => type.ToString()
        };
    }
    
    /// <summary>
    /// 状態異常の説明を取得する
    /// </summary>
    public string GetDescription(StatusEffectType type)
    {
        if (StatusEffectDataList == null) return "";
        
        var data = StatusEffectDataList.GetStatusEffectData(type);
        if (data == null) return "";
        
        // 直接説明文を返す（ローカライゼーション対応時は切り替え可能）
        if (!string.IsNullOrEmpty(data.description))
        {
            return data.description;
        }
        
        // TODO: ローカライゼーションシステムと連携
        // return LocalizationManager.GetLocalizedValue(data.localizationKeyDesc);
        
        return "";
    }
    
    /// <summary>
    /// 状態異常のローカライズされた説明を取得する（後方互換性）
    /// </summary>
    public string GetLocalizedDescription(StatusEffectType type)
    {
        return GetDescription(type);
    }
    
    /// <summary>
    /// 状態異常のアイコンを取得する
    /// </summary>
    public Sprite GetStatusEffectIcon(StatusEffectType type)
    {
        var data = StatusEffectDataList.GetStatusEffectData(type);
        return data != null ? data.icon : null;
    }
}