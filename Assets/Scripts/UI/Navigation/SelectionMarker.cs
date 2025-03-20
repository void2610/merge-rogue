using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; // DoTweenの名前空間

public class SelectionMarker : MonoBehaviour
{
    [SerializeField] private Vector2 offset;      // サイズに加算するオフセット
    [SerializeField] private Camera uiCamera;       // UI用のカメラ
    
    private float _tweenDuration = 0.1f;  // Tweenの所要時間
    private RectTransform _marker; // マーカーのRectTransform
    private Image _image;          // マーカーのImageコンポーネント

    // Tweenの重複生成を防ぐための参照
    private Tween _moveTween;
    private Tween _sizeTween;
    private Tween _fadeTween;

    // 前回の目標位置・サイズ（変化があった場合のみTweenするため）
    private Vector3 _lastTargetPos;
    private Vector2 _lastTargetSize;
    private bool _hasTarget = false;

    private void Awake()
    {
        _marker = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }
    
    private void Update()
    {
        // 現在選択されているUI要素を取得
        var selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject && selectedObject.TryGetComponent<RectTransform>(out var selectedRect))
        {
            // UI要素のワールド座標（4隅）を取得
            var corners = new Vector3[4];
            selectedRect.GetWorldCorners(corners);

            // ワールド座標からスクリーン座標へ変換
            var min = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

            // 目標の位置とサイズを計算（位置は中央、サイズは四隅から算出＋offset）
            var targetPos = selectedRect.position;
            var targetSize = new Vector2(max.x - min.x, max.y - min.y) + offset;

            // 位置のTween：前回の位置と大きく異なる場合のみTweenを開始
            if (!_hasTarget || Vector3.Distance(_lastTargetPos, targetPos) > 0.01f)
            {
                _moveTween?.Kill();
                _moveTween = _marker.DOMove(targetPos, _tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                _lastTargetPos = targetPos;
            }
            // サイズのTween：前回のサイズと大きく異なる場合のみTweenを開始
            if (!_hasTarget || Vector2.Distance(_lastTargetSize, targetSize) > 0.01f)
            {
                _sizeTween?.Kill();
                _sizeTween = _marker.DOSizeDelta(targetSize, _tweenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                _lastTargetSize = targetSize;
            }
            _hasTarget = true;

            // マーカーをフェードイン（アルファ値をTweenで1に）
            if (_image.color.a < 1)
            {
                _fadeTween?.Kill();
                _fadeTween = _image.DOFade(1, _tweenDuration).SetUpdate(true);
            }
        }
        else
        {
            // 選択が解除された場合はフェードアウト（アルファ値をTweenで0に）
            if (_image.color.a > 0)
            {
                _fadeTween?.Kill();
                _fadeTween = _image.DOFade(0, _tweenDuration).SetUpdate(true);
            }
            _hasTarget = false;
        }
    }
}
