using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Coin : MonoBehaviour
{
    private readonly Vector3 _target = new(7.5f, 4.5f, 0);
    private const float FLOOR = 2.8f;

    public void SetUp(float xPos) => SetUpAsync(xPos).Forget();
    
    private async UniTask SetUpAsync(float xPos)
    {
        var cancellationToken = this.GetCancellationTokenOnDestroy();
        var rx = Random.Range(-1f, 1f);
        this.transform.position = new Vector3(xPos, FLOOR + 1, 0);
        var rt = Random.Range(0.75f, 1.5f);
        
        // 水平方向の移動と床への落下を並列で実行
        await UniTask.WhenAll(
            this.transform.DOMoveX(1.5f * rx, rt + 0.5f).SetRelative(true).SetLink(gameObject).ToUniTask(cancellationToken: cancellationToken),
            this.transform.DOMoveY(FLOOR, rt).SetEase(Ease.OutBounce).SetLink(gameObject).ToUniTask(cancellationToken: cancellationToken)
        );
        
        // UIのコイン位置への移動
        var middle = new Vector3(((this.transform.position.x + _target.x) / 2) + 0.5f, ((this.transform.position.y + _target.y) / 2) + 0.5f, 0);
        await this.transform.DOPath(new[] { this.transform.position, middle, _target }, 0.8f)
            .SetEase(Ease.OutExpo)
            .SetLink(gameObject)
            .ToUniTask(cancellationToken: cancellationToken);
            
        // フェードアウトして削除
        await this.GetComponent<SpriteRenderer>().DOFade(0, 0.3f)
            .SetLink(gameObject)
            .ToUniTask(cancellationToken: cancellationToken);
            
        Destroy(this.gameObject);
    }
}
