using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Shop : MonoBehaviour
{
    public enum ShopState
    {
        NotSelected,
        Selected,
        Closed
    }
    public static Shop instance;

    [SerializeField]
    private BallDataList allBallDataList;
    private List<BallData> currentItems = new List<BallData>();
    private int itemNum = 3;
    private Transform itemContainer => this.transform.Find("Items");
    private List<Vector3> positions = new List<Vector3>();
    private ShopState state = ShopState.Closed;
    private int selectedItem = -1;

    public void OpenShop(int count = 3)
    {
        if (count > itemNum) return;
        state = ShopState.NotSelected;
        currentItems.Clear();
        currentItems.Add(allBallDataList.list[0]);
        SetEvent(itemContainer.GetChild(0).gameObject, allBallDataList.list[0], 0);
        currentItems.Add(allBallDataList.list[1]);
        SetEvent(itemContainer.GetChild(1).gameObject, allBallDataList.list[1], 1);
        currentItems.Add(allBallDataList.list[2]);
        SetEvent(itemContainer.GetChild(2).gameObject, allBallDataList.list[2], 2);
        // TODO: アイテムをランダムに選択
    }

    public void CloseShop()
    {
        state = ShopState.Closed;
        selectedItem = -1;
        for (int i = 0; i < itemNum; i++)
        {
            itemContainer.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            itemContainer.GetChild(i).DOScale(1, 0.2f).SetUpdate(true);
        }
        GameManager.instance.GetComponent<InventoryUI>().EnableCursor(false);
    }

    public void BuyBall(int index)
    {
        var item = currentItems[selectedItem];
        if (item == null) return;

        if (GameManager.instance.coin >= item.price)
        {
            Debug.Log(item.name + "を購入しました");
            GameManager.instance.AddCoin(-item.price);
            InventoryManager.instance.SetBall(item, index + 1);
            SeManager.instance.PlaySe("coin");

            itemContainer.GetChild(selectedItem).DOScale(1, 0.2f).SetUpdate(true);
            state = ShopState.NotSelected;
            GameManager.instance.GetComponent<InventoryUI>().EnableCursor(false);
            GameManager.instance.GetComponent<InventoryUI>().SetCursor(0);
            selectedItem = -1;
        }
        else
        {
            SeManager.instance.PlaySe("error");
        }
    }

    private void SetEvent(GameObject g, BallData ball, int index)
    {
        TextMeshProUGUI name = g.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        name.text = ball.name;
        TextMeshProUGUI price = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        price.text = ball.price.ToString();
        Image image = g.transform.Find("BallIcon").GetComponent<Image>();
        image.sprite = ball.sprite;
        Button button = g.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                var item = ball;
                if (item == null) return;
                if (GameManager.instance.coin >= item.price)
                {
                    state = ShopState.Selected;
                    selectedItem = index;
                    g.transform.DOScale(1.2f, 0.2f).SetUpdate(true);
                    GameManager.instance.GetComponent<InventoryUI>().EnableCursor(true);
                    SeManager.instance.PlaySe("button");
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

    private void Update()
    {
        if (state == ShopState.NotSelected) return;

        for (int i = 0; i < itemNum; i++)
        {
            if (i == selectedItem)
            {
                itemContainer.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                itemContainer.GetChild(i).GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1);
                if (itemContainer.GetChild(i).localScale.x > 1)
                {
                    itemContainer.GetChild(i).DOScale(1, 0.2f).SetUpdate(true);
                }
            }
        }
    }
}
// var item = ball;
// if (item == null) return;
// if (GameManager.instance.coin >= item.price)
// {
//     Debug.Log(item.name + "を購入しました");
//     GameManager.instance.player.AddGold(-item.price);
//     // TODO: Add item to inventory
//     Destroy(g);
//     currentItems.Remove(item);
//     SeManager.instance.PlaySe("coin");
// }
// else
// {
//     SeManager.instance.PlaySe("error");
// }
