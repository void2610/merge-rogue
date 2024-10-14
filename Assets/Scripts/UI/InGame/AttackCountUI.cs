using System.Collections.Generic;
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
        if(target == 0) return;
        
        float inDuration = 0.03f + (target * 0.0001f);
        float outDuration = inDuration * 15;
        float size = 1 + (target * 0.005f);
        float angle = 5 + (target * 0.05f);
        attackCountText.text = target.ToString();
        
        attackCountText.transform.DOScale(Vector3.one * size, inDuration)
            .OnComplete(() => 
                    attackCountText.transform.DOScale(Vector3.one, outDuration).SetEase(Ease.OutBounce)
            );
        // 少し時計回りに傾く
       attackCountText.transform.DORotate(new Vector3(0, 0, angle), inDuration)
            .OnComplete(() =>
                {
                    attackCountText.transform.DORotate(new Vector3(0, 0, 0), outDuration).SetEase(Ease.OutBounce);
                }
            );
    }

    private void Awake()
    {
        attackCountText = GetComponent<TMPro.TextMeshProUGUI>();
        attackCountText.text = "0";
        shakeTween = transform.DOShakePosition(1.0f, 0, VIBRATO, RANDOMNESS, false, false).SetLoops(-1);
    }
}
