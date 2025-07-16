using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 状態異常処理を一元管理する静的クラス
/// 全ての状態異常ロジックをswitch文で処理する
/// </summary>
public static class StatusEffectProcessor
{
    /// <summary>
    /// 指定した状態異常タイプのデータを取得する
    /// </summary>
    /// <param name="type">状態異常タイプ</param>
    /// <returns>状態異常データ、見つからない場合はnull</returns>
    private static StatusEffectData GetEffectData(StatusEffectType type)
    {
        var dataList = StatusEffectManager.Instance.GetStatusEffectDataList();
        return dataList.list.FirstOrDefault(d => d.type == type);
    }
    
    /// <summary>
    /// ターン終了時の状態異常処理を実行する
    /// OnTurnEndタイミングの効果を発動し、永続でない効果のスタック数を減少させる
    /// </summary>
    /// <param name="entity">処理対象のエンティティ</param>
    public static async UniTask ProcessTurnEnd(IEntity entity)
    {
        var toRemove = new List<StatusEffectType>();
        
        foreach (var kvp in entity.StatusEffectStacks.ToList())
        {
            var type = kvp.Key;
            var stacks = kvp.Value;
            var data = GetEffectData(type);
            
            if (data?.timing == StatusEffectTiming.OnTurnEnd)
            {
                ShowEffectText(entity, type);
                ProcessTurnEndEffect(entity, type, stacks);
                await UniTask.Delay((int)(500 * GameManager.Instance.TimeScale));
            }
            
            if (data != null && !data.isPermanent)
            {
                entity.StatusEffectStacks[type]--;
                if (entity.StatusEffectStacks[type] <= 0)
                {
                    toRemove.Add(type);
                }
            }
        }
        
        foreach (var type in toRemove)
        {
            entity.StatusEffectStacks.Remove(type);
        }
        
        UpdateUI(entity);
    }
    
    /// <summary>
    /// 受けるダメージを状態異常に基づいて修正する
    /// Shield、Invincibleなどのダメージ軽減・無効化効果を適用する
    /// </summary>
    /// <param name="entity">ダメージを受けるエンティティ</param>
    /// <param name="damage">元のダメージ値</param>
    /// <returns>状態異常による修正後のダメージ値</returns>
    public static int ModifyIncomingDamage(IEntity entity, int damage)
    {
        var effectsToProcess = entity.StatusEffectStacks.ToList();
        foreach (var kvp in effectsToProcess)
        {
            var type = kvp.Key;
            var stacks = kvp.Value;
            var data = GetEffectData(type);
            
            if (data?.timing == StatusEffectTiming.OnDamage)
            {
                ShowEffectText(entity, type, 1);
                damage = ProcessDamageModification(entity, type, stacks, damage);
            }
        }
        
        UpdateUI(entity);
        return damage;
    }
    
    /// <summary>
    /// 与える攻撃力を状態異常に基づいて修正する
    /// Power、Rageなどの攻撃力強化効果を適用する
    /// </summary>
    /// <param name="entity">攻撃するエンティティ</param>
    /// <param name="attackType">攻撃の種類</param>
    /// <param name="attack">元の攻撃力</param>
    /// <returns>状態異常による修正後の攻撃力</returns>
    public static int ModifyOutgoingAttack(IEntity entity, AttackType attackType, int attack)
    {
        var effectsToProcess = entity.StatusEffectStacks.ToList();
        foreach (var kvp in effectsToProcess)
        {
            var type = kvp.Key;
            var stacks = kvp.Value;
            var data = GetEffectData(type);
            
            if (data?.timing == StatusEffectTiming.OnAttack)
            {
                ShowEffectText(entity, type);
                attack = ProcessAttackModification(entity, type, stacks, attackType, attack);
            }
        }
        
        UpdateUI(entity);
        return attack;
    }
    
