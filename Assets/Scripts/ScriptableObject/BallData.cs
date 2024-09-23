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
    public BallRarity rarity;
    public int price;
}
