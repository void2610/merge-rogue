using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    private readonly Vector3 target = new(7.5f, 4.5f, 0);
    private const float FLOOR = 2.8f;

    public void SetUp(float xPos)
    {
        var rx = Random.Range(-1f, 1f);
        this.transform.position = new Vector3(xPos, FLOOR + 1, 0);
        var rt = Random.Range(0.75f, 1.5f);

        this.transform.DOMoveX(1.5f * rx, rt + 0.5f).SetRelative(true).SetLink(gameObject);
        this.transform.DOMoveY(FLOOR, rt).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            var middle = new Vector3(((this.transform.position.x + target.x) / 2) + 0.5f, ((this.transform.position.y + target.y) / 2) + 0.5f, 0);
            this.transform.DOPath(new [] { this.transform.position, middle, target }, 0.8f).SetEase(Ease.OutExpo
            ).OnComplete(() =>
            {
                GameManager.Instance.AddCoin(1);

                this.GetComponent<SpriteRenderer>().DOFade(0, 0.3f).OnComplete(() =>
                    Destroy(this.gameObject)
                ).SetLink(gameObject);
            }).SetLink(gameObject);
        }).SetLink(gameObject);
    }
}
