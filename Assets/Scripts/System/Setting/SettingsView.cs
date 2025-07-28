using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Button closeButton;
    
    // 外部への通知用イベント
    private readonly Subject<(string settingName, float value)> _onSliderChanged = new();
    private readonly Subject<(string settingName, string value)> _onEnumChanged = new();
    private readonly Subject<(string settingName, string value)> _onTextInputChanged = new();
    private readonly Subject<string> _onButtonClicked = new();
    
    private readonly List<ISettingItem> _settingItems = new();
    private readonly HashSet<string> _focusedInputFields = new();
    private SettingItemFactory _factory;
    
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
    /// フォーカス中のInputFieldがあるかチェック
    /// </summary>
    public bool HasFocusedInputField() => _focusedInputFields.Count > 0;
    
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
    /// 初期化処理
    /// </summary>
    private void Awake()
    {
        // ファクトリーを初期化
        _factory = new SettingItemFactory(
            settingsContentContainerPrefab,
            titleTextPrefab,
            sliderSettingPrefab,
            buttonSettingPrefab,
            enumSettingPrefab,
            textInputSettingPrefab,
            confirmationDialog,
            _onSliderChanged,
            _onEnumChanged,
            _onTextInputChanged,
            _onButtonClicked,
            _focusedInputFields
        );
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
            var settingItem = _factory.Create(settingData, this.transform);
            if (settingItem != null) _settingItems.Add(settingItem);
        }
        
        // 最初の要素にフォーカスを設定
        _settingItems[0].GetSelectables()[0].gameObject.AddComponent<FocusSelectable>();
        
        // ナビゲーションを設定
        SetupNavigation();
    }
    
    /// <summary>
    /// 個別の設定項目を更新（フォーカス維持）
    /// </summary>
    public void UpdateSetting(SettingDisplayData settingData)
    {
        var settingItem = _settingItems.FirstOrDefault(item => item.SettingName == settingData.name);
        settingItem?.UpdateValue(settingData);
    }
    
    /// <summary>
    /// 設定UIをクリア
    /// </summary>
    private void ClearSettingsUI()
    {
        foreach (var settingItem in _settingItems)
        {
            settingItem.Dispose();
        }
        
        _settingItems.Clear();
    }
    
    /// <summary>
    /// 生成された設定UIのナビゲーションを設定
    /// </summary>
    private void SetupNavigation()
    {
        var selectables = new List<Selectable>();
        
        // 各設定項目からSelectable要素を収集
        foreach (var settingItem in _settingItems)
        {
            selectables.AddRange(settingItem.GetSelectables());
        }
        
        // 垂直ナビゲーションを設定（isHorizontal = false）
        selectables.SetNavigation(false);
        
        // Enum設定の特別な処理：横ナビゲーションを追加（垂直ナビゲーション設定後に実行）
        for (var i = 0; i < selectables.Count - 1; i++)
        {
            var current = selectables[i];
            var next = selectables[i + 1];
            
            // 連続するボタンがPrevButton/NextButtonのペアの場合
            if (current is Button btn1 && next is Button btn2 &&
                btn1.name == "PrevButton" && btn2.name == "NextButton")
            {
                // 現在のナビゲーション設定を取得
                var nav1 = btn1.navigation;
                var nav2 = btn2.navigation;
                
                // 横ナビゲーションを追加（既存の上下ナビゲーションは保持）
                nav1.mode = Navigation.Mode.Explicit;
                nav1.selectOnRight = btn2;
                // 左ナビゲーションはnullのまま（ループしない）
                
                nav2.mode = Navigation.Mode.Explicit;
                nav2.selectOnLeft = btn1;
                // 右ナビゲーションはnullのまま（ループしない）
                
                // 更新したナビゲーションを適用
                btn1.navigation = nav1;
                btn2.navigation = nav2;
            }
        }
        
        // Closeボタンのナビゲーションを設定
        var closeNav = closeButton.navigation;
        closeNav.selectOnDown = selectables[0];
        closeNav.selectOnUp = selectables[^1];
        closeButton.navigation = closeNav;
        
        var firstNav = selectables[0].navigation;
        firstNav.selectOnUp = closeButton;
        selectables[0].navigation = firstNav;
    }
}