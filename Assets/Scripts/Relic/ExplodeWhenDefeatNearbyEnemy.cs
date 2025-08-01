using UnityEngine;
using R3;
using System.Linq;

public class ExplodeWhenDefeatNearbyEnemy : RelicBase
{
    private bool _isExploding; // 爆発連鎖を防ぐフラグ
    
    public override void RegisterEffects()
    {
        // 敵が倒された時のイベント購読
        AddSubscription(EventManager.OnEnemyDefeated.Subscribe(OnEnemyDefeated));
    }
    
    private void OnEnemyDefeated(EnemyBase defeatedEnemy)
    {
        if (!defeatedEnemy || !defeatedEnemy.gameObject) return;
        
        // 爆発連鎖を防ぐ
        if (_isExploding) return;
        
        if (!IsNearbyEnemy(defeatedEnemy)) return;
        
        // 爆発開始フラグを設定
        _isExploding = true;
        
        // 爆発エフェクトを発動
        ExplodeAroundEnemy(defeatedEnemy);
        UI?.ActivateUI();
        
        // 爆発終了フラグをリセット
        _isExploding = false;
    }
    
    /// <summary>
    /// 倒された敵が距離2以内かチェック
    /// </summary>
    private bool IsNearbyEnemy(EnemyBase enemy)
    {
        if (!enemy || !EnemyContainer.Instance) return false;
        
        var distance = EnemyContainer.Instance.GetEnemyIndex(enemy);
        return distance is >= 0 and <= 2;
    }
    
    /// <summary>
    /// 倒された敵の周囲1マスに爆発ダメージを与える
    /// </summary>
    private void ExplodeAroundEnemy(EnemyBase defeatedEnemy)
    {
        if (!EnemyContainer.Instance) return;
        
        var defeatedDistance = EnemyContainer.Instance.GetEnemyIndex(defeatedEnemy);
        if (defeatedDistance < 0) return;
        
        // nullエントリを除外してクラッシュを防ぐ
        var allEnemies = EnemyContainer.Instance.GetAllEnemies().Where(e => e != null).ToList();
        
        foreach (var enemy in allEnemies)
        {
            if (!enemy || !enemy.gameObject || enemy == defeatedEnemy) continue;
            
            var enemyDistance = EnemyContainer.Instance.GetEnemyIndex(enemy);
            if (enemyDistance < 0) continue;
            
            // 周囲1マス以内の敵にダメージ
            var distance = Mathf.Abs(enemyDistance - defeatedDistance);
            
            if (distance <= 1)
            {
                // 敵が既に死んでいる場合はスキップ
                if (enemy.Health <= 0) continue;
                
                enemy.Damage(AttackType.Normal, 100);
            }
        }
    }
}