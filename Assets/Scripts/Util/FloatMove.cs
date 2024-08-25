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

    private void StartMove()
    {
        transform.DOLocalMoveY(moveDistance, moveDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetRelative(true);
    }
    private void Awake()
    {
        Invoke("StartMove", delay);
    }
}
