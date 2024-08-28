using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    private Vector3 target = new Vector3(-8.5f, 4.75f, 0);
    void Start()
    {
        if (target == null) return;
        SeManager.instance.PlaySe("coin");

        float r = UnityEngine.Random.Range(-0.1f, 0.1f);
        this.transform.position += new Vector3(r, 1, 0);

        // this.transform.DOScale(1f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        // {
        //     this.transform.DOScale(0.5f, 0.1f).SetEase(Ease.Linear);
        // });

        if (r > 0.0f)
            this.transform.DOMoveX(-1.5f, 2f).SetRelative(true);
        else
            this.transform.DOMoveX(1.5f, 2f).SetRelative(true);

        this.transform.DOMoveY(-1f, 1.2f).SetEase(Ease.OutBounce).SetRelative(true).OnComplete(() =>
        {
            Vector3 middle = new Vector3(((this.transform.position.x + target.x) / 2) + 0.5f, ((this.transform.position.y + target.y) / 2) + 0.5f, 0);
            this.transform.DOPath(new Vector3[] { this.transform.position, middle, target }, 1f).SetEase(Ease.OutExpo
            ).OnComplete(() =>
            {
                this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f);
                Destroy(this.gameObject);
            });
        });
    }

    void Update()
    {

    }
}
