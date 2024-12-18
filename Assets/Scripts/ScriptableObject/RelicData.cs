using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RelicData", menuName = "Scriptable Objects/RelicData")]
public class RelicData : ScriptableObject
{
    public int id;
    public string className;
    public Sprite sprite;
    public string displayName;
    public string description;
    public string flavorText;
    public Rarity rarity;
    public int price;
}
