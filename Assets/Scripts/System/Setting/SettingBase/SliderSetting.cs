using System;
using UnityEngine;

/// <summary>
/// スライダー形式の設定項目（音量設定など）
/// </summary>
[System.Serializable]
public class SliderSetting : SettingBase<float>
{
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    
    /// <summary>
    /// 最小値
    /// </summary>
    public float MinValue => minValue;
    
    /// <summary>
    /// 最大値
    /// </summary>
    public float MaxValue => maxValue;
    
    /// <summary>
    /// 現在の値（範囲制限付き）
    /// </summary>
    public override float CurrentValue
    {
        get => currentValue;
        set
        {
            var clampedValue = Mathf.Clamp(value, minValue, maxValue);
            base.CurrentValue = clampedValue;
        }
    }
    
    /// <summary>
    /// ローカライゼーションキーベースのコンストラクタ
    /// </summary>
    public SliderSetting(string localizationKey, float defaultVal, float min, float max) 
        : base(localizationKey, defaultVal)
    {
        minValue = min;
        maxValue = max;
    }
    
    public SliderSetting()
    {
        // シリアライゼーション用のデフォルトコンストラクタ
        minValue = 0f;
        maxValue = 1f;
    }
    
    public override string GetSettingType()
    {
        return "Slider";
    }
    
    /// <summary>
    /// 0-1の正規化された値を取得
    /// </summary>
    public float GetNormalizedValue()
    {
        return Mathf.InverseLerp(minValue, maxValue, CurrentValue);
    }
    
    /// <summary>
    /// 0-1の正規化された値から実際の値を設定
    /// </summary>
    public void SetNormalizedValue(float normalizedValue)
    {
        CurrentValue = Mathf.Lerp(minValue, maxValue, Mathf.Clamp01(normalizedValue));
    }
}