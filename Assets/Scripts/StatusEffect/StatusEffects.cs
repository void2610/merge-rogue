using UnityEngine;

/// <summary>
/// 状態異常の適用・削除を管理する静的クラス
/// </summary>
public static class StatusEffects
{
    /// <summary>
    /// プレイヤーに状態異常を追加する
    /// </summary>
    public static void AddToPlayer(StatusEffectType type, int stackCount = 1)
    {
        AddToEntity(GameManager.Instance.Player, type, stackCount);
    }
    
    /// <summary>
    /// エンティティに状態異常を追加する
    /// </summary>
    public static void AddToEntity(IEntity target, StatusEffectType type, int stackCount = 1)
    {
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager is not initialized");
            return;
        }
        
        StatusEffectManager.Instance.AddStatusEffect(target, type, stackCount);
    }
    
    /// <summary>
    /// プレイヤーから状態異常を削除する
    /// </summary>
    public static void RemoveFromPlayer(StatusEffectType type, int stackCount = 1)
    {
        RemoveFromEntity(GameManager.Instance.Player, type, stackCount);
    }
    
    /// <summary>
    /// エンティティから状態異常を削除する
    /// </summary>
    public static void RemoveFromEntity(IEntity target, StatusEffectType type, int stackCount = 1)
    {
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager is not initialized");
            return;
        }
        
        StatusEffectManager.Instance.RemoveStatusEffect(target, type, stackCount);
    }
    
    /// <summary>
    /// 状態異常の色を取得する
    /// </summary>
    public static Color GetColor(StatusEffectType type)
    {
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager is not initialized");
            return Color.white;
        }
        
        return StatusEffectManager.Instance.GetStatusEffectColor(type);
    }
    
    /// <summary>
    /// 状態異常のローカライズされた名前を取得する
    /// </summary>
    public static string GetLocalizedName(StatusEffectType type)
    {
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager is not initialized");
            return type.ToString();
        }
        
        return StatusEffectManager.Instance.GetLocalizedName(type);
    }
    
    /// <summary>
    /// 状態異常の説明を取得する
    /// </summary>
    public static string GetDescription(StatusEffectType type)
    {
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager is not initialized");
            return "";
        }
        
        return StatusEffectManager.Instance.GetDescription(type);
    }
    
    /// <summary>
    /// 状態異常のローカライズされた説明を取得する（後方互換性）
    /// </summary>
    public static string GetLocalizedDescription(StatusEffectType type)
    {
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager is not initialized");
            return "";
        }
        
        return StatusEffectManager.Instance.GetLocalizedDescription(type);
    }
    
    /// <summary>
    /// 状態異常のアイコンを取得する
    /// </summary>
    public static Sprite GetIcon(StatusEffectType type)
    {
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager is not initialized");
            return null;
        }
        
        return StatusEffectManager.Instance.GetStatusEffectIcon(type);
    }
}