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
    
    private readonly Color defaultColor = new (0.3960784f, 0.3960784f, 0.3960784f, 1);
    private readonly Color bloomColor = Color.white;
    
    public void SetRelicData(RelicData r)
    {
        this.relicData = r;
        relicImage.sprite = relicData.sprite;
        
        // イベントを登録
        Utils.Instance.AddEventToObject(this.gameObject,  () =>
        {
            GameManager.Instance.uiManager.ShowRelicDescriptionWindow(relicData,
                transform.position + new Vector3(2.55f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.Instance.AddEventToObject(this.gameObject,  () =>
        {
            GameManager.Instance.uiManager.HideRelicDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
    
    public void ActivateUI()
    {
        if(!bloomImage) return;
        
        bloomImage.DOColor(bloomColor, 0.1f).OnComplete(() =>
        {
            bloomImage.DOColor(defaultColor, 0.5f).SetDelay(0.75f).SetLink(gameObject);
        }).SetLink(gameObject);
    }
    
    public void AlwaysActive()
    {
        bloomImage.DOColor(bloomColor, 0.1f).SetLink(gameObject);
    }
}
