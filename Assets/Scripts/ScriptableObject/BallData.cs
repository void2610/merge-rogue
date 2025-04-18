using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BallData", menuName = "Scriptable Objects/BallData")]
public class BallData : ScriptableObject
{
    public string className;
    public string displayName;
    [TextArea(1, 5)]
    public List<string> descriptions = new (){"", "", ""};
    [TextArea(1, 5)]
    public string flavorText;
    public Sprite sprite;
    public Rarity rarity;
    public BallShapeType shapeType = BallShapeType.Circle;
    public List<float> attacks = new (){0, 0, 0};
    public List<float> sizes = new (){1, 1, 1};
    public bool availableDemo = false;
}
