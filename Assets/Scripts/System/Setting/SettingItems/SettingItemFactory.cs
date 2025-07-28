using System.Collections.Generic;
using R3;
using UnityEngine;

/// <summary>
/// 設定項目を生成するファクトリークラス
/// </summary>
public class SettingItemFactory
{
    private readonly GameObject _containerPrefab;
    private readonly GameObject _titleTextPrefab;
    private readonly GameObject _sliderSettingPrefab;
    private readonly GameObject _buttonSettingPrefab;
    private readonly GameObject _enumSettingPrefab;
    private readonly GameObject _textInputSettingPrefab;
    private readonly ConfirmationDialogView _confirmationDialog;
    
    // イベント通知用のSubjects
    private readonly Subject<(string settingName, float value)> _onSliderChanged;
    private readonly Subject<(string settingName, string value)> _onEnumChanged;
    private readonly Subject<(string settingName, string value)> _onTextInputChanged;
    private readonly Subject<string> _onButtonClicked;
    private readonly HashSet<string> _focusedInputFields;
    
    public SettingItemFactory(
        GameObject containerPrefab,
        GameObject titleTextPrefab,
        GameObject sliderSettingPrefab,
        GameObject buttonSettingPrefab,
        GameObject enumSettingPrefab,
        GameObject textInputSettingPrefab,
        ConfirmationDialogView confirmationDialog,
        Subject<(string settingName, float value)> onSliderChanged,
        Subject<(string settingName, string value)> onEnumChanged,
        Subject<(string settingName, string value)> onTextInputChanged,
        Subject<string> onButtonClicked,
        HashSet<string> focusedInputFields)
    {
        _containerPrefab = containerPrefab;
        _titleTextPrefab = titleTextPrefab;
        _sliderSettingPrefab = sliderSettingPrefab;
        _buttonSettingPrefab = buttonSettingPrefab;
        _enumSettingPrefab = enumSettingPrefab;
        _textInputSettingPrefab = textInputSettingPrefab;
        _confirmationDialog = confirmationDialog;
        _onSliderChanged = onSliderChanged;
        _onEnumChanged = onEnumChanged;
        _onTextInputChanged = onTextInputChanged;
        _onButtonClicked = onButtonClicked;
        _focusedInputFields = focusedInputFields;
    }
    
    /// <summary>
    /// 設定データから適切な設定項目を生成
    /// </summary>
    public ISettingItem Create(SettingsView.SettingDisplayData settingData, Transform parent)
    {
        switch (settingData.type)
        {
            case SettingsView.SettingType.Slider:
                return new SliderSettingItem(
                    settingData,
                    _containerPrefab,
                    _titleTextPrefab,
                    _sliderSettingPrefab,
                    parent,
                    _onSliderChanged
                );
                
            case SettingsView.SettingType.Enum:
                return new EnumSettingItem(
                    settingData,
                    _containerPrefab,
                    _titleTextPrefab,
                    _enumSettingPrefab,
                    parent,
                    _onEnumChanged
                );
                
            case SettingsView.SettingType.Button:
                return new ButtonSettingItem(
                    settingData,
                    _containerPrefab,
                    _titleTextPrefab,
                    _buttonSettingPrefab,
                    parent,
                    _onButtonClicked,
                    _confirmationDialog
                );
                
            case SettingsView.SettingType.TextInput:
                return new TextInputSettingItem(
                    settingData,
                    _containerPrefab,
                    _titleTextPrefab,
                    _textInputSettingPrefab,
                    parent,
                    _onTextInputChanged,
                    _focusedInputFields
                );
                
            default:
                Debug.LogWarning($"未対応の設定タイプ: {settingData.type}");
                return null;
        }
    }
}