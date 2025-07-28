using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private string _lastSelectedObjectName; // 最後に選択されていたオブジェクトの名前を記憶
    private string _lastSelectedObjectType; // 最後に選択されていたオブジェクトのタイプを記憶
    private string _lastSelectedSettingName; // 最後に選択されていた要素が属する設定項目名を記憶
    
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
        
        // 各設定項目の値変更イベントを監視してフォーカス復元を実行
        _onSliderChanged.Subscribe(_ => {
            StoreCurrentSelection();
            RestoreFocusAfterDelay().Forget();
        });
        
        _onEnumChanged.Subscribe(_ => {
            StoreCurrentSelection();
            RestoreFocusAfterDelay().Forget();
        });
        
        _onButtonClicked.Subscribe(_ => {
            StoreCurrentSelection();
            RestoreFocusAfterDelay().Forget();
        });
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
    
    /// <summary>
    /// 現在選択されているオブジェクトの情報を記憶
    /// </summary>
    private void StoreCurrentSelection()
    {
        var currentSelected = EventSystem.current?.currentSelectedGameObject;
        if (currentSelected)
        {
            _lastSelectedObjectName = currentSelected.name;
            _lastSelectedObjectType = currentSelected.GetComponent<Selectable>()?.GetType().Name ?? "Unknown";
            _lastSelectedSettingName = FindSettingNameForObject(currentSelected);
        }
        else
        {
            _lastSelectedObjectName = null;
            _lastSelectedObjectType = null;
            _lastSelectedSettingName = null;
        }
    }
    
    /// <summary>
    /// 数フレーム後にフォーカスを復元
    /// </summary>
    private async UniTaskVoid RestoreFocusAfterDelay()
    {
        // UI更新が完了するまで待機
        await UniTask.DelayFrame(5);
        
        if (!string.IsNullOrEmpty(_lastSelectedObjectName))
        {
            RestoreFocusToSimilarElement();
        }
    }
    
    /// <summary>
    /// 類似の要素にフォーカスを復元
    /// </summary>
    private void RestoreFocusToSimilarElement()
    {
        // 特定の設定項目から要素を探す
        if (!string.IsNullOrEmpty(_lastSelectedSettingName))
        {
            var targetSettingItem = _settingItems.FirstOrDefault(item => item.SettingName == _lastSelectedSettingName);
            if (targetSettingItem != null)
            {
                var targetSelectables = targetSettingItem.GetSelectables();
                
                // 同じ設定項目内で同じ名前とタイプの要素を探す
                var exactMatch = targetSelectables.FirstOrDefault(s => 
                    s.name == _lastSelectedObjectName && 
                    s.GetType().Name == _lastSelectedObjectType);
                
                if (exactMatch)
                {
                    SelectionCursor.SetSelectedGameObjectSafe(exactMatch.gameObject);
                    return;
                }
                
                // 同じ設定項目内で同じ名前の要素を探す
                var nameMatch = targetSelectables.FirstOrDefault(s => s.name == _lastSelectedObjectName);
                if (nameMatch)
                {
                    SelectionCursor.SetSelectedGameObjectSafe(nameMatch.gameObject);
                    return;
                }
                
                // 同じ設定項目内で同じタイプの要素を探す
                var typeMatch = targetSelectables.FirstOrDefault(s => s.GetType().Name == _lastSelectedObjectType);
                if (typeMatch)
                {
                    SelectionCursor.SetSelectedGameObjectSafe(typeMatch.gameObject);
                    return;
                }
                
                // 同じ設定項目内の最初の要素を選択
                if (targetSelectables.Count > 0)
                {
                    SelectionCursor.SetSelectedGameObjectSafe(targetSelectables[0].gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// 指定されたゲームオブジェクトがどの設定項目に属するかを特定
    /// </summary>
    private string FindSettingNameForObject(GameObject targetObject)
    {
        foreach (var settingItem in _settingItems)
        {
            var selectables = settingItem.GetSelectables();
            if (selectables.Any(s => s.gameObject == targetObject))
            {
                return settingItem.SettingName;
            }
        }
        return null;
    }
}