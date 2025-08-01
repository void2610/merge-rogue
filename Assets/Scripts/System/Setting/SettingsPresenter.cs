using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
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
    private bool _isUpdating = false;
    
    public SettingsPresenter(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }
    
    public void Start()
    {
        _settingsView = Object.FindFirstObjectByType<SettingsView>();
        
        SubscribeToViewEvents();
        SubscribeToModelEvents();
        SubscribeToLocalizationEvents();
        
        // SettingsManagerの初期化完了を待ってから初回の表示更新
        WaitForSettingsAndRefresh().Forget();
    }
    
    private async UniTaskVoid WaitForSettingsAndRefresh()
    {
        // SettingsManagerが設定を初期化するまで待機
        while (_settingsManager.Settings.Count == 0)
        {
            await UniTask.Yield();
        }
        
        // 設定が準備できたらViewを更新
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
        
        // テキスト入力変更イベント
        _settingsView.OnTextInputChanged
            .Subscribe(data => {
                // 更新中でも自分自身の入力は処理する
                var setting = _settingsManager.GetSetting<TextInputSetting>(data.settingName);
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
    /// SettingsManagerのイベントをSubscribe（Model→View更新）
    /// </summary>
    private void SubscribeToModelEvents()
    {
        // フォーカス状態をチェックして更新を制限
        _settingsManager.OnSettingChanged
            .Where(_ => !_settingsView.HasFocusedInputField())
            .Subscribe(settingName => {
                _isUpdating = true;
                // 全体再生成ではなく個別更新を使用してUIの再生成を防ぐ
                UpdateIndividualSetting(settingName);
                _isUpdating = false;
            })
            .AddTo(_disposables);
    }
    
    /// <summary>
    /// LocalizeStringLoaderのイベントをSubscribe（ローカライゼーション更新時のView更新）
    /// </summary>
    private void SubscribeToLocalizationEvents()
    {
        if (LocalizeStringLoader.Instance)
        {
            LocalizeStringLoader.Instance.OnLocalizationUpdated
                .Subscribe(_ => RefreshSettingsViewWithStatePreservation())
                .AddTo(_disposables);
        }
    }
    
    /// <summary>
    /// SettingsManagerのデータをViewの形式に変換してViewに設定
    /// </summary>
    private void RefreshSettingsView()
    {
        // 設定が空の場合は何もしない（初期化待ち）
        if (_settingsManager.Settings.Count == 0) return;
        
        var settingsData = _settingsManager.Settings.Select(ConvertToDisplayData).ToArray();
        _settingsView.SetSettings(settingsData);
    }
    
    /// <summary>
    /// SettingsManagerのデータをViewの形式に変換してViewに設定（状態保持版）
    /// </summary>
    private void RefreshSettingsViewWithStatePreservation()
    {
        // 設定が空の場合は何もしない（初期化待ち）
        if (_settingsManager.Settings.Count == 0) return;
        
        var settingsData = _settingsManager.Settings.Select(ConvertToDisplayData).ToArray();
        _settingsView.SetSettingsWithStatePreservation(settingsData);
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
                
            case TextInputSetting textInputSetting:
                data.type = SettingsView.SettingType.TextInput;
                data.stringValue = textInputSetting.CurrentValue;
                data.maxLength = textInputSetting.MaxLength;
                data.placeholder = textInputSetting.Placeholder;
                break;
        }
        
        return data;
    }
    
    /// <summary>
    /// 個別の設定項目だけを更新（フォーカス維持のため）
    /// </summary>
    private void UpdateIndividualSetting(string settingName)
    {
        var setting = _settingsManager.Settings.FirstOrDefault(s => s.SettingName == settingName);
        if (setting != null)
        {
            var displayData = ConvertToDisplayData(setting);
            _settingsView.UpdateSetting(displayData);
        }
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}