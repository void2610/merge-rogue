using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ボタン設定項目の実装
/// </summary>
public class ButtonSettingItem : ISettingItem
{
    private readonly GameObject _containerObject;
    private readonly Button _button;
    private readonly Subject<string> _onButtonClicked;
    private readonly ConfirmationDialogView _confirmationDialog;
    private readonly SettingsView.SettingDisplayData _settingData;
    
    public string SettingName { get; }
    public GameObject GameObject => _containerObject;
    
    public ButtonSettingItem(
        SettingsView.SettingDisplayData settingData,
        GameObject containerPrefab,
        GameObject titlePrefab,
        GameObject buttonPrefab,
        Transform parent,
        Subject<string> onButtonClicked,
        ConfirmationDialogView confirmationDialog)
    {
        SettingName = settingData.name;
        _onButtonClicked = onButtonClicked;
        _confirmationDialog = confirmationDialog;
        _settingData = settingData;
        
        // コンテナを作成
        _containerObject = Object.Instantiate(containerPrefab, parent);
        
        // タイトルテキストを作成
        CreateTitleText(titlePrefab, _containerObject.transform, settingData.displayName);
        
        // ボタンUIを作成
        var buttonObject = Object.Instantiate(buttonPrefab, _containerObject.transform);
        
        // レイアウト要素を設定
        var layoutElement = buttonObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = buttonObject.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        // コンポーネントを取得
        _button = buttonObject.GetComponentInChildren<Button>();
        var buttonText = _button?.GetComponentInChildren<TextMeshProUGUI>();
        
        // ボタンテキストを設定
        if (buttonText) buttonText.text = settingData.buttonText;
        
        // ボタンクリック時のイベント
        if (_button)
        {
            _button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    public List<Selectable> GetSelectables()
    {
        var selectables = new List<Selectable>();
        if (_button) selectables.Add(_button);
        return selectables;
    }
    
    public void UpdateValue(SettingsView.SettingDisplayData settingData)
    {
        // ボタンは通常更新不要（テキストが変わることはない）
    }
    
    public void Dispose()
    {
        if (_button) _button.onClick.RemoveAllListeners();
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
    
    private void OnButtonClicked()
    {
        if (_settingData.requiresConfirmation && _confirmationDialog)
        {
            ShowConfirmationDialog().Forget();
        }
        else
        {
            _onButtonClicked.OnNext(SettingName);
        }
    }
    
    private async UniTaskVoid ShowConfirmationDialog()
    {
        var result = await _confirmationDialog.ShowDialog(
            _settingData.confirmationMessage,
            "実行",
            "キャンセル"
        );
        
        if (result)
        {
            _onButtonClicked.OnNext(SettingName);
        }
    }
}