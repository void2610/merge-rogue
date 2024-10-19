using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    public void SetUp(int damage){
        var t = GetComponent<TextMeshProUGUI>();
        var r = Random.Range(-0.5f, 0.5f);
        
        t.text = damage.ToString();
        t.color = Color.red;
        t.DOColor(Color.white, 0.8f).SetLink(gameObject);
        
        var s = Random.Range(2f, 4f);
        transform.DOScale(s, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOScale(s/2, 0.1f).SetEase(Ease.Linear).SetLink(gameObject);
        }).SetLink(gameObject);

        transform.DOMoveX(r > 0.0f ? -1.5f : 1.5f, 1.8f).SetRelative(true).SetEase(Ease.OutCubic).SetLink(gameObject);

        transform.DOMoveY(1.5f, 0.3f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOMoveY(-1.5f, 1f).SetRelative(true).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                t.DOFade(0, 0.5f).OnComplete(() => Destroy(gameObject)).SetLink(gameObject);
            }).SetLink(gameObject);
        }).SetLink(gameObject);
    }
}
