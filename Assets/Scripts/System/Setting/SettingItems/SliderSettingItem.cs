using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スライダー設定項目の実装
/// </summary>
public class SliderSettingItem : ISettingItem
{
    private readonly GameObject _containerObject;
    private readonly Slider _slider;
    private readonly TextMeshProUGUI _valueText;
    private readonly Subject<(string settingName, float value)> _onValueChanged;
    
    public string SettingName { get; }
    public GameObject GameObject => _containerObject;
    
    public SliderSettingItem(
        SettingsView.SettingDisplayData settingData,
        GameObject containerPrefab,
        GameObject titlePrefab,
        GameObject sliderPrefab,
        Transform parent,
        Subject<(string settingName, float value)> onValueChanged)
    {
        SettingName = settingData.name;
        _onValueChanged = onValueChanged;
        
        // コンテナを作成
        _containerObject = Object.Instantiate(containerPrefab, parent);
        
        // タイトルテキストを作成
        CreateTitleText(titlePrefab, _containerObject.transform, settingData.displayName);
        
        // スライダーUIを作成
        var sliderObject = Object.Instantiate(sliderPrefab, _containerObject.transform);
        
        // レイアウト要素を設定
        var layoutElement = sliderObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = sliderObject.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        // コンポーネントを取得
        _slider = sliderObject.GetComponentInChildren<Slider>();
        _valueText = sliderObject.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        
        // スライダーの設定
        if (_slider)
        {
            _slider.minValue = settingData.minValue;
            _slider.maxValue = settingData.maxValue;
            _slider.value = settingData.floatValue;
            
            // 値変更時のイベント
            _slider.onValueChanged.AddListener(value => {
                UpdateValueText(value);
                _onValueChanged.OnNext((SettingName, value));
            });
        }
        
        // 値テキストの初期化
        UpdateValueText(settingData.floatValue);
    }
    
    public List<Selectable> GetSelectables()
    {
        var selectables = new List<Selectable>();
        if (_slider) selectables.Add(_slider);
        return selectables;
    }
    
    public void UpdateValue(SettingsView.SettingDisplayData settingData)
    {
        if (_slider && _slider.value != settingData.floatValue)
        {
            // イベント発火を避けて値のみ更新
            _slider.SetValueWithoutNotify(settingData.floatValue);
            UpdateValueText(settingData.floatValue);
        }
    }
    
    public void Dispose()
    {
        if (_slider) _slider.onValueChanged.RemoveAllListeners();
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
    
    private void UpdateValueText(float value)
    {
        if (_valueText)
        {
            _valueText.text = $"{value:F2}";
        }
    }
}