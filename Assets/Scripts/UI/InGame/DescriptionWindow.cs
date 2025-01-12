using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DescriptionWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private List<TextMeshProUGUI> statusTexts;
    [SerializeField] private Vector2 minPos; // RectTransform上の座標で指定
    [SerializeField] private Vector2 maxPos; // RectTransform上の座標で指定
    private CanvasGroup _cg;
    private Tween _moveTween;
    private Tween _fadeTween;

    public void ShowWindow(object obj, Vector3 pos)
    {
        this.gameObject.SetActive(true);

        if(obj is BallData b) SetBallTexts(b);
        else if(obj is RelicData r) SetRelicTexts(r);
        else throw new System.ArgumentException("obj is not BallData or RelicData");

        // ワールド座標をRectTransformのローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.gameObject.transform.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(Camera.main, pos),
            Camera.main,
            out var localPos
        );

        // ローカル座標で位置をクランプ
        var clampedX = Mathf.Clamp(localPos.x, minPos.x, maxPos.x);
        var clampedY = Mathf.Clamp(localPos.y, minPos.y, maxPos.y);
        
        _moveTween?.Kill();
        _fadeTween?.Kill();
        
        this.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(clampedX, clampedY, 0) + new Vector3(0, 0.3f, 0);
        _moveTween = this.gameObject.transform.DOMoveY(0.3f, 0.2f).SetRelative(true).SetUpdate(true).SetEase(Ease.OutBack);
        _cg.alpha = 0;
        _fadeTween = _cg.DOFade(1, 0.15f).SetUpdate(true);
    }
    
    private void SetBallTexts(BallData b)
    {
        nameText.text = b.displayName;
        nameText.color = MyColors.GetRarityColor(b.rarity);
        descriptionText.text = b.mainDescription;
        flavorText.text = b.flavorText;
        statusTexts[0].text = "attack: " + b.atk.ToString(CultureInfo.InvariantCulture);
        statusTexts[0].alpha = 1;
        statusTexts[1].text = "size: " + b.size.ToString(CultureInfo.InvariantCulture);
        statusTexts[1].alpha = 1;
        statusTexts[2].text = "price: " + b.price.ToString(CultureInfo.InvariantCulture);
        statusTexts[2].alpha = 1;
    }

    private void SetRelicTexts(RelicData r)
    {
        nameText.text = r.displayName;
        nameText.color = MyColors.GetRarityColor(r.rarity);
        descriptionText.text = r.description;
        flavorText.text = r.flavorText;
        statusTexts[0].text = "price: " + r.price.ToString(CultureInfo.InvariantCulture);
        statusTexts[1].alpha = 0;
        statusTexts[2].alpha = 0;
    }

    public void HideWindow()
    {
        _moveTween?.Kill();
        _fadeTween?.Kill();
        
        _fadeTween = _cg.DOFade(0, 0.15f).SetUpdate(true).OnComplete(() => this.gameObject.SetActive(false));
    }

    private void Awake()
    {
        this.gameObject.SetActive(false);
        _cg = this.gameObject.GetComponent<CanvasGroup>();
    }
}