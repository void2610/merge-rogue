using UnityEngine;
using TMPro;
using DG.Tweening;

public class MergeText : MonoBehaviour
{
    public void SetUp(int damage){
        var t = GetComponent<TextMeshProUGUI>();
        
        t.text = damage.ToString();

        float s = 2 * (1 + ((damage - 15) / 75f));
        transform.DOScale(s, 0);
        
        transform.DOMoveY(0.75f, 2f).SetRelative(true).SetEase(Ease.OutCubic).SetLink(gameObject);
        t.DOFade(0, 1.5f).SetEase(Ease.Linear).SetLink(gameObject).OnComplete(() => Destroy(gameObject));
    }
}
