using System;
using UnityEngine;

/// <summary>
/// テキスト入力形式の設定項目（シード値など）
/// </summary>
[System.Serializable]
public class TextInputSetting : SettingBase<string>
{
    [SerializeField] private int maxLength = 50;
    [SerializeField] private string placeholder = "";
    
    /// <summary>
    /// 最大文字数
    /// </summary>
    public int MaxLength => maxLength;
    
    /// <summary>
    /// プレースホルダーテキスト
    /// </summary>
    public string Placeholder
    {
        get
        {
            if (!string.IsNullOrEmpty(localizationKey))
            {
                return LocalizeStringLoader.Instance?.Get($"{localizationKey}_PLACEHOLDER") ?? placeholder;
            }
            return placeholder;
        }
    }
    
    /// <summary>
    /// 現在の値（文字数制限付き）
    /// </summary>
    public override string CurrentValue
    {
        get => currentValue ?? "";
        set
        {
            var trimmedValue = value?.Trim() ?? "";
            if (trimmedValue.Length > maxLength)
            {
                trimmedValue = trimmedValue.Substring(0, maxLength);
            }
            base.CurrentValue = trimmedValue;
        }
    }
    
    public TextInputSetting(string name, string desc, string defaultVal, int maxLen = 50, string placeholderText = "") 
        : base(name, desc, defaultVal ?? "")
    {
        maxLength = maxLen;
        placeholder = placeholderText;
    }
    
    /// <summary>
    /// ローカライゼーションキーベースのコンストラクタ
    /// </summary>
    public TextInputSetting(string localizationKey, string defaultVal, int maxLen = 50) 
        : base(localizationKey, defaultVal ?? "")
    {
        maxLength = maxLen;
        placeholder = localizationKey + "_PLACEHOLDER"; // フォールバック用
    }
    
    public TextInputSetting()
    {
        // シリアライゼーション用のデフォルトコンストラクタ
        maxLength = 50;
        placeholder = "";
    }
    
    public override string GetSettingType()
    {
        return "TextInput";
    }
    
    /// <summary>
    /// 空文字列かどうか判定
    /// </summary>
    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(CurrentValue);
    }
}