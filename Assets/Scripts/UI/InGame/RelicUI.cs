using UnityEngine;
using UnityEngine.UI;

public class RelicUI : MonoBehaviour
{
    public RelicData relicData { get; private set; }
    
    [SerializeField] private Image relicImage;
    [SerializeField] private Text relicName;
    [SerializeField] private Text relicDescription;
    [SerializeField] private Image bloomImage;
    
    public void SetRelicData(RelicData r)
    {
        this.relicData = r;
        relicImage.sprite = relicData.sprite;
    }
    
    private void SetActive(bool active)
    {
        var c = active ? Color.white : new Color(0.3960784f, 0.3960784f, 0.3960784f, 1);
        bloomImage.color = c;
    }
}
