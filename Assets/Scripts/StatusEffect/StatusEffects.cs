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
        StatusEffectManager.Instance?.AddStatusEffect(target, type, stackCount);
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
        StatusEffectManager.Instance?.RemoveStatusEffect(target, type, stackCount);
    }
    
    /// <summary>
    /// 状態異常の色を取得する
    /// </summary>
    public static Color GetColor(StatusEffectType type)
    {
        return StatusEffectManager.Instance?.GetStatusEffectColor(type) ?? Color.white;
    }
    
    /// <summary>
    /// 状態異常のローカライズされた名前を取得する
    /// </summary>
    public static string GetLocalizedName(StatusEffectType type)
    {
        return StatusEffectManager.Instance?.GetLocalizedName(type) ?? type.ToString();
    }
    
    /// <summary>
    /// 状態異常の説明を取得する
    /// </summary>
    public static string GetDescription(StatusEffectType type)
    {
        return StatusEffectManager.Instance?.GetDescription(type) ?? "";
    }
}