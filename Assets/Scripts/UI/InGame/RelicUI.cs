using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using R3;
using DG.Tweening;

public class RelicUI : MonoBehaviour
{
    public RelicData relicData { get; private set; }
    
    [SerializeField] private Image relicImage;
    [SerializeField] private Text relicName;
    [SerializeField] private Text relicDescription;
    [SerializeField] private Image bloomImage;
    private Color defaultColor = Color.white;
    private Color bloomColor = Color.white;
    
    public void SetRelicData(RelicData r)
    {
        defaultColor = this.transform.Find("defaultColor").GetComponent<Image>().color;
        bloomColor = this.transform.Find("bloomColor").GetComponent<Image>().color;
        
        this.relicData = r;
        relicImage.sprite = relicData.sprite;
        
        // イベントを登録
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            UIManager.Instance.ShowRelicDescriptionWindow(relicData,
                transform.position + new Vector3(2.55f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            UIManager.Instance.HideRelicDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
    
    public void ActivateUI()
    {
        if(!bloomImage) return;
        var color = bloomColor * 1.3f;
        bloomImage.DOColor(color, 0.1f).OnComplete(() =>
        {
            bloomImage.DOColor(defaultColor, 0.5f).SetDelay(0.75f).SetLink(gameObject);
        }).SetLink(gameObject);
    }
    
    public void AlwaysActive()
    {
        bloomImage.DOColor(bloomColor, 0.1f).SetLink(gameObject);
    }
}
