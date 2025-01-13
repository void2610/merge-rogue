using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleItemContainer : MonoBehaviour
{
    public BallData BallData { get; private set; }
    public RelicData RelicData { get; private set; }
    
    [SerializeField] private Image relicImage;
    [SerializeField] private Text relicName;
    [SerializeField] private Text relicDescription;
    
    public void SetBallData(BallData b)
    {
        this.BallData = b;
        relicImage.sprite = BallData.sprite;
        
        // イベントを登録
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.ShowBallDescriptionWindow(BallData, this.gameObject);
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.HideBallDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
    
    public void SetRelicData(RelicData r)
    {
        this.RelicData = r;
        relicImage.sprite = RelicData.sprite;
        
        // イベントを登録
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.ShowRelicDescriptionWindow(RelicData, this.gameObject);
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.HideRelicDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
}
