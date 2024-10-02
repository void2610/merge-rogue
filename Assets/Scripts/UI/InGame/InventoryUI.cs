using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private Sprite cursorSprite;
    private List<GameObject> items = new List<GameObject>();
    private float sizeCoefficient = 8f;
    private GameObject cursor;

    public void SetItem(List<GameObject> inventory)
    {
        items = inventory;
    }

    public void SetCursor(int index)
    {
        if (items.Count == 0 || index < 0 || index >= items.Count) return;
        cursor.transform.position = items[index].transform.position;
        float size = items[index].transform.localScale.x * sizeCoefficient;
        cursor.transform.localScale = new Vector3(size, size, 1);
    }

    private void Awake()
    {
        cursor = new GameObject("Cursor", typeof(SpriteRenderer));
        cursor.GetComponent<SpriteRenderer>().sprite = cursorSprite;
    }
}
