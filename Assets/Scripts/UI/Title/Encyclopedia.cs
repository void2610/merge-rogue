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
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2 align;
    [SerializeField] private int column = 3;
    
    private List<GameObject> _items = new ();
    
    private void SetBallData(GameObject g, BallData b)
    {
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = b.sprite;
        if (b.sprite == null)
            image.color = new Color(0, 0, 0, 0);
        
        // イベントを登録
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.ShowDescriptionWindow(b, g.transform.Find("BG").gameObject);
        }, EventTriggerType.PointerEnter);
    }
    
    private void SetRelicData(GameObject g, RelicData r)
    {
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = r.sprite;
        
        // イベントを登録
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.ShowDescriptionWindow(r, g);
        }, EventTriggerType.PointerEnter);
    }
    
    private void AdjustContentSize()
    {
        var contentRect = itemContainer.GetComponent<RectTransform>();
        var count = allBallDataList.list.Count + allRelicDataList.list.Count;
        var rows = Mathf.CeilToInt((float)count / column);
        var totalHeight = rows * (align.y + 300) + offset.y;

        // Contentの高さを設定
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0);
    }

    private void Start()
    {
        if(allBallDataList.list.Count == 0) return;
        
        var tempColumn = 0;
        
        var balls = allBallDataList.list;
        balls.Sort((a, b) => a.rarity.CompareTo(b.rarity));
        for (var i = balls.Count - 1; i >= 0; i--)
        {
            var pos = new Vector3((i % column) * align.x, -(i / column) * align.y, 0) + new Vector3(offset.x, offset.y, 0);
            var container = Instantiate(ballContainerPrefab, pos, Quaternion.identity, itemContainer);
            SetBallData(container, balls[i]);
            tempColumn = i / column;
            _items.Add(container);
        }

        tempColumn += 2;

        // レアリティでソート
        var relics = allRelicDataList.list;
        relics.Sort((a, b) => a.rarity.CompareTo(b.rarity));
        for (var i =　relics.Count - 1; i >= 0; i--)
        {
            var pos = new Vector3((i % column) * align.x, -((i / column) + tempColumn) * align.y, 0) + new Vector3(offset.x, offset.y, 0);
            var container = Instantiate(relicContainerPrefab, pos, Quaternion.identity, itemContainer);
            SetRelicData(container, relics[i]);
            _items.Add(container);
        }
        
        AdjustContentSize();
        // 謎に縦の位置がズレるので修正
        if (_items[0].GetComponent<RectTransform>().anchoredPosition.y > 300)
        {
            _items.ForEach(i =>  i.transform.Translate(new Vector3(0, -7, 0)));
        }
        Canvas.ForceUpdateCanvases();
    }
}
