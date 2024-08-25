using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Shop : MonoBehaviour
{
    [System.Serializable]
    internal class ItemData
    {
        public GameObject prefab;
        public float probability;
    }

    [SerializeField]
    private Image shopBG;

    [SerializeField]
    private List<GameObject> shopOptions = new List<GameObject>();

    [SerializeField]
    private List<ItemData> items = new List<ItemData>();

    private List<GameObject> currentItems = new List<GameObject>();
    private int itemNum = 3;
    private float alignment = 3;

    private List<Vector3> positions = new List<Vector3>();

    public void SetItem(int count = 1)
    {
        if (count > itemNum) return;
        currentItems.Clear();
        for (int i = 0; i < count; i++)
        {
            float total = 0;
            foreach (ItemData itemData in items)
            {
                total += itemData.probability;
            }
            float randomPoint = GameManager.instance.RandomRange(0.0f, total);

            foreach (ItemData itemData in items)
            {
                if (randomPoint < itemData.probability)
                {
                    var g = Instantiate(itemData.prefab, shopOptions[i].transform);
                    g.transform.localScale = Vector3.one;
                    currentItems.Add(g);
                    // g.GetComponent<RectTransform>().position = positions[i];
                    // g.transform.localPosition = positions[i];
                    SetOnClick(g);
                    g.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(1, 0.5f);
                    break;
                }
                randomPoint -= itemData.probability;
            }
        }

        shopBG.DOFade(0, 0);
        shopBG.DOFade(1, 0.5f);
    }

    public void ResetItem()
    {
        foreach (GameObject g in currentItems)
        {
            g.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => Destroy(g));
        }
        currentItems.Clear();
        shopBG.DOFade(0, 0.5f);
    }

    private void SetOnClick(GameObject g)
    {
        Button b = g.transform.Find("Canvas").transform.Find("Image").gameObject.AddComponent<Button>();
        if (b != null)
        {
            b.onClick.AddListener(() =>
            {
                var item = g.GetComponent<ItemBase>();
                if (item == null) return;
                if (GameManager.instance.player.gold >= item.price)
                {
                    GameManager.instance.player.AddGold(-item.price);
                    item.Use(GameManager.instance.player);
                    Destroy(g);
                    currentItems.Remove(g);
                    SeManager.instance.PlaySe("coin");
                }
                else
                {
                    SeManager.instance.PlaySe("error");
                }
            });
        }

        // Vector3 defaultPos = g.transform.position;
        // EventTrigger trigger = g.AddComponent<EventTrigger>();
        // EventTrigger.Entry entry = new EventTrigger.Entry();
        // entry.eventID = EventTriggerType.PointerEnter;
        // entry.callback.AddListener((data) =>
        // {
        //     //上にバウンドする
        //     if (g.transform.position.y == defaultPos.y)
        //         g.transform.DOPunchPosition(new Vector3(0, 0.5f, 0), 0.2f, 1, 1).SetRelative(true).OnComplete(() =>
        //         {
        //             g.transform.position = defaultPos;
        //         });
        // });
        // trigger.triggers.Add(entry);
        // entry = new EventTrigger.Entry();
        // entry.eventID = EventTriggerType.PointerExit;
        // entry.callback.AddListener((data) =>
        // {
        //     g.transform.DOScale(defaultScale, 0.1f);
        // });
        // trigger.triggers.Add(entry);
    }

    private void Awake()
    {
        float a = 190;
        float offset = this.transform.parent.transform.position.x;
        positions.Add(this.transform.position + new Vector3(-a, 0, 0));
        positions.Add(this.transform.position);
        positions.Add(this.transform.position + new Vector3(a, 0, 0));
    }
}
