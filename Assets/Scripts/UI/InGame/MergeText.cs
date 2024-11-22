using UnityEngine;
using TMPro;
using DG.Tweening;

public class MergeText : MonoBehaviour
{
    public void SetUp(int damage, Color color = default){
        var x = Random.Range(-1.0f, 1.0f);
        var y = Random.Range(-0.25f, 1.0f);
        transform.position += new Vector3(x, y, 0);
        
        var t = GetComponent<TextMeshProUGUI>();
        
        t.text = damage.ToString();
        t.color = color;

        float s = 2 * (1 + ((damage - 15) / 75f));
        transform.DOScale(s, 0);
        
        transform.DOMoveY(0.75f, 2f).SetRelative(true).SetEase(Ease.OutCubic).SetLink(gameObject);
        t.DOFade(0, 1.5f).SetEase(Ease.Linear).SetLink(gameObject).OnComplete(() => Destroy(gameObject));
    }
}
