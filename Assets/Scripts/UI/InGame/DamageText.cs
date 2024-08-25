using UnityEngine;

public class DamageText : MonoBehaviour
{
    // public void ShowDamage(int damage)
    // {
    //     float r = UnityEngine.Random.Range(-0.5f, 0.5f);
    //     var g = Instantiate(damageTextPrefab, this.transform.position + new Vector3(r, 0, 0), Quaternion.identity, this.canvas.transform);
    //     g.GetComponent<TextMeshProUGUI>().text = damage.ToString();

    //     g.GetComponent<TextMeshProUGUI>().color = Color.red;
    //     g.GetComponent<TextMeshProUGUI>().DOColor(Color.white, 0.5f);

    //     g.transform.DOScale(3f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
    //     {
    //         g.transform.DOScale(1.75f, 0.1f).SetEase(Ease.Linear);
    //     });

    //     g.transform.DOMoveX(r > 0.0f ? -1.5f : 1.5f, 2f).SetRelative(true).SetEase(Ease.Linear);

    //     g.transform.DOMoveY(0.75f, 0.75f).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() =>
    //     {
    //         g.GetComponent<TextMeshProUGUI>().DOFade(0, 0.5f);
    //         g.transform.DOMoveY(-1f, 0.5f).SetRelative(true).SetEase(Ease.InQuad).OnComplete(() => Destroy(g));
    //     });
    // }
}
