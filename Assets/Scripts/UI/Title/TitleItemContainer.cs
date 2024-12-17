using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleItemContainer : MonoBehaviour
{
    public BallData ballData { get; private set; }
    public RelicData relicData { get; private set; }
    
    [SerializeField] private Image relicImage;
    [SerializeField] private Text relicName;
    [SerializeField] private Text relicDescription;
    
    public void SetBallData(BallData b)
    {
        this.ballData = b;
        relicImage.sprite = ballData.sprite;
        
        // イベントを登録
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.ShowBallDescriptionWindow(ballData,
                transform.position + new Vector3(2.55f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.HideBallDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
    
    public void SetRelicData(RelicData r)
    {
        this.relicData = r;
        relicImage.sprite = relicData.sprite;
        
        // イベントを登録
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.ShowRelicDescriptionWindow(relicData,
                transform.position + new Vector3(2.55f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(this.gameObject,  () =>
        {
            TitleMenu.Instance.HideRelicDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
}
