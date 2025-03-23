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

    private TextMeshProUGUI NameText => transform.Find("Canvas").transform.Find("NameText").GetComponent<TextMeshProUGUI>();
    private TextMeshProUGUI PriceText => transform.Find("Canvas").transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
    private Image Image => transform.Find("Canvas").transform.Find("Image").GetComponent<Image>();


    public virtual void Use(Player p) { }

    protected virtual void Awake()
    {
        NameText.text = itemName;
        PriceText.text = price.ToString();
        Image.sprite = icon;
        // image.GetComponent<OverRayWindow>().text = description;
    }
}
