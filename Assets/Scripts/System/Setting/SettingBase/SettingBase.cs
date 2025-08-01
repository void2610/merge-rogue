using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

/// <summary>
/// 設定項目の基底クラス（ジェネリック版）
/// SubclassSelectorで拡張可能な設計
/// </summary>
[System.Serializable]
public abstract class SettingBase<T> : ISettingBase
{
    [SerializeField] protected T currentValue;
    [SerializeField] protected T defaultValue;
    [SerializeField] protected string localizationKey; // ローカライゼーションキーのベース部分
    
    private readonly Subject<Unit> _onSettingChanged = new();
    private readonly Subject<T> _onValueChanged = new();
    
    /// <summary>
    /// 設定項目の名前
    /// </summary>
    public string SettingName
    {
        get
        {
            if (!string.IsNullOrEmpty(localizationKey))
            {
                return LocalizeStringLoader.Instance?.Get($"{localizationKey}_NAME") ?? localizationKey;
            }
            return localizationKey;
        }
    }
    
    /// <summary>
    /// 設定項目の説明
    /// </summary>
    public string Description
    {
        get
        {
            if (!string.IsNullOrEmpty(localizationKey))
            {
                return LocalizeStringLoader.Instance?.Get($"{localizationKey}_DESC") ?? $"{localizationKey}_DESC";
            }
            return $"{localizationKey}_DESC";
        }
    }
    
    /// <summary>
    /// ローカライゼーションキー（設定検索用）
    /// </summary>
    public string LocalizeKey => localizationKey;
    
    /// <summary>
    /// 現在の値
    /// </summary>
    public virtual T CurrentValue
    {
        get => currentValue;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(currentValue, value))
            {
                currentValue = value;
                _onValueChanged.OnNext(value);
                _onSettingChanged.OnNext(Unit.Default);
            }
        }
    }
    
    /// <summary>
    /// デフォルト値
    /// </summary>
    public T DefaultValue => defaultValue;
    
    /// <summary>
    /// 値が変更された時のイベント（型付き）
    /// </summary>
    public Observable<T> OnValueChanged => _onValueChanged;
    
    /// <summary>
    /// 設定が変更された時のイベント（Unit型、通知用）
    /// </summary>
    public Observable<Unit> OnSettingChanged => _onSettingChanged;
    
    /// <summary>
    /// ローカライゼーションキーベースのコンストラクタ
    /// </summary>
    protected SettingBase(string localizationKey, T defaultVal)
    {
        this.localizationKey = localizationKey;
        defaultValue = defaultVal;
        currentValue = defaultVal;
    }
    
    protected SettingBase()
    {
        // シリアライゼーション用のデフォルトコンストラクタ
    }
    
    /// <summary>
    /// 設定値をデフォルト値にリセット
    /// </summary>
    public virtual void ResetToDefault()
    {
        CurrentValue = defaultValue;
    }
    
    /// <summary>
    /// 設定値をJSON形式でシリアライズ
    /// </summary>
    public virtual string SerializeValue()
    {
        return JsonUtility.ToJson(new SerializableValue<T> { value = currentValue });
    }
    
    /// <summary>
    /// JSON形式から設定値をデシリアライズ
    /// </summary>
    public virtual void DeserializeValue(string json)
    {
        try
        {
            var data = JsonUtility.FromJson<SerializableValue<T>>(json);
            CurrentValue = data.value;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Setting {localizationKey} のデシリアライズに失敗: {e.Message}");
            ResetToDefault();
        }
    }
    
    /// <summary>
    /// 設定値を現在の値で初期化（起動時などに使用）
    /// </summary>
    public virtual void ApplyCurrentValue()
    {
        _onValueChanged.OnNext(currentValue);
        _onSettingChanged.OnNext(Unit.Default);
    }
    
    /// <summary>
    /// 設定の種類を取得（デバッグ・UI表示用）
    /// </summary>
    public abstract string GetSettingType();
    
    [System.Serializable]
    private class SerializableValue<TValue>
    {
        public TValue value;
    }
}

/// <summary>
/// 非ジェネリック インターフェース（SettingsManagerでの統一管理用）
/// </summary>
public interface ISettingBase
{
    string SettingName { get; }
    string Description { get; }
    string LocalizeKey { get; }
    Observable<Unit> OnSettingChanged { get; }
    void ResetToDefault();
    string SerializeValue();
    void DeserializeValue(string json);
    void ApplyCurrentValue();
    string GetSettingType();
}