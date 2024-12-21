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

    private Tween floatTween;

    public void MoveTo(Vector3 target, float duration)
    {
        floatTween?.Kill(); // 既存のTweenを停止

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
        floatTween?.Kill(); // 過去のTweenを停止

        float currentY = targetTransform.localPosition.y; // 現在のY座標を取得

        floatTween = DOTween.To(
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
        floatTween?.Kill();
    }
}