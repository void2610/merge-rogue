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
    public List<float> attacks = new (){0, 0, 0};
    public List<float> sizes = new (){1, 1, 1};
    public List<float> weights = new (){1, 1, 1};
    public bool availableDemo = false;
    
    // ローカライズされた名前を取得
    public string GetDisplayName() => LocalizeStringLoader.Instance.Get($"{className}_N");

    public string GetDescription() => LocalizeStringLoader.Instance.Get($"{className}_D");

    public string GetFlavorText() => LocalizeStringLoader.Instance.Get($"{className}_F");
}
