using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private Sprite cursorSprite;
    private List<GameObject> items = new List<GameObject>();
    private float sizeCoefficient = 8f;
    private GameObject cursor;
    private int cursorIndex = 0;

    public void SetItem(List<GameObject> inventory)
    {
        items = inventory;
    }

    public void SetCursor(int index)
    {
        if (items.Count == 0 || index < 0 || index >= items.Count) return;
        cursor.transform.DOMove(items[index].transform.position, 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
        float size = items[index].transform.localScale.x * sizeCoefficient;
        cursor.transform.DOScale(new Vector3(size, size, 1), 0.3f).SetUpdate(true).SetEase(Ease.OutQuint);
        cursorIndex = index;
    }

    public void EnableCursor(bool b)
    {
        cursor.GetComponent<SpriteRenderer>().enabled = b;
    }

    private void Awake()
    {
        cursor = new GameObject("Cursor", typeof(SpriteRenderer));
        cursor.GetComponent<SpriteRenderer>().sprite = cursorSprite;
        EnableCursor(false);
    }
}
