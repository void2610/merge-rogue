using System;
using System.Collections.Generic;
using System.Linq;
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
    private GameObject _lastTrackedSelectedObject; // スクロール追従用の最後に追跡したオブジェクト
    private ScrollRect _scrollRect;
    
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
        _scrollRect = this.transform.parent.parent.parent.GetComponent<ScrollRect>();
        
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
        
        // UI再生成を停止したため、フォーカス復元処理は不要
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
        
        // スクロール位置を最上部にリセット
        _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, 0);
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
        foreach (var settingItem in _settingItems)
        {
            if (settingItem is EnumSettingItem enumSetting)
            {
                // EnumSettingItemのPrevButtonとNextButtonを取得
                var prevButton = enumSetting.GameObject.GetComponentsInChildren<Button>()
                    .FirstOrDefault(b => b.name == "PrevButton");
                var nextButton = enumSetting.GameObject.GetComponentsInChildren<Button>()
                    .FirstOrDefault(b => b.name == "NextButton");
                
                if (prevButton && nextButton)
                {
                    // PrevButtonのナビゲーション設定（垂直ナビゲーションは既に設定済み）
                    var prevNav = prevButton.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    prevNav.selectOnRight = nextButton;  // 右でNextButtonに移動
                    prevButton.navigation = prevNav;
                    
                    // NextButtonのナビゲーション設定（左右 + 上下ナビゲーション）
                    var nextNav = nextButton.navigation;
                    nextNav.mode = Navigation.Mode.Explicit;
                    nextNav.selectOnLeft = prevButton;   // 左でPrevButtonに戻る
                    
                    // 上下ナビゲーションを設定（PrevButtonと同じ上下要素に移動）
                    nextNav.selectOnUp = prevNav.selectOnUp;
                    nextNav.selectOnDown = prevNav.selectOnDown;
                    nextButton.navigation = nextNav;
                }
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
    /// 毎フレーム選択されたオブジェクトを監視してスクロール位置を調整
    /// </summary>
    private void Update()
    {
        var currentSelected = EventSystem.current?.currentSelectedGameObject;
        
        // 選択が変更された場合のみ処理
        if (currentSelected && currentSelected != _lastTrackedSelectedObject)
        {
            _lastTrackedSelectedObject = currentSelected;
            
            // 選択されたオブジェクトがScrollView内にあるかチェック
            if (IsObjectInsideScrollView(currentSelected))
            {
                EnsureSelectedObjectIsVisible(currentSelected);
            }
        }
    }
    
    /// <summary>
    /// オブジェクトがScrollView内にあるかチェック
    /// </summary>
    private bool IsObjectInsideScrollView(GameObject obj)
    {
        var t = obj.transform;
        
        // ScrollRectのコンテンツの子孫であるかチェック
        while (t)
        {
            if (t == _scrollRect.content.transform) return true;
            t = t.parent;
        }
        
        return false;
    }
    
    /// <summary>
    /// 選択されたオブジェクトが見えるようにスクロール位置を調整
    /// </summary>
    private void EnsureSelectedObjectIsVisible(GameObject selectedObject)
    {
        var selectedRectTransform = selectedObject.GetComponent<RectTransform>();
        if (!selectedRectTransform) return;
        
        // 選択されたオブジェクトとコンテンツの相対位置を計算
        var contentTransform = _scrollRect.content;
        var viewportTransform = _scrollRect.viewport;
        
        // 選択されたオブジェクトのローカル位置を取得
        var selectedLocalPos = contentTransform.InverseTransformPoint(selectedRectTransform.position);
        var selectedRect = selectedRectTransform.rect;
        
        // ビューポートのサイズ
        var viewportHeight = viewportTransform.rect.height;
        
        // 現在のスクロール位置
        var currentScrollPos = contentTransform.anchoredPosition.y;
        
        // 選択されたオブジェクトの上端と下端のY座標（コンテンツ内でのローカル座標）
        var selectedTop = -selectedLocalPos.y + selectedRect.height / 2;
        var selectedBottom = -selectedLocalPos.y - selectedRect.height / 2;
        
        // ビューポートの表示範囲
        var viewportTop = currentScrollPos;
        var viewportBottom = currentScrollPos + viewportHeight;
        
        // マージン（要素が画面端に来ないようにする）
        var margin = 50f;
        
        // スクロール調整が必要かチェック
        var newScrollPos = currentScrollPos;
        
        if (selectedTop < viewportTop + margin)
        {
            // 選択された要素が上にはみ出している場合
            newScrollPos = selectedTop - margin;
        }
        else if (selectedBottom > viewportBottom - margin)
        {
            // 選択された要素が下にはみ出している場合
            newScrollPos = selectedBottom - viewportHeight + margin;
        }
        
        // スクロール位置の範囲制限
        newScrollPos = Mathf.Clamp(newScrollPos, 0, Mathf.Max(0, contentTransform.sizeDelta.y - viewportHeight));
        
        // スクロール位置を更新
        if (Mathf.Abs(newScrollPos - currentScrollPos) > 0.1f)
        {
            contentTransform.anchoredPosition = new Vector2(contentTransform.anchoredPosition.x, newScrollPos);
        }
    }
}