    /// <summary>
    /// 戦闘終了時の状態異常処理を実行する
    /// 全ての状態異常をクリアし、OnBattleEndタイミングの効果があれば発動する
    /// </summary>
    /// <param name="entity">処理対象のエンティティ</param>
    public static void OnBattleEnd(IEntity entity)
    {
        var effectsToProcess = entity.StatusEffectStacks.ToList();
        entity.StatusEffectStacks.Clear();
        
        foreach (var kvp in effectsToProcess)
        {
            var data = GetEffectData(kvp.Key);
            if (data?.timing == StatusEffectTiming.OnBattleEnd)
            {
                ShowEffectText(entity, kvp.Key);
                ProcessBattleEndEffect(entity, kvp.Key, kvp.Value);
            }
        }
        
        UpdateUI(entity);
    }
    
    /// <summary>
    /// エンティティに状態異常を追加する
    /// 既に同じ状態異常が存在する場合はスタック数を加算する
    /// </summary>
    /// <param name="entity">対象エンティティ</param>
    /// <param name="type">状態異常タイプ</param>
    /// <param name="stacks">追加するスタック数</param>
    public static void AddStatusEffect(IEntity entity, StatusEffectType type, int stacks)
    {
        if (!entity.StatusEffectStacks.TryAdd(type, stacks))
            entity.StatusEffectStacks[type] += stacks;

        UpdateUI(entity);
    }
    
    /// <summary>
    /// エンティティから状態異常を削除する
    /// 指定したスタック数を減算し、0以下になった場合は完全に削除する
    /// </summary>
    /// <param name="entity">対象エンティティ</param>
    /// <param name="type">状態異常タイプ</param>
    /// <param name="stacks">削除するスタック数</param>
    public static void RemoveStatusEffect(IEntity entity, StatusEffectType type, int stacks)
    {
        if (!entity.StatusEffectStacks.ContainsKey(type)) return;
        
        entity.StatusEffectStacks[type] -= stacks;
        if (entity.StatusEffectStacks[type] <= 0)
        {
            entity.StatusEffectStacks.Remove(type);
        }
        
        UpdateUI(entity);
    }
    
