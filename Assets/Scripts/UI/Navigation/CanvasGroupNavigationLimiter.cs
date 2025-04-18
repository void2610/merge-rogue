using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class CanvasGroupNavigationLimiter : MonoBehaviour
{
    [SerializeField] private RectTransform marker;
    [SerializeField] private Image markerImage;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private float magnification = 4f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float tweenDuration = 0.2f;

    private Canvas _canvas;
    private RectTransform _canvasRect;
    private GameObject _previousSelected;
    private static bool _allowProgrammaticChange = false;

    public static void SetSelectedGameObjectSafe(GameObject go)
    {
        _allowProgrammaticChange = true;
        EventSystem.current.SetSelectedGameObject(go);
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

    private void Awake()
    {
        // CanvasとそのRectTransformを取得
        _canvas = marker.GetComponentInParent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();
    }

    private void Update()
    {
        var currentSelected = EventSystem.current.currentSelectedGameObject;

        if (!currentSelected)
        {
            _previousSelected = null;
            if(UIManager.Instance) UIManager.Instance.ResetSelectedGameObject();
            // if (markerImage.color.a >= 1) markerImage.DOFade(0, tweenDuration).SetUpdate(true);
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
                // グループが異なる場合は選択をキャンセル
                if (!IsSameCanvasGroup(currentSelected, _previousSelected) || !currentSelected.GetComponent<Selectable>().interactable)
                {
                    EventSystem.current.SetSelectedGameObject(_previousSelected);
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
        if (selectedObject.TryGetComponent<RectTransform>(out RectTransform selectedRect))
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
            marker.localPosition = localCenter;
            marker.sizeDelta = new Vector2(localMax.x - localMin.x, localMax.y - localMin.y) * magnification;
            markerImage.color = new Color(1, 1, 1, 1);
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
            marker.DOAnchorPos(targetCenter, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
            marker.DOSizeDelta(targetSize, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
            if (markerImage.color.a <= 0)
            {
                markerImage.DOFade(1, tweenDuration).SetUpdate(true);
            }
        }
    }
}
