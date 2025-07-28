using System;
using R3;
using UnityEngine;

/// <summary>
/// ボタン形式の設定項目（セーブデータ削除など）
/// </summary>
[System.Serializable]
public class ButtonSetting : SettingBase<Unit>
{
    [SerializeField] private string buttonText;
    [SerializeField] private bool requiresConfirmation;
    [SerializeField] private string confirmationMessage;
    
    /// <summary>
    /// ボタンに表示するテキスト
    /// </summary>
    public string ButtonText => buttonText;
    
    /// <summary>
    /// 確認ダイアログが必要かどうか
    /// </summary>
    public bool RequiresConfirmation => requiresConfirmation;
    
    /// <summary>
    /// 確認ダイアログのメッセージ
    /// </summary>
    public string ConfirmationMessage => confirmationMessage;
    
    /// <summary>
    /// ボタンアクション実行用のデリゲート
    /// </summary>
    public Action ButtonAction { get; set; }
    
    public ButtonSetting(string name, string desc, string btnText, bool needsConfirmation = false, string confirmMsg = "") 
        : base(name, desc, Unit.Default)
    {
        buttonText = btnText;
        requiresConfirmation = needsConfirmation;
        confirmationMessage = confirmMsg;
    }
    
    public ButtonSetting()
    {
        // シリアライゼーション用のデフォルトコンストラクタ
        buttonText = "実行";
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
            Debug.LogError($"ButtonSetting {settingName} のアクション実行中にエラー: {e.Message}");
        }
    }
    
    public override string GetSettingType()
    {
        return "Button";
    }
}