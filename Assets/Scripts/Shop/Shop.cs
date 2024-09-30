using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Shop : MonoBehaviour
{
    public static Shop instance;

    [SerializeField]
    private BallDataList allBallDataList;
    private List<BallData> currentItems = new List<BallData>();
    private int itemNum = 3;
    private Transform itemContainer => this.transform.Find("Items");
    private List<Vector3> positions = new List<Vector3>();

    public void SetItem(int count = 3)
    {
        if (count > itemNum) return;
        currentItems.Clear();
        currentItems.Add(allBallDataList.list[0]);
        SetOnClick(itemContainer.GetChild(0).gameObject, allBallDataList.list[0]);
        currentItems.Add(allBallDataList.list[1]);
        SetOnClick(itemContainer.GetChild(1).gameObject, allBallDataList.list[1]);
        currentItems.Add(allBallDataList.list[2]);
        SetOnClick(itemContainer.GetChild(2).gameObject, allBallDataList.list[2]);
        // for (int i = 0; i < count; i++)
        // {
        //     float total = 0;
        //     foreach (ItemData itemData in items)
        //     {
        //         total += itemData.probability;
        //     }
        //     float randomPoint = GameManager.instance.RandomRange(0.0f, total);

        //     foreach (ItemData itemData in items)
        //     {
        //         if (randomPoint < itemData.probability)
        //         {
        //             var g = Instantiate(itemData.prefab, shopOptions[i].transform);
        //             g.transform.localScale = Vector3.one;
        //             currentItems.Add(g);
        //             SetOnClick(g);
        //             g.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        //             break;
        //         }
        //         randomPoint -= itemData.probability;
        //     }
        // }

        // shopBG.DOFade(0, 0);
        // shopBG.DOFade(1, 0.5f);
    }

    public void ResetItem()
    {
        currentItems.Clear();
        // shopBG.DOFade(0, 0.5f);
    }

    private void SetOnClick(GameObject g, BallData ball)
    {
        Button button = g.GetComponent<Button>();
        TextMeshProUGUI name = g.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        name.text = ball.name;
        TextMeshProUGUI price = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        price.text = ball.price.ToString();
        Image image = g.transform.Find("BallIcon").GetComponent<Image>();
        image.sprite = ball.sprite;
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                var item = ball;
                if (item == null) return;
                if (GameManager.instance.player.gold >= item.price)
                {
                    GameManager.instance.player.AddGold(-item.price);
                    // TODO: Add item to inventory
                    // Destroy(g);
                    // currentItems.Remove(g);
                    SeManager.instance.PlaySe("coin");
                }
                else
                {
                    SeManager.instance.PlaySe("error");
                }
            });
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        float offset = 220;
        positions.Add(this.transform.position + new Vector3(-offset, 0, 0));
        positions.Add(this.transform.position);
        positions.Add(this.transform.position + new Vector3(offset, 0, 0));
    }
}
