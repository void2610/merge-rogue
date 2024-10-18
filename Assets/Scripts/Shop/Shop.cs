using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Shop : MonoBehaviour
{
    private enum ShopState
    {
        NotSelected,
        Selected,
        Closed
    }
    public static Shop Instance;

    [SerializeField]
    private BallDataList allBallDataList;
    private List<BallData> currentItems = new List<BallData>();
    private const int ITEM_NUM = 3;
    private Transform itemContainer => this.transform.Find("Items");
    private ShopState state = ShopState.Closed;
    private int selectedItem = -1;

    public void OpenShop(int count = 3)
    {
        if (count > ITEM_NUM) return;
        state = ShopState.NotSelected;
        currentItems.Clear();
        currentItems.Add(allBallDataList.list[1]);
        SetEvent(itemContainer.GetChild(0).gameObject, currentItems[0], 0);
        currentItems.Add(allBallDataList.list[2]);
        SetEvent(itemContainer.GetChild(1).gameObject, currentItems[1], 1);
        currentItems.Add(allBallDataList.list[3]);
        SetEvent(itemContainer.GetChild(2).gameObject, currentItems[2], 2);
        // TODO: アイテムをランダムに選択
    }

    public void CloseShop()
    {
        state = ShopState.Closed;
        selectedItem = -1;
        for (int i = 0; i < ITEM_NUM; i++)
        {
            itemContainer.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            itemContainer.GetChild(i).DOScale(1, 0.2f).SetUpdate(true);
        }
        GameManager.Instance.GetComponent<InventoryUI>().EnableCursor(false);
    }

    public void BuyBall(int index)
    {
        if(selectedItem == -1) return;
        var item = currentItems[selectedItem];
        if (item == null) return;

        if (GameManager.Instance.coin.Value >= item.price)
        {
            Debug.Log(item.name + "を購入しました");
            GameManager.Instance.coin.Value -= item.price;
            InventoryManager.instance.SetBall(item, index + 1);
            SeManager.Instance.PlaySe("coin");

            itemContainer.GetChild(selectedItem).DOScale(1, 0.2f).SetUpdate(true);
            state = ShopState.NotSelected;
            GameManager.Instance.GetComponent<InventoryUI>().EnableCursor(false);
            GameManager.Instance.GetComponent<InventoryUI>().SetCursor(0);
            selectedItem = -1;
        }
        else
        {
            SeManager.Instance.PlaySe("error");
        }
    }

    private void SetEvent(GameObject g, BallData ball, int index)
    {
        var nameText = g.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        nameText.text = ball.name;
        var price = g.transform.Find("Price").GetComponent<TextMeshProUGUI>();
        price.text = ball.price.ToString();
        var image = g.transform.Find("BallIcon").GetComponent<Image>();
        image.sprite = ball.sprite;
        var button = g.GetComponent<Button>();
        if (button)
        {
            button.onClick.AddListener(() =>
            {
                var item = ball;
                if (!item) return;
                if (GameManager.Instance.coin.Value >= item.price)
                {
                    state = ShopState.Selected;
                    selectedItem = index;
                    g.transform.DOScale(1.2f, 0.2f).SetUpdate(true);
                    GameManager.Instance.GetComponent<InventoryUI>().EnableCursor(true);
                    SeManager.Instance.PlaySe("button");
                }
                else
                {
                    SeManager.Instance.PlaySe("error");
                }
            });
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (state == ShopState.NotSelected) return;

        for (int i = 0; i < ITEM_NUM; i++)
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