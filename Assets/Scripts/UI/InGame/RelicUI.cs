using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using R3;
using DG.Tweening;
using TMPro;

public class RelicUI : MonoBehaviour
{
    [SerializeField] private Image relicImage;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image bloomImage;
    private Color _defaultColor = Color.white;
    private Color _bloomColor = Color.white;
    private RelicData _relicData;
    
    public void SetRelicData(RelicData r)
    {
        // 6F6F6F
        _defaultColor = new Color(0.435f, 0.435f, 0.435f, 1);
        
        this._relicData = r;
        relicImage.sprite = _relicData.sprite;
        
        // イベントを登録
        this.gameObject.AddDescriptionWindowEvent(r);
    }
    
    public void ActivateUI()
    {
        if(!bloomImage) return;
        var color = _bloomColor * 1.3f;
        bloomImage.DOColor(color, 0.1f).OnComplete(() =>
        {
            bloomImage.DOColor(_defaultColor, 0.5f).SetDelay(0.75f).SetLink(gameObject);
        }).SetLink(gameObject);
    }
    
    public void ActiveAlways() => bloomImage.DOColor(_bloomColor, 0.1f).SetLink(gameObject);
    public void EnableCount(bool enable) => countText.gameObject.SetActive(enable);

    public void SubscribeCount(ReactiveProperty<int> count) =>
        count.Subscribe(x => { countText.text = x.ToString(); }).AddTo(this);
}
