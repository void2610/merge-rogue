using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    private readonly Vector3 target = new(7.5f, 4.5f, 0);
    private const float FLOOR = 2.75f;

    public void SetUp(float xPos)
    {
        var r = Random.Range(-0.1f, 0.1f);
        this.transform.position = new Vector3(xPos + r, FLOOR + 1, 0);

        if (r > 0.0f)
            this.transform.DOMoveX(-1.5f, 2f).SetRelative(true).SetUpdate(true).SetLink(gameObject);
        else
            this.transform.DOMoveX(1.5f, 2f).SetRelative(true).SetUpdate(true).SetLink(gameObject);

        this.transform.DOMoveY(FLOOR, 1.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            var middle = new Vector3(((this.transform.position.x + target.x) / 2) + 0.5f, ((this.transform.position.y + target.y) / 2) + 0.5f, 0);
            this.transform.DOPath(new [] { this.transform.position, middle, target }, 0.8f).SetEase(Ease.OutExpo
            ).OnComplete(() =>
            {
                GameManager.Instance.AddCoin(1);

                this.GetComponent<SpriteRenderer>().DOFade(0, 0.3f).OnComplete(() =>
                    Destroy(this.gameObject)
                ).SetUpdate(true).SetLink(gameObject);
            }).SetUpdate(true).SetLink(gameObject);
        }).SetUpdate(true).SetLink(gameObject);
    }
}
