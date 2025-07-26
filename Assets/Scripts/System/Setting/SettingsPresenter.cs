using System;
using System.Linq;
using R3;
using VContainer.Unity;
using Object = UnityEngine.Object;

/// <summary>
/// SettingsManagerとSettingsViewの橋渡しを行うPresenterクラス
/// MVPパターンに基づいてViewとModelを分離
/// </summary>
public class SettingsPresenter : IStartable, IDisposable
{
    private SettingsView _settingsView;
    private readonly SettingsManager _settingsManager;
    private readonly CompositeDisposable _disposables = new();
    
    public SettingsPresenter(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }
    
    public void Start()
    {
        _settingsView = Object.FindFirstObjectByType<SettingsView>();
        
        SubscribeToViewEvents();
        RefreshSettingsView();
    }
    
    /// <summary>
    /// ViewのイベントをSettingsManagerに接続
    /// </summary>
    private void SubscribeToViewEvents()
    {
        // スライダー変更イベント
        _settingsView.OnSliderChanged
            .Subscribe(data => {
                var setting = _settingsManager.GetSetting<SliderSetting>(data.settingName);
                if (setting != null) setting.CurrentValue = data.value;
            })
            .AddTo(_disposables);
        
        // 列挙型変更イベント
        _settingsView.OnEnumChanged
            .Subscribe(data => {
                var setting = _settingsManager.GetSetting<EnumSetting>(data.settingName);
                if (setting != null) setting.CurrentValue = data.value;
            })
            .AddTo(_disposables);
        
        // ボタンクリックイベント
        _settingsView.OnButtonClicked
            .Subscribe(settingName => {
                var setting = _settingsManager.GetSetting<ButtonSetting>(settingName);
                setting?.ExecuteAction();
            })
            .AddTo(_disposables);
    }
    
    /// <summary>
    /// SettingsManagerのデータをViewの形式に変換してViewに設定
    /// </summary>
    private void RefreshSettingsView()
    {
        var settingsData = _settingsManager.Settings.Select(ConvertToDisplayData).ToArray();
        _settingsView.SetSettings(settingsData);
    }
    
    /// <summary>
    /// ISettingBaseをSettingDisplayDataに変換
    /// </summary>
    private SettingsView.SettingDisplayData ConvertToDisplayData(ISettingBase setting)
    {
        var data = new SettingsView.SettingDisplayData
        {
            name = setting.SettingName,
            displayName = setting.SettingName
        };
        
        switch (setting)
        {
            case SliderSetting sliderSetting:
                data.type = SettingsView.SettingType.Slider;
                data.floatValue = sliderSetting.CurrentValue;
                data.minValue = sliderSetting.MinValue;
                data.maxValue = sliderSetting.MaxValue;
                break;
                
            case EnumSetting enumSetting:
                data.type = SettingsView.SettingType.Enum;
                data.stringValue = enumSetting.CurrentValue;
                data.options = enumSetting.Options;
                data.displayNames = enumSetting.DisplayNames;
                break;
                
            case ButtonSetting buttonSetting:
                data.type = SettingsView.SettingType.Button;
                data.buttonText = buttonSetting.ButtonText;
                data.requiresConfirmation = buttonSetting.RequiresConfirmation;
                data.confirmationMessage = buttonSetting.ConfirmationMessage;
                break;
        }
        
        return data;
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}