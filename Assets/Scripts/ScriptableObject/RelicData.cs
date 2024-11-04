using UnityEngine;

[CreateAssetMenu(fileName = "RelicData", menuName = "Scriptable Objects/RelicData")]
public class RelicData : ScriptableObject
{
    public enum RelicRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public int id;
    public string className;
    public Sprite sprite;
    public string displayName;
    public string description;
    public RelicRarity rarity;
    public int price;
}
