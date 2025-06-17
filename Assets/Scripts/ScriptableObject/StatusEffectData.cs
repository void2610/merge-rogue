using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectData", menuName = "StatusEffect/StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    [Header("基本情報")]
    public StatusEffectType type;
    public string className;
    public Sprite icon;
    public Color effectColor = Color.white;
    
    [Header("効果パラメータ")]
    public StatusEffectBase.EffectTiming timing = StatusEffectBase.EffectTiming.OnTurnEnd;
    public bool isPermanent = false;
    public bool stackable = true;
    
    [Header("視覚効果")]
    public string particleEffectName;
    public string soundEffectName;
    
    [Header("表示情報")]
    [TextArea(2, 5)]
    public string description;
    
    [Header("ローカライゼーション")]
    public string localizationKeyName;
    public string localizationKeyDesc;
    
    [Header("追加パラメータ")]
    [Tooltip("効果の強度に使用される基本値")]
    public float baseValue = 1.0f;
    [Tooltip("スタック毎の倍率")]
    public float stackMultiplier = 1.0f;
}