    /// <summary>
    /// ターン終了時タイミングの状態異常効果を個別に処理する
    /// Burn: ダメージ, Regeneration: 回復, Shock: 全敵にダメージ, Curse: 妨害球生成
    /// </summary>
    /// <param name="entity">効果を適用するエンティティ</param>
    /// <param name="type">状態異常タイプ</param>
    /// <param name="stacks">現在のスタック数</param>
    private static void ProcessTurnEndEffect(IEntity entity, StatusEffectType type, int stacks)
    {
        switch (type)
        {
            // 【Burn】毎ターン終了時にスタック数分のダメージを与える（継続ダメージ）
            // 効果: スタック数 = ダメージ量、永続でないため毎ターンスタック数-1
            case StatusEffectType.Burn:
                var burnDamage = stacks;
                entity.Damage(AttackType.Normal, burnDamage);
                break;
                
            // 【Regeneration】毎ターン終了時にスタック数分のHPを回復する（継続回復）
            // 効果: スタック数 = 回復量、永続でないため毎ターンスタック数-1
            case StatusEffectType.Regeneration:
                var healAmount = stacks;
                entity.Heal(healAmount);
                break;
                
            // 【Shock】敵専用 - 毎ターン終了時に全ての敵にスタック数分のダメージを与える
            // 効果: 感電した敵が他の敵にもダメージを連鎖させる、プレイヤーには無効
            case StatusEffectType.Shock:
                if (entity is EnemyBase)
                {
                    foreach (var enemy in EnemyContainer.Instance.GetAllEnemies())
                    {
                        enemy?.GetComponent<EnemyBase>()?.Damage(AttackType.Normal, stacks);
                    }
                }
                break;
                
            // 【Curse】プレイヤー専用 - 毎ターン終了時にスタック数分の妨害球を生成する
            // 効果: マージエリアに邪魔なボールを追加してプレイヤーの戦略を妨害、敵には無効
            case StatusEffectType.Curse:
                if (entity is Player)
                {
                    MergeManager.Instance.CreateDisturbBall(stacks);
                }
                break;
                
            // 【Decay】腐敗 - 毎ターン終了時にスタック数分のランダムなボールを消滅させる
            // 効果: プレイヤーのボールを直接削除してリソース圧迫、戦略的選択肢を制限する
            case StatusEffectType.Decay:
                if (entity is Player)
                {
                    for (var i = 0; i < stacks; i++)
                    {
                        MergeManager.Instance.RemoveRandomBall();
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// OnDamageタイミングの状態異常によるダメージ修正を処理する
    /// Invincible: 完全無効化, Shield: スタック分吸収して減少
    /// </summary>
    /// <param name="entity">ダメージを受けるエンティティ</param>
    /// <param name="type">状態異常タイプ</param>
    /// <param name="stacks">スタック数（参考値、実際の値は内部で再取得）</param>
    /// <param name="damage">元のダメージ値</param>
    /// <returns>修正後のダメージ値</returns>
    private static int ProcessDamageModification(IEntity entity, StatusEffectType type, int stacks, int damage)
    {
        switch (type)
        {
            // 【Invincible】完全無敵 - 受けるダメージを0にする
            // 効果: スタック数に関係なく全てのダメージを無効化、永続でないため毎ターンスタック数-1
            case StatusEffectType.Invincible:
                return 0;
                
            // 【Shield】シールド - スタック数分のダメージを吸収する
            // 効果: 吸収したダメージ分だけスタック数を即座に減少、0になると効果終了
            case StatusEffectType.Shield:
                var currentStacks = entity.StatusEffectStacks.GetValueOrDefault(StatusEffectType.Shield, 0);
                var absorbed = Mathf.Min(damage, currentStacks);
                if (absorbed > 0)
                {
                    RemoveStatusEffect(entity, StatusEffectType.Shield, absorbed);
                }
                return damage - absorbed;
                
            default:
                return damage;
        }
    }
    
    /// <summary>
    /// OnAttackタイミングの状態異常による攻撃力修正を処理する
    /// Power: 通常攻撃のみ加算強化, Rage: 全攻撃に乗算強化
    /// </summary>
    /// <param name="entity">攻撃するエンティティ</param>
    /// <param name="type">状態異常タイプ</param>
    /// <param name="stacks">現在のスタック数</param>
    /// <param name="attackType">攻撃の種類</param>
    /// <param name="attack">元の攻撃力</param>
    /// <returns>修正後の攻撃力</returns>
    private static int ProcessAttackModification(IEntity entity, StatusEffectType type, int stacks, AttackType attackType, int attack)
    {
        switch (type)
        {
            // 【Power】パワー - 通常攻撃のみスタック数分の攻撃力を加算する
            // 効果: Normal攻撃限定でスタック数をそのまま攻撃力に加算、永続でないため毎ターンスタック数-1
            case StatusEffectType.Power:
                if (attackType == AttackType.Normal)
                {
                    return attack + stacks;
                }
                break;
                
            // 【Rage】怒り - 全ての攻撃にスタック数×10%の倍率ボーナスを適用する
            // 効果: 攻撃タイプ問わず乗算強化、スタック3なら1.3倍、永続でないため毎ターンスタック数-1
            case StatusEffectType.Rage:
                return Mathf.RoundToInt(attack * (1f + 0.1f * stacks));
        }
        
        return attack;
    }
    
    /// <summary>
    /// OnBattleEndタイミングの状態異常効果を処理する
    /// 現在は特別な処理は実装されていないが、将来の拡張用に予約
    /// </summary>
    /// <param name="entity">対象エンティティ</param>
    /// <param name="type">状態異常タイプ</param>
    /// <param name="stacks">スタック数</param>
    private static void ProcessBattleEndEffect(IEntity entity, StatusEffectType type, int stacks)
    {
        // 現在のところ、戦闘終了時の特殊な処理は無い
    }
    
    /// <summary>
    /// エンティティがFreeze状態で行動をスキップするかチェックする
    /// スタック数に応じて凍結確率が上昇する（最大90%）
    /// </summary>
    /// <param name="entity">チェック対象のエンティティ</param>
    /// <returns>true: 凍結して行動スキップ, false: 通常行動</returns>
    public static bool CheckFreeze(IEntity entity)
    {
        if (!entity.StatusEffectStacks.TryGetValue(StatusEffectType.Freeze, out var stacks)) return false;

        // 【Freeze】凍結 - スタック数×10%の確率で行動をスキップする（最大90%）
        // 効果: 敵のAction()やプレイヤー行動前にチェック、永続でないため毎ターンスタック数-1
        var freezeChance = Mathf.Min(stacks * 0.1f, 0.9f);
        return Random.value < freezeChance;
    }
    
    /// <summary>
    /// エンティティがConfusion状態かどうかをチェックする
    /// 主にプレイヤーのカーソル制御混乱判定に使用される
    /// </summary>
    /// <param name="entity">チェック対象のエンティティ</param>
    /// <returns>true: 混乱状態, false: 正常状態</returns>
    public static bool IsConfused(IEntity entity)
    {
        // 【Confusion】混乱 - プレイヤー専用、カーソル制御を波打つように妨害する
        // 効果: MergeManager.ApplyConfusionToPosition()で位置計算を歪める、敵には無効
        // 永続でないため毎ターンスタック数-1、スタック数は混乱の強度に影響
        return entity.StatusEffectStacks.ContainsKey(StatusEffectType.Confusion);
    }
    
    /// <summary>
    /// 状態異常発動時のエフェクトテキストを画面に表示する
    /// エンティティの位置とプレイヤー/敵の判定に基づいて表示位置を決定
    /// </summary>
    /// <param name="entity">エフェクトを表示するエンティティ</param>
    /// <param name="type">表示する状態異常タイプ</param>
    /// <param name="priority">表示優先度（複数同時発動時の位置調整用）</param>
    private static void ShowEffectText(IEntity entity, StatusEffectType type, int priority = 0)
    {
        var position = entity switch
        {
            Player player => player.transform.position,
            EnemyBase enemy => enemy.transform.position,
            _ => Vector3.zero
        };
        
        var isPlayer = entity is Player;
        StatusEffectManager.Instance.ShowEffectText(type, position, isPlayer, priority);
    }
    
    /// <summary>
    /// エンティティの状態異常UIを最新の状態に更新する
    /// Player/EnemyBaseそれぞれのStatusEffectUIコンポーネントを呼び出す
    /// </summary>
    /// <param name="entity">UIを更新するエンティティ</param>
    private static void UpdateUI(IEntity entity)
    {
        if (entity is Player player && player.StatusEffectUI != null)
        {
            player.StatusEffectUI.UpdateUI(entity.StatusEffectStacks);
        }
        else if (entity is EnemyBase enemy && enemy.StatusEffectUI != null)
        {
            enemy.StatusEffectUI.UpdateUI(entity.StatusEffectStacks);
        }
    }
    
    /// <summary>
    /// 指定した状態異常に関連するサウンドエフェクトを再生する
    /// StatusEffectDataにsoundEffectNameが設定されている場合のみ再生
    /// </summary>
    /// <param name="type">サウンドを再生する状態異常タイプ</param>
    public static void PlaySoundEffect(StatusEffectType type)
    {
        var data = GetEffectData(type);
        if (data && !string.IsNullOrEmpty(data.soundEffectName))
        {
            SeManager.Instance.PlaySe(data.soundEffectName);
        }
    }
}