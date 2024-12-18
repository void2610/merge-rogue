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
    [SerializeField] private float containerSize = 1;
    
    private void SetBallData(GameObject g, BallData b)
    {
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = b.sprite;
        if (b.sprite == null)
            image.color = new Color(0, 0, 0, 0);
        
        // イベントを登録
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.ShowBallDescriptionWindow(b,
                g.transform.position + new Vector3(2.7f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.HideBallDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }
    
    private void SetRelicData(GameObject g, RelicData r)
    {
        var image = g.transform.Find("Icon").GetComponent<Image>();
        image.sprite = r.sprite;
        
        // イベントを登録
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.ShowRelicDescriptionWindow(r,
                g.transform.position + new Vector3(2.7f, 0, 0));
        }, EventTriggerType.PointerEnter);
        Utils.AddEventToObject(g,  () =>
        {
            TitleMenu.Instance.HideRelicDescriptionWindow();
        }, EventTriggerType.PointerExit);
    }

    private void Start()
    {
        if(allBallDataList.list.Count == 0) return;

        var adjustedAlignX = align.x;
        var adjustedAlignY = align.y;

        var nowColumn = 0;

        for (var i = allBallDataList.list.Count - 1; i >= 0; i--)
        {
            var pos = new Vector3((i % column) * adjustedAlignX, -(i / column) * adjustedAlignY, 0) + new Vector3(offset.x, offset.y, 0);
            var container = Instantiate(ballContainerPrefab, this.transform.position + pos, Quaternion.identity, itemContainer);
            container.transform.localScale = new Vector3(containerSize, containerSize, containerSize);
            SetBallData(container, allBallDataList.list[i]);
            nowColumn = i / column;
        }

        nowColumn += 2;
        
        for (var i = allRelicDataList.list.Count - 1; i >= 0; i--)
        {
            var pos = new Vector3((i % column) * adjustedAlignX, -((i / column) + nowColumn) * adjustedAlignY, 0) + new Vector3(offset.x, offset.y, 0);
            var container = Instantiate(relicContainerPrefab, this.transform.position + pos, Quaternion.identity, itemContainer);
            container.transform.localScale = new Vector3(containerSize, containerSize, containerSize);
            SetRelicData(container, allRelicDataList.list[i]);
        }
    }
}
