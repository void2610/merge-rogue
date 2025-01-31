using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Treasure : MonoBehaviour
{
    public static Treasure Instance;
    
    [SerializeField] private List<GameObject> items;
    [SerializeField] private Vector3 itemPosition;
    [SerializeField] private float itemOffset;
    private const int MAX_ITEMS = 3;
    private readonly Vector3 _disablePosition = new Vector3(100, 100, 0);

    public void OpenTreasure(int count, Rarity rarity = Rarity.Common)
    {
        if (count is > MAX_ITEMS or <= 0) throw new System.Exception("Invalid count");
        for (var i = 0; i < items.Count; i++)
        {
            if (i < count)
                items[i].transform.position = itemPosition + Vector3.right * (itemOffset * (i - (count - 1) / 2f));
            else
                items[i].transform.position = _disablePosition;
        }
        
        var relics = ContentProvider.Instance.GetRelicDataByRarity(rarity);
        for (var i = 0; i < count; i++)
        {
            var r = GameManager.Instance.RandomRange(0, relics.Count);
            SetEvent(items[i], relics[r]);
            relics.RemoveAt(r);
        }
    }
    
    public void CloseTreasure()
    {
        items.ForEach(item => item.transform.position = _disablePosition);
        UIManager.Instance.EnableCanvasGroup("Treasure", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    private void SetEvent(GameObject g, RelicData relic)
    {
        Utils.RemoveAllEventFromObject(g);
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = relic.sprite;
        var button = g.GetComponent<Button>();
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                if (!relic) return;
                
                RelicManager.Instance.AddRelic(relic);
                SeManager.Instance.PlaySe("coin");
                g.transform.position = _disablePosition;
                EventManager.OnRelicObtainedTreasure.Trigger(relic);
                CloseTreasure();
            });
        }

        Utils.AddEventToObject(g, () => { 
            UIManager.Instance.ShowRelicDescriptionWindow(relic, g);
        }, EventTriggerType.PointerEnter);
    }
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        
        foreach (var item in items)
        {
            item.transform.position = _disablePosition;
        }
    }
}