using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    private const float FLOOR = 2.9f;

    public void SetUp(int damage, float xPos){
        this.transform.position = new Vector3(xPos, FLOOR + 1, 0);
        
        var t = GetComponent<TextMeshProUGUI>();
        var dir = Random.Range(-0.5f, 0.5f);
        
        t.text = damage.ToString();
        t.color = Color.red;
        t.DOColor(Color.white, 0.8f).SetLink(gameObject);
    
        var s = 2 * (1 + ((damage - 15) / 100f));
        transform.DOScale(s, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOScale(s/2, 0.1f).SetEase(Ease.Linear).SetLink(gameObject);
        }).SetLink(gameObject);

        var r = Random.Range(0.5f, 1.5f);
        transform.DOMoveX(dir > 0.0f ? -1.5f * r : 1.5f * r, 1.8f * r).SetRelative(true).SetEase(Ease.OutCubic).SetLink(gameObject);
        
        transform.DOMoveY(FLOOR + 2, 0.3f * r).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOMoveY(FLOOR, 1.2f * r).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                t.DOFade(0, 0.5f).OnComplete(() => Destroy(gameObject)).SetLink(gameObject);
            }).SetLink(gameObject);
        }).SetLink(gameObject);
    }
}
