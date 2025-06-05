using UnityEngine;

/// <summary>
/// 壁の弾性を追加するレリック
/// 新しい安全なイベントシステムを使用したバージョン
/// </summary>
public class AddElasticityToWall : RelicBase
{
    private PhysicsMaterial2D _pm;

    protected override void RegisterEffects()
    {
        // 物理マテリアルの設定
        _pm = MergeManager.Instance.GetWallMaterial();
        _pm.bounciness = 0.8f;
        UI?.ActiveAlways();
    }

    public override void RemoveAllEffects()
    {
        base.RemoveAllEffects();
        if (_pm != null)
        {
            _pm.bounciness = 0;
        }
    }
}
