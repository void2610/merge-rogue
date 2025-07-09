using UnityEngine;
using VContainer;

/// <summary>
/// 敵の難易度を計算するサービスの実装
/// </summary>
public class EnemyDifficultyService : IEnemyDifficultyService
{
    private readonly IContentService _contentService;
    
    [Inject]
    public EnemyDifficultyService(IContentService contentService)
    {
        _contentService = contentService;
    }
    
    public float CalculateMagnification(int stage, int act, EnemyType enemyType)
    {
        // 基本的な計算式（カスタマイズ可能）
        float baseMagnification = 1.0f;
        
        // ステージによる倍率
        baseMagnification += stage * GetStageMultiplier(enemyType);
        
        // Actによる倍率
        baseMagnification += act * GetActMultiplier(enemyType);
        
        // 敵種別による調整
        baseMagnification *= GetEnemyTypeMultiplier(enemyType);
        
        // ContentServiceから全体倍率を適用
        baseMagnification *= _contentService.GlobalEnemyDifficultyMultiplier;
        
        return Mathf.Max(0.00001f, baseMagnification);
    }
    
    public float CalculateHealthMagnification(float baseMagnification, EnemyType enemyType)
    {
        // ヘルス特有の倍率調整
        float healthMultiplier = enemyType switch
        {
            EnemyType.Normal => 1.0f,
            EnemyType.Minion => 0.8f,    // ミニオンはヘルスが低め
            EnemyType.MiniBoss => 1.2f,  // ミニボスは少し高め
            EnemyType.Boss => 1.5f,      // ボスはヘルスが高め
            _ => 1.0f
        };
        
        // ContentServiceから基本ヘルス倍率を適用
        return baseMagnification * healthMultiplier * _contentService.BaseEnemyHealthMultiplier;
    }
    
    public float CalculateAttackMagnification(float baseMagnification, EnemyType enemyType)
    {
        // 攻撃力特有の倍率調整
        float attackMultiplier = enemyType switch
        {
            EnemyType.Normal => 0.8f,     // 通常敵は攻撃力控えめ
            EnemyType.Minion => 0.6f,     // ミニオンは攻撃力が低い
            EnemyType.MiniBoss => 1.1f,   // ミニボスは少し高め
            EnemyType.Boss => 1.2f,       // ボスは攻撃力高め
            _ => 1.0f
        };
        
        // ContentServiceから基本攻撃力倍率を適用
        return baseMagnification * attackMultiplier * _contentService.BaseEnemyAttackMultiplier;
    }
    
    public float CalculateRewardMagnification(float baseMagnification, EnemyType enemyType)
    {
        // 報酬特有の倍率調整
        float rewardMultiplier = enemyType switch
        {
            EnemyType.Normal => 1.0f,
            EnemyType.Minion => 0.7f,     // ミニオンは報酬が少ない
            EnemyType.MiniBoss => 1.5f,   // ミニボスは多め
            EnemyType.Boss => 2.0f,       // ボスは報酬が多い
            _ => 1.0f
        };
        
        return baseMagnification * rewardMultiplier;
    }
    
    /// <summary>
    /// ステージごとの倍率を取得
    /// </summary>
    private float GetStageMultiplier(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Normal => 0.8f,     // 通常敵は穏やかな成長
            EnemyType.Minion => 0.6f,     // ミニオンは成長が遅い
            EnemyType.MiniBoss => 0.9f,   // ミニボスは中間
            EnemyType.Boss => 1.0f,       // ボスは標準的な成長
            _ => 0.8f
        };
    }
    
    /// <summary>
    /// Actごとの倍率を取得
    /// </summary>
    private float GetActMultiplier(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Normal => 0.3f,     // Actごとの成長は控えめ
            EnemyType.Minion => 0.2f,     // ミニオンは成長が小さい
            EnemyType.MiniBoss => 0.4f,   // ミニボスは中間
            EnemyType.Boss => 0.5f,       // ボスはActごとの成長が大きい
            _ => 0.3f
        };
    }
    
    /// <summary>
    /// 敵種別による基本倍率を取得
    /// </summary>
    private float GetEnemyTypeMultiplier(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Normal => 1.0f,
            EnemyType.Minion => 0.9f,     // ミニオンは基本的に弱い
            EnemyType.MiniBoss => 1.1f,   // ミニボスは少し強い
            EnemyType.Boss => 1.3f,       // ボスは基本的に強い
            _ => 1.0f
        };
    }
}