using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RelicData", menuName = "Scriptable Objects/RelicData")]
public class RelicData : ScriptableObject
{
    public int id;
    public string className;
    public Sprite sprite;
    public string displayName;
    [TextArea(1, 5)]
    public string description;
    [TextArea(1, 5)]
    public string flavorText;
    public Rarity rarity;
}
