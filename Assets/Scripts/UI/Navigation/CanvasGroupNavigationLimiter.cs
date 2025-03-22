using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class CanvasGroupNavigationLimiter : MonoBehaviour
{
    [SerializeField] private RectTransform marker;    // マーカーのRectTransform
    [SerializeField] private Image markerImage;         // マーカーのImageコンポーネント
    [SerializeField] private Camera uiCamera;           // UI表示用のカメラ
    [SerializeField] private Vector2 offset;            // サイズに加算するオフセット
    [SerializeField] private float tweenDuration = 0.2f;  // Tweenの所要時間

    // 前回の正当な選択対象
    private GameObject _previousSelected;

    // プログラム側での選択変更の場合はtrueにするフラグ
    private static bool _allowProgrammaticChange = false;

    /// <summary>
    /// プログラム側からの選択変更用ラッパーメソッド。
    /// このメソッド経由なら、CanvasGroupの制限チェックを無視して選択変更が行われます。
    /// </summary>
    public static void SetSelectedGameObjectSafe(GameObject go)
    {
        _allowProgrammaticChange = true;
        EventSystem.current.SetSelectedGameObject(go);
    }

    private void Update()
    {
        var currentSelected = EventSystem.current.currentSelectedGameObject;

        // 選択が無い場合
        if (!currentSelected)
        {
            _previousSelected = null;
            // マーカーをフェードアウト
            if (markerImage.color.a > 0)
            {
                markerImage.DOFade(0, tweenDuration).SetUpdate(true);
            }
            return;
        }

        // 初回の選択なら、即時反映
        if (!_previousSelected)
        {
            _previousSelected = currentSelected;
            UpdateMarkerImmediate(currentSelected);
            return;
        }

        // 前回と異なるUI要素が選ばれた場合
        var sel = currentSelected.GetComponent<Selectable>();
        if (currentSelected != _previousSelected)
        {
            // プログラムによる変更でない場合、CanvasGroupやInteractable状態をチェック
            if (!_allowProgrammaticChange)
            {
                // CanvasGroupの判定（両者にCanvasGroupがあれば比較）
                var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
                var previousGroup = _previousSelected.GetComponentInParent<CanvasGroup>();

                if (currentGroup && previousGroup && currentGroup != previousGroup)
                {
                    // 異なるグループへの移動はユーザー入力として不許可
                    RevertSelection();
                    return;
                }
            }

            // ここまで来た場合、選択変更は許容されるのでTweenで移動
            _previousSelected = currentSelected;
            TweenMarker(currentSelected);
            // プログラム側フラグはリセット
            _allowProgrammaticChange = false;
        }
    }

    /// <summary>
    /// 選択対象のRectTransformからマーカーの位置とサイズを即時反映します。
    /// </summary>
    private void UpdateMarkerImmediate(GameObject selectedObject)
    {
        var corners = new Vector3[4];
        if (selectedObject.TryGetComponent<RectTransform>(out RectTransform selectedRect))
        {
            selectedRect.GetWorldCorners(corners);
            var min = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

            marker.position = selectedRect.position;
            marker.sizeDelta = new Vector2(max.x - min.x, max.y - min.y) + offset;
            markerImage.color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// Tweenを利用してマーカーを滑らかに移動・サイズ変更します。
    /// </summary>
    private void TweenMarker(GameObject selectedObject)
    {
        if (selectedObject.TryGetComponent<RectTransform>(out RectTransform selectedRect))
        {
            var corners = new Vector3[4];
            selectedRect.GetWorldCorners(corners);
            var min = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

            var targetPos = selectedRect.position;
            var targetSize = new Vector2(max.x - min.x, max.y - min.y) + offset;

            // Tweenの開始（前のTweenはKillしてから開始）
            marker.DOMove(targetPos, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
            marker.DOSizeDelta(targetSize, tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
            if (markerImage.color.a < 1)
            {
                markerImage.DOFade(1, tweenDuration).SetUpdate(true);
            }
        }
    }

    /// <summary>
    /// 選択変更が不許可の場合、直ちに前回の選択対象に戻します。
    /// </summary>
    private void RevertSelection()
    {
        EventSystem.current.SetSelectedGameObject(_previousSelected);
    }
}
