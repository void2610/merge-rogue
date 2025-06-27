using UnityEngine;
using R3;

public class ExpandMergeArea : RelicBase
{
    public override void RegisterEffects()
    {
        MergeManager.Instance.LevelUpWallWidth();
    }
}