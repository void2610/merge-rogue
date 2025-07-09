/// <summary>
/// 敵の難易度（倍率）を計算するサービス
/// </summary>
public interface IEnemyDifficultyService
{
    /// <summary>
    /// 敵の基本倍率を計算
    /// </summary>
    /// <param name="stage">ステージ数</param>
    /// <param name="act">Act数</param>
    /// <param name="enemyType">敵の種類</param>
    /// <returns>倍率</returns>
    float CalculateMagnification(int stage, int act, EnemyType enemyType);
    
    /// <summary>
    /// ヘルス倍率を計算
    /// </summary>
    /// <param name="baseMagnification">基本倍率</param>
    /// <param name="enemyType">敵の種類</param>
    /// <returns>ヘルス倍率</returns>
    float CalculateHealthMagnification(float baseMagnification, EnemyType enemyType);
    
    /// <summary>
    /// 攻撃力倍率を計算
    /// </summary>
    /// <param name="baseMagnification">基本倍率</param>
    /// <param name="enemyType">敵の種類</param>
    /// <returns>攻撃力倍率</returns>
    float CalculateAttackMagnification(float baseMagnification, EnemyType enemyType);
    
    /// <summary>
    /// 経験値・コイン倍率を計算
    /// </summary>
    /// <param name="baseMagnification">基本倍率</param>
    /// <param name="enemyType">敵の種類</param>
    /// <returns>経験値・コイン倍率</returns>
    float CalculateRewardMagnification(float baseMagnification, EnemyType enemyType);
}