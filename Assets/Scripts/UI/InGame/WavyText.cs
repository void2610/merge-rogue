using UnityEngine;
using DG.Tweening;
using TMPro;

public class WavyText : MonoBehaviour
{
    private TextMeshProUGUI _tmp;

    public void SetUp(string text, Color color, float fontSize)
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        _tmp.color = color;
        _tmp.fontSize = fontSize;
        _tmp.text = text;
        _tmp.alpha = 0;

        var animator = new DOTweenTMPAnimator(_tmp);
        var sequence = DOTween.Sequence();

        for (var i = 0; i < animator.textInfo.characterCount; i++)
        {
            sequence.Join(CreateWavyTween(animator, i));
        }

        sequence.OnComplete(() => {
            Destroy(gameObject, 0.2f);
        });
    }

    private static Sequence CreateWavyTween(DOTweenTMPAnimator animator, int i)
    {
        const float height = 15f;
        const float delay = 0.1f;
        const float duration = 0.2f;
        
        // フェードインを別のSequenceで管理
        var fadeSequence = DOTween.Sequence()
            .AppendCallback(() => animator.DOFadeChar(i, 0, 0))  // 初期値を確実に0に
            .Append(animator.DOFadeChar(i, 1, duration))
            .SetDelay(i * delay);

        // 移動のアニメーションを別のSequenceで管理
        var moveSequence = DOTween.Sequence()
            .Append(animator.DOOffsetChar(i, new Vector3(0, height, 0), duration))
            .Append(animator.DOOffsetChar(i, new Vector3(0, 0, 0), duration))
            .SetDelay(i * delay)
            .SetEase(Ease.InOutSine);

        // 2つのSequenceを合成
        return DOTween.Sequence()
            .Join(fadeSequence)
            .Join(moveSequence);
    }
}