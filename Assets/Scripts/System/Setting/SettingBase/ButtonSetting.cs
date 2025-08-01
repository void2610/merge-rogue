using System;
using R3;
using UnityEngine;

/// <summary>
/// ボタン形式の設定項目（セーブデータ削除など）
/// </summary>
[System.Serializable]
public class ButtonSetting : SettingBase<Unit>
{
    [SerializeField] private bool requiresConfirmation;
    
    /// <summary>
    /// ボタンに表示するテキスト
    /// </summary>
    public string ButtonText
    {
        get
        {
            if (!string.IsNullOrEmpty(localizationKey))
            {
                return LocalizeStringLoader.Instance?.Get($"{localizationKey}_BUTTON") ?? $"{localizationKey}_BUTTON";
            }
            return $"{localizationKey}_BUTTON";
        }
    }
    
    /// <summary>
    /// 確認ダイアログが必要かどうか
    /// </summary>
    public bool RequiresConfirmation => requiresConfirmation;
    
    /// <summary>
    /// 確認ダイアログのメッセージ
    /// </summary>
    public string ConfirmationMessage
    {
        get
        {
            if (!string.IsNullOrEmpty(localizationKey))
            {
                return LocalizeStringLoader.Instance?.Get($"{localizationKey}_CONFIRM") ?? $"{localizationKey}_CONFIRM";
            }
            return $"{localizationKey}_CONFIRM";
        }
    }
    
    /// <summary>
    /// ボタンアクション実行用のデリゲート
    /// </summary>
    public Action ButtonAction { get; set; }
    
    /// <summary>
    /// ローカライゼーションキーベースのコンストラクタ
    /// </summary>
    public ButtonSetting(string localizationKey, bool needsConfirmation = false) 
        : base(localizationKey, Unit.Default)
    {
        requiresConfirmation = needsConfirmation;
    }
    
    public ButtonSetting()
    {
        // シリアライゼーション用のデフォルトコンストラクタ
    }
    
    /// <summary>
    /// ボタンがクリックされた時に呼び出される
    /// </summary>
    public void ExecuteAction()
    {
        CurrentValue = Unit.Default; // これでOnValueChangedが発火する
        
        try
        {
            ButtonAction?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"ButtonSetting {localizationKey} のアクション実行中にエラー: {e.Message}");
        }
    }
    
    public override string GetSettingType()
    {
        return "Button";
    }
}