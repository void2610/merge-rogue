using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// 設定画面のUI表示を担当するViewクラス
/// 純粋なView - 外部からのデータ注入とイベント通知のみ
/// </summary>
public class SettingsView : MonoBehaviour
{
    [SerializeField] private ConfirmationDialogView confirmationDialog;
    [SerializeField] private GameObject settingsContentContainerPrefab;
    [SerializeField] private GameObject titleTextPrefab;
    [SerializeField] private GameObject sliderSettingPrefab;
    [SerializeField] private GameObject buttonSettingPrefab;
    [SerializeField] private GameObject enumSettingPrefab;
    [SerializeField] private GameObject textInputSettingPrefab;
    
    // 外部への通知用イベント
    private readonly Subject<(string settingName, float value)> _onSliderChanged = new();
    private readonly Subject<(string settingName, string value)> _onEnumChanged = new();
    private readonly Subject<(string settingName, string value)> _onTextInputChanged = new();
    private readonly Subject<string> _onButtonClicked = new();
    
    private readonly List<GameObject> _settingUIObjects = new();
    
    /// <summary>
    /// スライダー設定値変更イベント
    /// </summary>
    public Observable<(string settingName, float value)> OnSliderChanged => _onSliderChanged;
    
    /// <summary>
    /// 列挙型設定値変更イベント  
    /// </summary>
    public Observable<(string settingName, string value)> OnEnumChanged => _onEnumChanged;
    
    /// <summary>
    /// テキスト入力設定値変更イベント
    /// </summary>
    public Observable<(string settingName, string value)> OnTextInputChanged => _onTextInputChanged;
    
    /// <summary>
    /// ボタンクリックイベント
    /// </summary>
    public Observable<string> OnButtonClicked => _onButtonClicked;
    
    /// <summary>
    /// 設定データ構造体
    /// </summary>
    [Serializable]
    public struct SettingDisplayData
    {
        public string name;
        public string displayName;
        public SettingType type;
        public float floatValue;
        public string stringValue;
        public float minValue;
        public float maxValue;
        public string[] options;
        public string[] displayNames;
        public string buttonText;
        public bool requiresConfirmation;
        public string confirmationMessage;
        public int maxLength;
        public string placeholder;
    }
    
    public enum SettingType
    {
        Slider,
        Enum,
        Button,
        TextInput
    }
    
    /// <summary>
    /// 外部から設定データを注入してUIを更新
    /// </summary>
    /// <param name="settingsData">設定データ配列</param>
    public void SetSettings(SettingDisplayData[] settingsData)
    {
        // 既存のUI要素をクリア
        ClearSettingsUI();
        
        // 各設定項目のUIを生成
        foreach (var settingData in settingsData)
        {
            CreateSettingUI(settingData);
        }
    }
    
    /// <summary>
    /// 設定項目のUIを生成
    /// </summary>
    private void CreateSettingUI(SettingDisplayData settingData)
    {
        // 設定項目のコンテナを作成（横並び用）
        var containerObject = Instantiate(settingsContentContainerPrefab, this.transform);
        // containerObject.transform.localScale = Vector3.one;
        
        // タイトルテキストを作成（左側）
        CreateTitleText(containerObject.transform, settingData.displayName);
        
        // 設定固有のUIを作成（右側）
        switch (settingData.type)
        {
            case SettingType.Slider:
                CreateSliderUI(settingData, containerObject.transform);
                break;
            case SettingType.Button:
                CreateButtonUI(settingData, containerObject.transform);
                break;
            case SettingType.Enum:
                CreateEnumUI(settingData, containerObject.transform);
                break;
            case SettingType.TextInput:
                CreateTextInputUI(settingData, containerObject.transform);
                break;
            default:
                Debug.LogWarning($"未対応の設定タイプ: {settingData.type}");
                break;
        }
        
        _settingUIObjects.Add(containerObject);
    }
    
