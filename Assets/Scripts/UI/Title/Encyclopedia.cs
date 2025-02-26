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
    
    // 空白セルの高さ（例えば、セルの高さと同じか、調整したい値）
    [SerializeField] private float spacerHeight = 100f;

    private List<GameObject> _items = new();

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
    
    private void CreateSpacer(int count)
    {
        for(var i = 0; i < count; i++)
        {
            // 空のGameObjectを生成
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(itemContainer, false);

            // LayoutElementを追加して、スペースとしてのサイズを指定
            LayoutElement le = spacer.AddComponent<LayoutElement>();
            // グリッドの方向に合わせたPreferred値を設定する
            le.preferredHeight = spacerHeight;
        }
    }

    private void Start()
    {
        // Grid Layout Group が itemContainer にアタッチされている前提で位置設定は不要

        // Ball アイテムの生成
        foreach (var ball in allBallDataList.list)
        {
            var container = Instantiate(ballContainerPrefab, itemContainer);
            SetBallData(container, ball);
            _items.Add(container);
        }

        // ボールとレリックの間に空白セル（Spacer）を挟む
        CreateSpacer(16);

        // Relic アイテムの生成
        foreach (var relic in allRelicDataList.list)
        {
            var container = Instantiate(relicContainerPrefab, itemContainer);
            SetRelicData(container, relic);
            _items.Add(container);
        }
        
        // レイアウト更新
        Canvas.ForceUpdateCanvases();
    }
}
