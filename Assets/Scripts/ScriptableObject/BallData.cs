using UnityEngine;

[CreateAssetMenu(fileName = "BallData", menuName = "Scriptable Objects/BallData")]
public class BallData : ScriptableObject
{
    public int id;
    public string className;
    public Sprite sprite;
    public Rarity rarity;
    public int price;
}
