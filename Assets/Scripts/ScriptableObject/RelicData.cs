using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RelicData", menuName = "Scriptable Objects/RelicData")]
public class RelicData : ScriptableObject
{
    public List<EffectTiming> timing;

    public int id;
    public string className;
    public Sprite sprite;
    public string displayName;
    public string description;
    public RelicRarity rarity;
    public int price;
}
