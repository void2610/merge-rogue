using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using unityroom.Api;
using TMPro;
using UnityEngine.EventSystems;

public class Encyclopedia : MonoBehaviour
{
    [SerializeField] private BallDataList allBallDataList;
    [SerializeField] private RelicDataList allRelicDataList;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject ballContainerPrefab;
    [SerializeField] private GameObject relicContainerPrefab;
    [SerializeField] private SerializableDictionary<BallShapeType, Sprite> ballBaseImages;
    [SerializeField] private int numColumns = 15; // 列数
    [SerializeField] private Selectable cloceButton;
    // 空白セルの高さ（例えば、セルの高さと同じか、調整したい値）
    [SerializeField] private float spacerHeight = 100f;

    private List<GameObject> _items = new();

    private void SetBallData(GameObject g, BallData b)
    {
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = b.sprite;
        image.gameObject.AddComponent<ImageShinyEffect>().SetColor(b.rarity);
        
        // イベントを登録
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.ShowDescriptionWindow(b, g);
        }, EventTriggerType.PointerEnter);
        
        var d = g.AddComponent<ShowDescription>();
        d.isBall = true;
        d.ballData = b;
    }
    
    private void SetRelicData(GameObject g, RelicData r)
    {
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = r.sprite;
        image.gameObject.AddComponent<ImageShinyEffect>().SetColor(r.rarity);
        
        // イベントを登録
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.ShowDescriptionWindow(r, g);
        }, EventTriggerType.PointerEnter);
        
        var d = g.AddComponent<ShowDescription>();
        d.isBall = false;
        d.relicData = r;
    }
    
    private void CreateSpacer(int count)
    {
        for(var i = 0; i < count; i++)
        {
            // 空のGameObjectを生成
            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(itemContainer, false);

            // LayoutElementを追加して、スペースとしてのサイズを指定
            var le = spacer.AddComponent<LayoutElement>();
            // グリッドの方向に合わせたPreferred値を設定する
            le.preferredHeight = spacerHeight;
        }
    }

    private void Start()
    {
        // Ball アイテムの生成
        foreach (var ball in allBallDataList.list)
        {
            var container = Instantiate(ballContainerPrefab, itemContainer);
            SetBallData(container, ball);
            
            #if DEMO_PLAY
                container.transform.Find("LockIcon").gameObject.SetActive(!ball.availableDemo);
                if(!ball.availableDemo)
                    container.transform.Find("BallBase").GetComponent<Image>().color =  new Color(0.4352941f, 0.4352941f, 0.4352941f, 0.5f);
                container.GetComponent<Image>().color = ball.availableDemo ? new Color(0.4352941f, 0.4352941f, 0.4352941f, 1) : new Color(0.4352941f, 0.4352941f, 0.4352941f, 0.5f);
            #else
                container.transform.Find("LockIcon").gameObject.SetActive(false);
            #endif
            
            _items.Add(container);
        }

        // ボールとレリックの間に空白セル（Spacer）を挟む
        CreateSpacer(numColumns + numColumns - (allBallDataList.list.Count % numColumns));

        // Relic アイテムの生成
        foreach (var relic in allRelicDataList.list)
        {
            var container = Instantiate(relicContainerPrefab, itemContainer);
            SetRelicData(container, relic);
            
            #if DEMO_PLAY
                container.transform.Find("LockIcon").gameObject.SetActive(!relic.availableDemo);
                if(!relic.availableDemo)
                    container.transform.Find("Icon").GetComponent<Image>().color =  new Color(0.4352941f, 0.4352941f, 0.4352941f, 0.5f);
                container.GetComponent<Image>().color = relic.availableDemo ? new Color(0.4352941f, 0.4352941f, 0.4352941f, 1) : new Color(0.4352941f, 0.4352941f, 0.4352941f, 0.5f);
            #else
                container.transform.Find("LockIcon").gameObject.SetActive(false);
            #endif
            
            _items.Add(container);
        }
        
        // cloceボタンへのナビゲーション
        for (var i = _items.Count - numColumns; i < _items.Count; i++)
        {
            var s = _items[i].GetComponent<Selectable>();
            
            var right = i + 1 < _items.Count ? _items[i + 1].GetComponent<Selectable>() : cloceButton;
            
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = _items[i - numColumns].GetComponent<Selectable>(),
                selectOnDown = cloceButton,
                selectOnLeft = _items[i - 1].GetComponent<Selectable>(),
                selectOnRight = right,
            };
            s.navigation = nav;
        }
        
        // レイアウト更新
        Canvas.ForceUpdateCanvases();
    }
}
