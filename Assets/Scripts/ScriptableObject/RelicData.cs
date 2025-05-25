using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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
    public bool availableDemo = false;
    
    // ローカライズされた名前を取得
    public string GetDisplayName() => LocalizeStringLoader.Instance.Get($"{className}_N");

    public string GetDescription() => LocalizeStringLoader.Instance.Get($"{className}_D");

    public string GetFlavorText() => LocalizeStringLoader.Instance.Get($"{className}_F");
}