    /// <summary>
    /// タイトルテキストを作成
    /// </summary>
    private void CreateTitleText(Transform parent, string titleText)
    {
        var titleObject = Instantiate(titleTextPrefab, parent);
        
        // プレハブからTextコンポーネントを取得してテキストを設定
        var textComponent = titleObject.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = titleText;
        // レイアウト要素を追加してタイトル幅を固定
        var layoutElement = titleObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = titleObject.AddComponent<LayoutElement>();
        }
        layoutElement.preferredWidth = 150f; // タイトルの固定幅
        layoutElement.flexibleWidth = 0f;    // 伸縮しない
    }
    
    /// <summary>
    /// スライダー設定のUIを生成
    /// </summary>
    private GameObject CreateSliderUI(SettingDisplayData settingData, Transform parent)
    {
        var uiObject = Instantiate(sliderSettingPrefab, parent);
        
        // レイアウト要素を追加して残り幅を使用
        var layoutElement = uiObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = uiObject.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        // UIコンポーネントを取得
        var slider = uiObject.GetComponentInChildren<Slider>();
        var valueText = uiObject.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        
        // スライダーの設定
        if (slider)
        {
            slider.minValue = settingData.minValue;
            slider.maxValue = settingData.maxValue;
            slider.value = settingData.floatValue;
            
            // スライダー変更時のイベント - 外部に通知
            slider.onValueChanged.AddListener(value => {
                UpdateValueText(valueText, value);
                _onSliderChanged.OnNext((settingData.name, value));
            });
        }
        
        // 値テキストの初期化
        UpdateValueText(valueText, settingData.floatValue);
        
        return uiObject;
    }
    
    /// <summary>
    /// ボタン設定のUIを生成
    /// </summary>
    private GameObject CreateButtonUI(SettingDisplayData settingData, Transform parent)
    {
        var uiObject = Instantiate(buttonSettingPrefab, parent);
        
        // レイアウト要素を追加して残り幅を使用
        var layoutElement = uiObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = uiObject.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        // UIコンポーネントを取得
        var button = uiObject.GetComponentInChildren<Button>();
        var buttonText = button?.GetComponentInChildren<TextMeshProUGUI>();
        
        // ボタンテキストを設定
        if (buttonText) buttonText.text = settingData.buttonText;
        
        // ボタンクリック時のイベント - 外部に通知
        if (button)
        {
            button.onClick.AddListener(() => {
                if (settingData.requiresConfirmation)
                {
                    ShowConfirmationDialog(settingData).Forget();
                }
                else
                {
                    _onButtonClicked.OnNext(settingData.name);
                }
            });
        }
        
        return uiObject;
    }
    
    /// <summary>
    /// 確認ダイアログを表示
    /// </summary>
    private async UniTaskVoid ShowConfirmationDialog(SettingDisplayData settingData)
    {
        var result = await confirmationDialog.ShowDialog(
            settingData.confirmationMessage,
            "実行",
            "キャンセル"
        );
        
        if (result)
        {
            _onButtonClicked.OnNext(settingData.name);
        }
    }
    
    /// <summary>
    /// 値テキストを更新
    /// </summary>
    private void UpdateValueText(TextMeshProUGUI valueText, float value)
    {
        if (valueText)
        {
            valueText.text = $"{value:F2}";
        }
    }
    
    
    /// <summary>
    /// Enum設定のUIを生成
    /// </summary>
    private GameObject CreateEnumUI(SettingDisplayData settingData, Transform parent)
    {
        var uiObject = Instantiate(enumSettingPrefab, parent);
        
        // レイアウト要素を追加して残り幅を使用
        var layoutElement = uiObject.GetComponent<LayoutElement>();
        if (!layoutElement)
        {
            layoutElement = uiObject.AddComponent<LayoutElement>();
        }
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        // UIコンポーネントを取得
        var prevButton = uiObject.transform.Find("PrevButton")?.GetComponent<Button>();
        var nextButton = uiObject.transform.Find("NextButton")?.GetComponent<Button>();
        var valueText = uiObject.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        
        // 現在のインデックスを計算
        int currentIndex = System.Array.IndexOf(settingData.options ?? new string[0], settingData.stringValue);
        if (currentIndex < 0) currentIndex = 0;
        
        // ボタンの設定
        if (prevButton)
        {
            prevButton.onClick.AddListener(() => {
                if (settingData.options != null && settingData.options.Length > 0)
                {
                    currentIndex = (currentIndex - 1 + settingData.options.Length) % settingData.options.Length;
                    var newValue = settingData.options[currentIndex];
                    UpdateEnumValueText(valueText, settingData, currentIndex);
                    _onEnumChanged.OnNext((settingData.name, newValue));
                }
            });
        }
        
        if (nextButton)
        {
            nextButton.onClick.AddListener(() => {
                if (settingData.options != null && settingData.options.Length > 0)
                {
                    currentIndex = (currentIndex + 1) % settingData.options.Length;
                    var newValue = settingData.options[currentIndex];
                    UpdateEnumValueText(valueText, settingData, currentIndex);
                    _onEnumChanged.OnNext((settingData.name, newValue));
                }
            });
        }
        
        // 値テキストの初期化
        UpdateEnumValueText(valueText, settingData, currentIndex);
        
        return uiObject;
    }
    
    /// <summary>
    /// Enumの値テキストを更新
    /// </summary>
    private void UpdateEnumValueText(TextMeshProUGUI valueText, SettingDisplayData settingData, int index)
    {
        if (valueText && settingData.displayNames != null && index >= 0 && index < settingData.displayNames.Length)
        {
            valueText.text = settingData.displayNames[index];
        }
        else if (valueText && settingData.options != null && index >= 0 && index < settingData.options.Length)
        {
            valueText.text = settingData.options[index];
        }
    }
    
    /// <summary>
    /// テキスト入力設定のUIを生成
    /// </summary>
    private GameObject CreateTextInputUI(SettingDisplayData settingData, Transform parent)
    {
        var uiObject = Instantiate(textInputSettingPrefab, parent);
        
        // レイアウト要素を追加して残り幅を使用
        var layoutElement = uiObject.GetComponent<LayoutElement>();
        if (!layoutElement) layoutElement = uiObject.AddComponent<LayoutElement>();
        layoutElement.flexibleWidth = 1f; // 残りの幅を使用
        
        var inputField = uiObject.GetComponentInChildren<TMP_InputField>();
        if (inputField)
        {
            inputField.text = settingData.stringValue ?? "";
            inputField.characterLimit = settingData.maxLength > 0 ? settingData.maxLength : 50;
            
            if (!string.IsNullOrEmpty(settingData.placeholder) && inputField.placeholder)
            {
                var placeholderText = inputField.placeholder.GetComponent<TextMeshProUGUI>();
                if (placeholderText)
                {
                    placeholderText.text = settingData.placeholder;
                }
            }
            
            // テキスト変更時のイベント - 外部に通知
            inputField.onValueChanged.AddListener(value => {
                _onTextInputChanged.OnNext((settingData.name, value));
            });
        }
        
        return uiObject;
    }
    
    /// <summary>
    /// 設定UIをクリア
    /// </summary>
    private void ClearSettingsUI()
    {
        foreach (var uiObject in _settingUIObjects)
        {
            if (uiObject) DestroyImmediate(uiObject);
        }
        
        _settingUIObjects.Clear();
    }
}