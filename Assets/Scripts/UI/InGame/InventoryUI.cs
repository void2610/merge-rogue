using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private GameObject cursor;
    private List<GameObject> items = new List<GameObject>();
    private const float SIZE_COEFFICIENT = 8f;
    private int cursorIndex = 0;

    public void SetItem(List<GameObject> inventory)
    {
        items = inventory;
    }

    public void SetCursor(int index)
    {
        if (items.Count == 0 || index < 0 || index >= items.Count) return;
        cursor.transform.DOMove(items[index].transform.position, 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
        var size = items[index].transform.localScale.x * SIZE_COEFFICIENT;
        cursor.transform.DOScale(new Vector3(size, size, 1), 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
        cursorIndex = index;
    }

    public void EnableCursor(bool b)
    {
        cursor.GetComponent<SpriteRenderer>().enabled = b;
    }

    private void Awake()
    {
        EnableCursor(false);
    }
}
