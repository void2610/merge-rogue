using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackCountUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI attackCountText;
    private const int VIBRATO = 10;
    private const float RANDOMNESS = 90.0f;
    
    private Tween _sizeTween;
    private Tween _angleTween;
    private float _defaultSize;
    
    public void SetAttackCount(int target)
    {
        _sizeTween?.Kill();
        _angleTween?.Kill();
        
        if (target == 0)
        {
            attackCountText.text = "0";
            attackCountText.transform.DOScale(Vector3.one * _defaultSize, 0.03f);
            return;
        }
        
        var inDuration = 0.03f + (target * 0.0001f);
        var outDuration = inDuration * 15;
        var size = 1 + (target * 0.001f);
        var angle = 5 + (target * 0.03f);
        attackCountText.text = target.ToString();
        
        _sizeTween = attackCountText.transform.DOScale(Vector3.one * (_defaultSize * (size * 3)), inDuration)
            .OnComplete(() => 
                    attackCountText.transform.DOScale(Vector3.one * (_defaultSize * size), outDuration).SetEase(Ease.OutBounce)
            );
        // 少し時計回りに傾く
       _angleTween = attackCountText.transform.DORotate(new Vector3(0, 0, angle), inDuration)
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
        _defaultSize = attackCountText.transform.localScale.x;
    }
}
