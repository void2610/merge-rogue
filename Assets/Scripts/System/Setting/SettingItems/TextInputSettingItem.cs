using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// テキスト入力設定項目の実装
/// </summary>
public class TextInputSettingItem : ISettingItem
{
    private readonly GameObject _containerObject;
    private readonly TMP_InputField _inputField;
    private readonly Subject<(string settingName, string value)> _onValueChanged;
    private readonly HashSet<string> _focusedInputFields;
    
    public string SettingName { get; }
    public GameObject GameObject => _containerObject;
    
    public TextInputSettingItem(
        SettingsView.SettingDisplayData settingData,
        GameObject containerPrefab,
        GameObject titlePrefab,
        GameObject textInputPrefab,
        Transform parent,
        Subject<(string settingName, string value)> onValueChanged,
        HashSet<string> focusedInputFields)
    {
        SettingName = settingData.name;
        _onValueChanged = onValueChanged;
        _focusedInputFields = focusedInputFields;
        
        // コンテナを作成
        _containerObject = Object.Instantiate(containerPrefab, parent);
        
        // タイトルテキストを作成
        CreateTitleText(titlePrefab, _containerObject.transform, settingData.displayName);
        
        // テキスト入力UIを作成
        var inputObject = Object.Instantiate(textInputPrefab, _containerObject.transform);
        
        // レイアウト要素を設定
        var layoutElement = inputObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = inputObject.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        // コンポーネントを取得
        _inputField = inputObject.GetComponentInChildren<TMP_InputField>();
        
        if (_inputField)
        {
            _inputField.text = settingData.stringValue ?? "";
            _inputField.characterLimit = settingData.maxLength > 0 ? settingData.maxLength : 50;
            
            if (!string.IsNullOrEmpty(settingData.placeholder) && _inputField.placeholder)
            {
                var placeholderText = _inputField.placeholder.GetComponent<TextMeshProUGUI>();
                if (placeholderText)
                {
                    placeholderText.text = settingData.placeholder;
                }
            }
            
            // フォーカス状態を監視
            _inputField.onSelect.AddListener(OnInputSelect);
            _inputField.onDeselect.AddListener(OnInputDeselect);
            _inputField.onValueChanged.AddListener(OnValueChanged);
        }
    }
    
    public List<Selectable> GetSelectables()
    {
        var selectables = new List<Selectable>();
        if (_inputField) selectables.Add(_inputField);
        return selectables;
    }
    
    public void UpdateValue(SettingsView.SettingDisplayData settingData)
    {
        if (_inputField && !_inputField.isFocused && _inputField.text != settingData.stringValue)
        {
            // フォーカス中でない場合のみ更新
            _inputField.SetTextWithoutNotify(settingData.stringValue ?? "");
        }
    }
    
    public void Dispose()
    {
        if (_inputField)
        {
            _inputField.onSelect.RemoveAllListeners();
            _inputField.onDeselect.RemoveAllListeners();
            _inputField.onValueChanged.RemoveAllListeners();
        }
        if (_containerObject) Object.Destroy(_containerObject);
    }
    
    private void CreateTitleText(GameObject titlePrefab, Transform parent, string titleText)
    {
        var titleObject = Object.Instantiate(titlePrefab, parent);
        
        var textComponent = titleObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent) textComponent.text = titleText;
        
        var layoutElement = titleObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = titleObject.AddComponent<LayoutElement>();
        }
        layoutElement.preferredWidth = 150f; // タイトルの固定幅
        layoutElement.flexibleWidth = 0f;    // 伸縮しない
    }
    
    private void OnInputSelect(string str)
    {
        _focusedInputFields.Add(SettingName);
    }
    
    private void OnInputDeselect(string str)
    {
        _focusedInputFields.Remove(SettingName);
        // フォーカスが外れた時に最終的な値を送信
        _onValueChanged.OnNext((SettingName, _inputField.text));
    }
    
    private void OnValueChanged(string value)
    {
        _onValueChanged.OnNext((SettingName, value));
    }
}