using System.Globalization;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class BallDescriptionWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private TextMeshProUGUI priceText;

    [SerializeField] private Vector2 minPos; // RectTransform上の座標で指定
    [SerializeField] private Vector2 maxPos; // RectTransform上の座標で指定
    private CanvasGroup cg;
    private Tween moveTween;
    private Tween fadeTween;

    public void ShowWindow(BallData b, Vector3 pos)
    {
        this.gameObject.SetActive(true);

        nameText.text = b.displayName;
        nameText.color = MyColors.GetRarityColor(b.rarity);
        descriptionText.text = b.mainDescription;
        flavorText.text = b.flavorText;
        atkText.text = b.atk.ToString(CultureInfo.InvariantCulture);
        sizeText.text = b.size.ToString(CultureInfo.InvariantCulture);
        priceText.text = b.price.ToString(CultureInfo.InvariantCulture);

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
        
        moveTween?.Kill();
        fadeTween?.Kill();

        this.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(clampedX, clampedY, 0) + new Vector3(0, 0.3f, 0);
        moveTween = this.gameObject.transform.DOMoveY(0.3f, 0.2f).SetRelative(true).SetUpdate(true).SetEase(Ease.OutBack);
        cg.alpha = 0;
        fadeTween = cg.DOFade(1, 0.15f).SetUpdate(true);
    }

    public void HideWindow()
    {
        moveTween?.Kill();
        fadeTween?.Kill();
        
        fadeTween = cg.DOFade(0, 0.15f).SetUpdate(true).OnComplete(() => this.gameObject.SetActive(false));
    }

    private void Awake()
    {
        cg = this.gameObject.GetComponent<CanvasGroup>();
        this.gameObject.SetActive(false);
    }
}