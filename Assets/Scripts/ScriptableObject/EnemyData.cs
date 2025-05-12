using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Serializable]
    public class EnemyBehaviorData
    {
        public float probability;
        public EnemyActionData actionData;
    }
    
    
    [Header("Enemy Info")]
    public string className;
    public string displayName;
    [TextArea(1, 5)]
    public string flavorText;

    [Header("Enemy Status")]
    public EnemyType enemyType;

    public int maxHealthMin;
    public int maxHealthMax;
    public int attack;
    public int interval;
    public int coin;
    public int exp;
    
    [Header("Enemy Actions")]
    public List<EnemyActionData> actions;
    
    [Header("Enemy Appearance")]
    public Texture2D spriteSheet;
    public List<Sprite> sprites;
    public int framePerSecond;
    public float hpSliderYOffset;
    public float enemyYOffset;
    
    // SpriteAnimatorの自動設定ボタン
    [ContextMenu("Refresh Frames")]
    public void RefreshFrames()
    {
#if UNITY_EDITOR
        if (!spriteSheet)
        {
            Debug.LogError("[EnemyData] spriteSheet が設定されていません。");
            return;
        }
        var path = AssetDatabase.GetAssetPath(spriteSheet);
        var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToList();
        sprites = assets;
        EditorUtility.SetDirty(this);
        Debug.Log($"[EnemyData] {sprites.Count} 枚のフレームを読み込みました。 (SpriteSheet: {spriteSheet.name})");
#endif
    }
}
