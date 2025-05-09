using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class BallQte : MonoBehaviour
{
    [SerializeField] private RectTransform qteCursor;
    [SerializeField] private Vector2 beginPos;
    [SerializeField] private Vector2 endPos;
    [SerializeField] private int maxBallCount = 7;
    [SerializeField] private float speed = 1.0f;
    
    public void StartQte()
    {
        qteCursor.gameObject.SetActive(true);
        qteCursor.DOAnchorPos(beginPos, 0.0f);
        qteCursor.DOAnchorPos(endPos, 1f / speed).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }
    
    public void StopQte()
    {
        qteCursor.gameObject.SetActive(false);
        qteCursor.DOKill();
    }

    public int GetBallRank()
    {
        var p = qteCursor.anchoredPosition.x / (endPos.x - beginPos.x);
        var rank = Mathf.FloorToInt(p * maxBallCount);
        return Mathf.Clamp(rank, 0, maxBallCount);
    }
}
