using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackCountUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI attackCountText;
    private const int VIBRATO = 10;
    private const float RANDOMNESS = 90.0f;
    
    private Tween sizeTween;
    private Tween angleTween;
    private float defaultSize;
    
    public void SetAttackCount(int target)
    {
        sizeTween?.Kill();
        angleTween?.Kill();
        
        if (target == 0)
        {
            attackCountText.text = "0";
            attackCountText.transform.DOScale(Vector3.one * defaultSize, 0.03f);
            return;
        }
        
        float inDuration = 0.03f + (target * 0.0001f);
        float outDuration = inDuration * 15;
        float size = 1 + (target * 0.01f);
        float angle = 5 + (target * 0.05f);
        attackCountText.text = target.ToString();
        
        sizeTween = attackCountText.transform.DOScale(Vector3.one * (defaultSize * (size * 3)), inDuration)
            .OnComplete(() => 
                    attackCountText.transform.DOScale(Vector3.one * (defaultSize * size), outDuration).SetEase(Ease.OutBounce)
            );
        // 少し時計回りに傾く
       angleTween = attackCountText.transform.DORotate(new Vector3(0, 0, angle), inDuration)
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
        defaultSize = attackCountText.transform.localScale.x;
    }
}
