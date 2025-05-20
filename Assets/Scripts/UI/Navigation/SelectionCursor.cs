using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class SelectionCursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private float magnification = 4f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float tweenDuration = 0.2f;

    private Canvas _canvas;
    private RectTransform _canvasRect;
    private GameObject _previousSelected;
    private static bool _allowProgrammaticChange = false;
    private static bool _isLockToInventory = false;
    private static EventSystem _eventSystem;
    
    public static void LockCursorToInventory(bool b) => _isLockToInventory = b;

    public static void SetSelectedGameObjectSafe(GameObject go)
    {
        _allowProgrammaticChange = true;
        _eventSystem.SetSelectedGameObject(go);
    }
    
    // ナビゲーション移動がCanvasGroupを跨いでいるかどうかを判定する
    private bool IsSameCanvasGroup(GameObject currentSelected, GameObject previousSelected)
    {
        var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
        var previousGroup = previousSelected.GetComponentInParent<CanvasGroup>();
        var result = (currentGroup && previousGroup && currentGroup == previousGroup);
        if (result) return true;

        // StatusEffectUIだけは許可
        if (!UIManager.Instance) return false;
        if (UIManager.Instance.EnemyStatusUIContainer.OfType<Transform>().ToList().Contains(currentGroup.transform))
        {
            result = true;
        }
        else if (UIManager.Instance.PlayerStatusUI.OfType<Transform>().ToList().Contains(currentSelected.transform))
        {
            result = true;
        }
        return result;
    }
    
    private bool IsInInventoryCanvasGroup(GameObject currentSelected)
    {
        var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
        if (!currentGroup) return false;
        // PauseとTutorialは例外的に許可
        if (currentGroup.name == "Pause" || currentGroup.name == "Tutorial") return true;
        if (currentGroup.name == "InventoryUIContainer") return true;
        return false;
    }

    private void Awake()
    {
        // CanvasとそのRectTransformを取得
        _canvas = cursor.GetComponentInParent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();
        _isLockToInventory = false;
        _eventSystem = EventSystem.current;
    }

    private void Update()
    {
        var currentSelected = _eventSystem.currentSelectedGameObject;

        if (!currentSelected)
        {
            _previousSelected = null;
            if(UIManager.Instance) UIManager.Instance.ResetSelectedGameObject();
            return;
        }

        if (!_previousSelected)
        {
            _previousSelected = currentSelected;
            UpdateMarkerImmediate(currentSelected);
            _allowProgrammaticChange = false;
            return;
        }

        if (currentSelected != _previousSelected)
        {
            if (!_allowProgrammaticChange)
            {
                // ロック状態なら移動をキャンセル
                if (_isLockToInventory && !IsInInventoryCanvasGroup(currentSelected))
                {
                    _eventSystem.SetSelectedGameObject(_previousSelected);
                    Debug.Log("ロック中のため");
                    return;
                }
                
                // グループが異なる場合は選択をキャンセル
                if (!IsSameCanvasGroup(currentSelected, _previousSelected) || !currentSelected.GetComponent<Selectable>().interactable)
                {
                    _eventSystem.SetSelectedGameObject(_previousSelected);
                    Debug.Log("グループが異なるため");
                    return;
                }
            }
            _previousSelected = currentSelected;
            TweenMarker(currentSelected);
            _allowProgrammaticChange = false;
            
            
            // 前の選択対象の説明ウィンドウを非表示にする
            if (_previousSelected.TryGetComponent<ShowSubDescription>(out var sd2))
            {
                DescriptionWindow.Instance.HideSubWindowFromNavigation(_previousSelected, sd2.word);
            }
            
            // 説明ウィンドウを出す
            if (currentSelected.TryGetComponent<ShowDescription>(out var sd))
            {
                if(sd.isBall)
                    DescriptionWindow.Instance.ShowWindowFromNavigation( sd.ballData, currentSelected, sd.level);
                else
                    DescriptionWindow.Instance.ShowWindowFromNavigation(sd.relicData, currentSelected);
            }
            else
            {
                DescriptionWindow.Instance.HideWindowFromNavigation();
            }
            
            if (currentSelected.TryGetComponent<ShowSubDescription>(out var ssd))
            {
                DescriptionWindow.Instance.ShowSubWindow(currentSelected, ssd.word);
            }
        }
        else
        {
            _allowProgrammaticChange = false;
        }
    }

    /// <summary>
    /// 選択対象のRectTransformからマーカーの位置とサイズをキャンバスのローカル座標で即時反映します。
    /// </summary>
    private void UpdateMarkerImmediate(GameObject selectedObject)
    {
        var corners = new Vector3[4];
        if (selectedObject.TryGetComponent<RectTransform>(out var selectedRect))
        {
            selectedRect.GetWorldCorners(corners);
            
            // ワールド座標→スクリーン座標
            var screenMin = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            var screenMax = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

            // スクリーン座標→キャンバスローカル座標に変換
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenMin, uiCamera, out var localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenMax, uiCamera, out var localMax);

            // 中心とサイズを算出
            var localCenter = ((localMin + localMax) / 2f) + offset;
            cursor.localPosition = localCenter;
            cursor.sizeDelta = new Vector2(localMax.x - localMin.x, localMax.y - localMin.y) * magnification;
            cursorImage.color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// Tweenを利用してマーカーを滑らかに移動・サイズ変更します（キャンバスローカル座標で計算）。
    /// </summary>
    private void TweenMarker(GameObject selectedObject)
    {
        if (selectedObject.TryGetComponent<RectTransform>(out RectTransform selectedRect))
        {
            var corners = new Vector3[4];
            selectedRect.GetWorldCorners(corners);
        
            var screenMin = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            var screenMax = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenMin, uiCamera, out var localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenMax, uiCamera, out var localMax);

            // キャンバスローカル座標で中心位置とサイズを算出
            var targetCenter = ((localMin + localMax) / 2f) + offset;
            var targetSize = new Vector2(localMax.x - localMin.x, localMax.y - localMin.y) * magnification;

            // ここでは、DOMoveではなくDOAnchorPosを使用して、アンカー座標をTweenします
            cursor.DOAnchorPos(targetCenter, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
            cursor.DOSizeDelta(targetSize, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
            if (cursorImage.color.a <= 0)
            {
                cursorImage.DOFade(1, tweenDuration).SetUpdate(true);
            }
        }
    }
}
