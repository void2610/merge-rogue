using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enum設定項目の実装
/// </summary>
public class EnumSettingItem : ISettingItem
{
    private readonly GameObject _containerObject;
    private readonly Button _prevButton;
    private readonly Button _nextButton;
    private readonly TextMeshProUGUI _valueText;
    private readonly Subject<(string settingName, string value)> _onValueChanged;
    private SettingsView.SettingDisplayData _currentData;
    private int _currentIndex;
    
    public string SettingName { get; }
    public GameObject GameObject => _containerObject;
    
    public EnumSettingItem(
        SettingsView.SettingDisplayData settingData,
        GameObject containerPrefab,
        GameObject titlePrefab,
        GameObject enumPrefab,
        Transform parent,
        Subject<(string settingName, string value)> onValueChanged)
    {
        SettingName = settingData.name;
        _onValueChanged = onValueChanged;
        _currentData = settingData;
        
        // コンテナを作成
        _containerObject = Object.Instantiate(containerPrefab, parent);
        
        // タイトルテキストを作成
        CreateTitleText(titlePrefab, _containerObject.transform, settingData.displayName);
        
        // Enum UIを作成
        var enumObject = Object.Instantiate(enumPrefab, _containerObject.transform);
        
        // レイアウト要素を設定
        var layoutElement = enumObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = enumObject.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        // コンポーネントを取得
        _prevButton = enumObject.transform.Find("PrevButton")?.GetComponent<Button>();
        _nextButton = enumObject.transform.Find("NextButton")?.GetComponent<Button>();
        _valueText = enumObject.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        
        // 現在のインデックスを計算
        _currentIndex = System.Array.IndexOf(settingData.options ?? new string[0], settingData.stringValue);
        if (_currentIndex < 0) _currentIndex = 0;
        
        // ボタンの設定
        if (_prevButton)
        {
            _prevButton.onClick.AddListener(OnPrevButtonClicked);
        }
        
        if (_nextButton)
        {
            _nextButton.onClick.AddListener(OnNextButtonClicked);
        }
        
        // 値テキストの初期化
        UpdateValueText();
    }
    
    public List<Selectable> GetSelectables()
    {
        var selectables = new List<Selectable>();
        if (_prevButton) selectables.Add(_prevButton);
        return selectables;
    }
    
    public void UpdateValue(SettingsView.SettingDisplayData settingData)
    {
        _currentData = settingData;
        int newIndex = System.Array.IndexOf(settingData.options ?? new string[0], settingData.stringValue);
        if (newIndex < 0) newIndex = 0;
        
        if (_currentIndex != newIndex)
        {
            _currentIndex = newIndex;
            UpdateValueText();
        }
    }
    
    public void Dispose()
    {
        if (_prevButton) _prevButton.onClick.RemoveAllListeners();
        if (_nextButton) _nextButton.onClick.RemoveAllListeners();
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
    
    private void OnPrevButtonClicked()
    {
        if (_currentData.options != null && _currentData.options.Length > 0)
        {
            _currentIndex = (_currentIndex - 1 + _currentData.options.Length) % _currentData.options.Length;
            var newValue = _currentData.options[_currentIndex];
            UpdateValueText();
            _onValueChanged.OnNext((SettingName, newValue));
            RestoreFocus(_prevButton).Forget();
        }
    }
    
    private void OnNextButtonClicked()
    {
        if (_currentData.options != null && _currentData.options.Length > 0)
        {
            _currentIndex = (_currentIndex + 1) % _currentData.options.Length;
            var newValue = _currentData.options[_currentIndex];
            UpdateValueText();
            _onValueChanged.OnNext((SettingName, newValue));
            RestoreFocus(_nextButton).Forget();
        }
    }
    
    private void UpdateValueText()
    {
        if (_valueText && _currentData.displayNames != null && 
            _currentIndex >= 0 && _currentIndex < _currentData.displayNames.Length)
        {
            _valueText.text = _currentData.displayNames[_currentIndex];
        }
        else if (_valueText && _currentData.options != null && 
                 _currentIndex >= 0 && _currentIndex < _currentData.options.Length)
        {
            _valueText.text = _currentData.options[_currentIndex];
        }
    }
    
    /// <summary>
    /// 1フレーム後にボタンの選択状態を復元
    /// </summary>
    private async UniTaskVoid RestoreFocus(Button targetButton)
    {
        await UniTask.Yield();
        if (targetButton && targetButton.gameObject)
        {
            SelectionCursor.SetSelectedGameObjectSafe(targetButton.gameObject);
        }
    }
}