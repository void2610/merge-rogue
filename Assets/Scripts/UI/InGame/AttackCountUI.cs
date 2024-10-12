using UnityEngine;
using DG.Tweening;

public class AttackCountUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI attackCountText;
    private Tween shakeTween;
    private const int VIBRATO = 10;
    private const float RANDOMNESS = 90.0f;

    public void SetAttackCount(int target)
    {
        var first = attackCountText.text == "" ? 0 : int.Parse(attackCountText.text);
        DOTween.To(() => first, x => attackCountText.text =  x.ToString(), target, (target - first) * 0.1f);
        shakeTween = transform.DOShakePosition(1.0f, target * 0.1f, VIBRATO, RANDOMNESS, false, false).SetLoops(-1);
    }

    private void Awake()
    {
        attackCountText = GetComponent<TMPro.TextMeshProUGUI>();
        attackCountText.text = "0";
        shakeTween = transform.DOShakePosition(1.0f, 0, VIBRATO, RANDOMNESS, false, false).SetLoops(-1);
    }
}
