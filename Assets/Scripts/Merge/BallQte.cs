using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class BallQte : MonoBehaviour
{
    [SerializeField] private RectTransform qteCursor;
    [SerializeField] private Vector2 beginPos;
    [SerializeField] private Vector2 endPos;
    [SerializeField] private int maxBallCount = 7;
    [SerializeField] private float speed = 1.0f;

    public async UniTask<int> GetBallRankFromQte()
    {
        StartQte();
        
        await UniTask.WaitUntil(() => InputProvider.Instance.Gameplay.LeftClick.IsPressed());
        StopQte();
        return GetBallRank();
    }
    
    private void StartQte()
    {
        qteCursor.gameObject.SetActive(true);
        qteCursor.DOAnchorPos(beginPos, 0.0f);
        qteCursor.DOAnchorPos(endPos, 1f / speed).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }
    
    private void StopQte()
    {
        qteCursor.gameObject.SetActive(false);
        qteCursor.DOKill();
    }

    private int GetBallRank()
    {
        var p = qteCursor.anchoredPosition.x / (endPos.x - beginPos.x);
        var rank = Mathf.FloorToInt(p * maxBallCount);
        return Mathf.Clamp(rank, 0, maxBallCount);
    }

    private void Awake()
    {
        qteCursor.gameObject.SetActive(false);
    }
}
