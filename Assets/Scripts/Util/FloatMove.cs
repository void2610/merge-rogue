using UnityEngine;
using DG.Tweening;

public class FloatMove : MonoBehaviour
{
    [SerializeField]
    private float moveDistance = 0.2f;
    [SerializeField]
    private float moveDuration = 1f;
    [SerializeField]
    private float delay = 0f;

    private Tween _floatTween;

    public void MoveTo(Vector3 target, float duration)
    {
        _floatTween?.Kill(); // 既存のTweenを停止

        if (TryGetComponent(out RectTransform rectTransform))
        {
            rectTransform.DOLocalMove(target, duration).OnComplete(() => StartMove(rectTransform));
        }
        else
        {
            transform.DOLocalMove(target, duration).OnComplete(() => StartMove(transform));
        }
    }

    private void StartMove(Transform targetTransform)
    {
        _floatTween?.Kill(); // 過去のTweenを停止

        var currentY = targetTransform.localPosition.y; // 現在のY座標を取得

        _floatTween = DOTween.To(
                () => targetTransform.localPosition.y,
                y => targetTransform.localPosition = new Vector3(
                    targetTransform.localPosition.x,
                    y,
                    targetTransform.localPosition.z),
                currentY + moveDistance,
                moveDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void Awake()
    {
        Invoke(nameof(DelayedStartMove), delay);
    }

    private void DelayedStartMove()
    {
        StartMove(transform);
    }
    
    private void OnDestroy()
    {
        _floatTween?.Kill();
    }
}