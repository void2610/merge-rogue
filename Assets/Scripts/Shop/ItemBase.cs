using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemBase : MonoBehaviour
{
    public string itemName;
    public int price;
    [SerializeField]
    public Sprite icon;
    public string description;

    private TextMeshProUGUI nameText => transform.Find("Canvas").transform.Find("NameText").GetComponent<TextMeshProUGUI>();
    private TextMeshProUGUI priceText => transform.Find("Canvas").transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
    private Image image => transform.Find("Canvas").transform.Find("Image").GetComponent<Image>();


    public virtual void Use(Player p)
    {
        Debug.Log("Using item: " + itemName);
    }

    protected virtual void Awake()
    {
        nameText.text = itemName;
        priceText.text = price.ToString();
        image.sprite = icon;
        // image.GetComponent<OverRayWindow>().text = description;
    }
}
