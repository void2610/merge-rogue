using UnityEngine;

[CreateAssetMenu(fileName = "BallData", menuName = "Scriptable Objects/BallData")]
public class BallData : ScriptableObject
{
    public int id;
    public string className;
    public string displayName;
    public string mainDescription;
    public string altDescription;
    public string flavorText;
    public Sprite sprite;
    public Rarity rarity;
    public float atk;
    public float size;
    public int price;
}
