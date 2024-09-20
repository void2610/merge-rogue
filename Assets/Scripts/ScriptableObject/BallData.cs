using UnityEngine;

[CreateAssetMenu(fileName = "BallData", menuName = "Scriptable Objects/BallData")]
public class BallData : ScriptableObject
{
    public enum BallRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public int id;
    public string className;
    public string name;
    public string description;
    public int price;
    public Sprite sprite;
    public BallRarity rarity;
}
