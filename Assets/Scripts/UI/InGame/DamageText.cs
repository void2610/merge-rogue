using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    public void SetUp(int damage){
        var t = GetComponent<TextMeshProUGUI>();
        var dir = Random.Range(-0.5f, 0.5f);
        
        t.text = damage.ToString();
        t.color = Color.red;
        t.DOColor(Color.white, 0.8f).SetLink(gameObject).SetUpdate(true);
        

        float s = 2 * (1 + ((damage - 15) / 100f));
        transform.DOScale(s, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOScale(s/2, 0.1f).SetEase(Ease.Linear).SetLink(gameObject).SetUpdate(true);
        }).SetLink(gameObject).SetUpdate(true);

        var r = Random.Range(0.5f, 1.5f);
        transform.DOMoveX(dir > 0.0f ? -1.5f * r : 1.5f * r, 1.8f * r).SetRelative(true).SetEase(Ease.OutCubic).SetLink(gameObject).SetUpdate(true);

        transform.DOMoveY(1.5f * r, 0.3f * r).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOMoveY(-1.5f * r, 1f * r).SetRelative(true).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                t.DOFade(0, 0.5f).OnComplete(() => Destroy(gameObject)).SetLink(gameObject).SetUpdate(true);
            }).SetLink(gameObject).SetUpdate(true);
        }).SetLink(gameObject).SetUpdate(true);
    }
}
