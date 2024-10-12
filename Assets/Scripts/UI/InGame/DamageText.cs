using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    public void SetUp(int damage){
        var r = Random.Range(-0.5f, 0.5f);
        transform.position += new Vector3(r, 0, 0);
        GetComponent<TextMeshProUGUI>().text = damage.ToString();
        GetComponent<TextMeshProUGUI>().color = Color.red;
        GetComponent<TextMeshProUGUI>().DOColor(Color.white, 0.5f).SetLink(gameObject);

        transform.DOScale(3f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOScale(1.75f, 0.1f).SetEase(Ease.Linear).SetLink(gameObject);
        }).SetLink(gameObject);

        transform.DOMoveX(r > 0.0f ? -1.5f : 1.5f, 2f).SetRelative(true).SetEase(Ease.Linear).SetLink(gameObject);

        transform.DOMoveY(0.75f, 0.75f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            GetComponent<TextMeshProUGUI>().DOFade(0, 0.5f).SetLink(gameObject);
            transform.DOMoveY(-1f, 0.5f).SetRelative(true).SetEase(Ease.InQuad).OnComplete(() => Destroy(gameObject)).SetLink(gameObject);
        }).SetLink(gameObject);
    }
}
