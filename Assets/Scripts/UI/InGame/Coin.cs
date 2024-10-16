using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    private readonly Vector3 target = new(-8.5f, 4.75f, 0);
    private void Start()
    {
        var r = Random.Range(-0.1f, 0.1f);
        this.transform.position += new Vector3(r, 1, 0);

        if (r > 0.0f)
            this.transform.DOMoveX(-1.5f, 2f).SetRelative(true).SetUpdate(true).SetLink(gameObject);
        else
            this.transform.DOMoveX(1.5f, 2f).SetRelative(true).SetUpdate(true).SetLink(gameObject);

        this.transform.DOMoveY(-1f, 1.2f).SetEase(Ease.OutBounce).SetRelative(true).OnComplete(() =>
        {
            var middle = new Vector3(((this.transform.position.x + target.x) / 2) + 0.5f, ((this.transform.position.y + target.y) / 2) + 0.5f, 0);
            this.transform.DOPath(new [] { this.transform.position, middle, target }, 1f).SetEase(Ease.OutExpo
            ).OnComplete(() =>
            {
                this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).SetLink(gameObject);
                Destroy(this.gameObject);
            }).SetUpdate(true).SetLink(gameObject);
        }).SetUpdate(true).SetLink(gameObject);
    }
}
