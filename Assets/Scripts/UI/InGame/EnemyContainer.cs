using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EnemyContainer : SingletonMonoBehaviour<EnemyContainer>
{
    [System.Serializable]
    internal class EnemyData
    {
        public GameObject prefab;
        public float probability;
    }
    
    [SerializeField] private GameObject enemyBasePrefab;
    [SerializeField] private GameObject enemyHpSliderPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float alignment = 3;
    [SerializeField] private Treasure treasure;
    public readonly ReactiveProperty<int> DefeatedEnemyCount = new(0);
    // 近接用レーン（柔軟なサイズ）
    private List<EnemyBase> _meleeEnemies;
    // 遠隔用レーン（柔軟なサイズ）
    private List<EnemyBase> _rangedEnemies;
    private const int MAX_ENEMY_COUNT = 6;
    private int _gainedExp;
    private int _pendingSpawnCount; // スポーン待ちの敵の数
    private int _spawnStage; // スポーンする敵のステージレベル
    
    private IContentService _contentService;
    private IRandomService _randomService;
    private IEnemyDifficultyService _difficultyService;
    
    protected override void Awake()
    {
        base.Awake();
        // 各レーンを柔軟サイズで初期化（全体で6体まで可能）
        _meleeEnemies = new List<EnemyBase>(new EnemyBase[MAX_ENEMY_COUNT]);
        _rangedEnemies = new List<EnemyBase>(new EnemyBase[MAX_ENEMY_COUNT]);
    }
    
    [Inject]
    public void InjectDependencies(IContentService contentService, IRandomService randomService, IEnemyDifficultyService difficultyService)
    {
        _contentService = contentService;
        _randomService = randomService;
        _difficultyService = difficultyService;
    }

    public int GetCurrentEnemyCount() => _meleeEnemies.Count(e => e) + _rangedEnemies.Count(e => e);
    public int GetRemainingEnemyCount() => GetCurrentEnemyCount() + _pendingSpawnCount;
    public List<EnemyBase> GetAllEnemies() => 
        _meleeEnemies.Where(e => e).Concat(_rangedEnemies.Where(e => e)).ToList();
    public EnemyBase GetRandomEnemy() 
    {
        var validEnemies = GetAllEnemies();
        return validEnemies.Count > 0 ? validEnemies[_randomService.RandomRange(0, validEnemies.Count)] : null;
    }
    
    /// <summary>
    /// 敵のレーン内インデックスを取得（各レーン内で0-5）
    /// </summary>
    public int GetEnemyIndex(EnemyBase enemy)
    {
        var meleeIndex = _meleeEnemies.IndexOf(enemy);
        if (meleeIndex >= 0) return meleeIndex;
        
        var rangedIndex = _rangedEnemies.IndexOf(enemy);
        if (rangedIndex >= 0) return rangedIndex;
        
        return -1;
    }
    
    /// <summary>
    /// 敵が近接タイプかどうか判定
    /// </summary>
    private bool IsMeleeEnemy(EnemyBase enemy) => enemy.IsMelee;
    
    /// <summary>
    /// EnemyDataから近接タイプかどうか判定
    /// </summary>
    private bool IsMeleeEnemyByData(global::EnemyData enemyData)
    {
        return enemyData.isMelee;
    }
    public GameObject GetHpSliderPrefab() => enemyHpSliderPrefab;
    public GameObject GetCoinPrefab() => coinPrefab;

    public void SpawnBoss(int stage)
    {
        // 最も後ろの空いている位置を見つける（スポーン前にチェック）
        var spawnIndex = -1;
        
        // ボスのタイプに応じてスポーン位置を決定
        var bossData = _contentService.GetRandomBoss();
        // 近接・遠隔の判定
        var isMelee = IsMeleeEnemyByData(bossData);
        var targetLane = isMelee ? _meleeEnemies : _rangedEnemies;
        
        // 後方から空いている位置を探す
        for (var i = MAX_ENEMY_COUNT - 1; i >= 0; i--)
        {
            if (!targetLane[i])
            {
                spawnIndex = i;
                break;
            }
        }
        
        // 空いている位置がない場合は、スポーンをスキップ
        if (spawnIndex == -1) return;
        
        // ボスの場合はEnemyBaseのサブクラスを取得して使用する
        var e = Instantiate(enemyBasePrefab, this.transform);
        Destroy(e.GetComponent<EnemyBase>());
        var type = Type.GetType(bossData.className);
        var behaviour = e.AddComponent(type) as EnemyBase;
        
        if (!behaviour) return;
        
        e.transform.localScale = new Vector3(1, 1, 1);
        
        // ボスの初期化
        int currentAct = _contentService.Act;
        behaviour.Init(bossData, stage, _randomService, _difficultyService, currentAct);
        
        targetLane[spawnIndex] = behaviour;
        
        // 全ての敵の位置を更新（スポーン時は即座に配置）
        UpdateAllEnemyPositions(true);
    }

    /// <summary>
    /// スポーン待ちの敵を設定する
    /// </summary>
    public void SpawnEnemy(int count, int stage)
    {
        if (count <= 0) return;
        
        _pendingSpawnCount = count;
        _spawnStage = stage;
        
        // 最初の最大3体は即座にスポーン（ステージ開始時）
        var initialSpawnCount = Math.Min(3, _pendingSpawnCount);
        for (var i = 0; i < initialSpawnCount; i++)
        {
            if (SpawnSingleEnemy(_spawnStage))
            {
                _pendingSpawnCount--;
            }
            else
            {
                // スポーンに失敗した場合はループを抜ける（スペースがない）
                break;
            }
        }
    }
    
    /// <summary>
    /// 敵を1体だけスポーンする
    /// </summary>
    /// <returns>スポーンに成功した場合はtrue、失敗した場合はfalse</returns>
    public bool SpawnSingleEnemy(int stage)
    {
        // 最も後ろの空いている位置を見つける（スポーン前にチェック）
        var spawnIndex = -1;
        
        // 敵のタイプに応じてスポーン位置を決定
        var enemyData = _contentService.GetRandomEnemy();
        // 近接・遠隔の判定
        var isMelee = IsMeleeEnemyByData(enemyData);
        var targetLane = isMelee ? _meleeEnemies : _rangedEnemies;
        
        // 後方から空いている位置を探す
        for (var i = MAX_ENEMY_COUNT - 1; i >= 0; i--)
        {
            if (!targetLane[i])
            {
                spawnIndex = i;
                break;
            }
        }
        
        // 空いている位置がない場合は、スポーンをスキップ
        if (spawnIndex == -1)
        {
            // スポーン待ちカウントを維持（次のターンで再試行）
            return false;
        }
        
        // スポーン可能な場合のみ敵を作成
        var e = Instantiate(enemyBasePrefab, this.transform).GetComponent<EnemyBase>();
        e.transform.localScale = new Vector3(1, 1, 1);
        
        // 敵の初期化
        var currentAct = _contentService.Act;
        e.Init(enemyData, stage, _randomService, _difficultyService, currentAct);
        
        targetLane[spawnIndex] = e;
        
        // 全ての敵の位置を更新（スポーン時は即座に配置）
        UpdateAllEnemyPositions(true);
        
        return true;
    }
    
    public void DamageAllEnemies(int damage)
    {
        foreach (var e in _meleeEnemies)
            e?.Damage(AttackType.All, damage);
        foreach (var e in _rangedEnemies)
            e?.Damage(AttackType.All, damage);
    }
    
    public void HealAllEnemies(int heal)
    {
        foreach (var e in _meleeEnemies)
            e?.Heal(heal);
        foreach (var e in _rangedEnemies)
            e?.Heal(heal);
    }
    
    public void HealEnemy(int index, int heal)
    {
        if (index < 0) return;
        
        // 近接レーンの範囲内かチェック
        if (index < _meleeEnemies.Count && _meleeEnemies[index])
        {
            _meleeEnemies[index].Heal(heal);
            return;
        }
        
        // 遠隔レーンの範囲内かチェック
        if (index < _rangedEnemies.Count && _rangedEnemies[index])
        {
            _rangedEnemies[index].Heal(heal);
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        var enemyBase = enemy.GetComponent<EnemyBase>();
        DefeatedEnemyCount.Value++;
        _gainedExp += enemyBase.Exp;
        
        // リストから削除（nullに置き換え）
        var meleeIndex = _meleeEnemies.IndexOf(enemyBase);
        if (meleeIndex >= 0)
        {
            _meleeEnemies[meleeIndex] = null;
        }
        else
        {
            var rangedIndex = _rangedEnemies.IndexOf(enemyBase);
            if (rangedIndex >= 0)
            {
                _rangedEnemies[rangedIndex] = null;
            }
        }
        
        enemyBase.OnDisappear();

        // 全ての敵を倒し、スポーン予定の敵もいなければステージ進行
        if (GetRemainingEnemyCount() == 0)
        {
            // リストを再初期化（Clearは不要、新しいリストで上書き）
            _meleeEnemies = new List<EnemyBase>(new EnemyBase[MAX_ENEMY_COUNT]);
            _rangedEnemies = new List<EnemyBase>(new EnemyBase[MAX_ENEMY_COUNT]);
            
            Physics2D.simulationMode = SimulationMode2D.Script;
            
            if (StageManager.CurrentStage.Type == StageType.Boss)
                treasure.OpenTreasure(Treasure.TreasureType.Boss);
            else EndBattle().Forget();
        }
    }
    
    public async UniTaskVoid EndBattle()
    {
        MergeManager.Instance.EndMerge().Forget();
        await UniTask.Delay(2000);
        GameManager.Instance.Player.OnBattleEnd();
        GameManager.Instance.Player.AddExp(_gainedExp);
        _gainedExp = 0;
        BgmManager.Instance.PlayRandomBGM(BgmType.Other).Forget();
        
        // ボスを倒した時
        if (StageManager.CurrentStage.Type == StageType.Boss)
        {
            GameManager.Instance.Player.HealToFull();
            _contentService.AddAct();
            Debug.Log(_contentService.Act);
        }
    }
    
    public async UniTask AttackEnemy(AttackType type, int atk)
    {
        var es = GetAllEnemies().ToList();
        
        switch (type)
        {
            case AttackType.Normal:
                // 一番前の敵を攻撃、攻撃力が残っていたら次の敵を攻撃
                var singleDamage = atk;
                for (var i = 0; i < es.Count; i++)
                {
                    var e = es[i];
                    if (!e) continue;
                    if (singleDamage <= 0) break;
                    var actualDamage = singleDamage > e.Health ? e.Health : singleDamage;
                    if (es.IndexOf(e) == es.Count - 1) actualDamage = singleDamage;
    
                    SeManager.Instance.PlaySe("playerAttack");
                    e.Damage(AttackType.Normal, actualDamage);
                    singleDamage -= actualDamage;
    
                    CameraMove.Instance.ShakeCamera(0.5f, actualDamage * 0.01f);
                    await UniTask.Delay(200);
                }
                break;
            case AttackType.Last:
                ParticleManager.Instance.HitParticle(new Vector3(-3.7f, 3.3f, 0));
                es.Last().Damage(type, atk);
                break;
            case AttackType.Random:
                ParticleManager.Instance.HitParticle(new Vector3(-3.7f, 3.3f, 0));
                GetRandomEnemy().Damage(type, atk);
                break;
            case AttackType.All:
                ParticleManager.Instance.AllHitParticle(new Vector3(-3.7f, 3.3f, 0));
                foreach (var e in es) e.Damage(type, atk);
                break;
        } 
        
        SeManager.Instance.PlaySe("playerAttack");
        CameraMove.Instance.ShakeCamera(0.5f, atk * 0.01f);
        
        await UniTask.Delay(250);
        
    }
    
    public void Action()
    {
        AttackPlayerAsync().Forget();
    }

    private async UniTaskVoid AttackPlayerAsync()
    {
        // 現在の敵がいる場合のみ行動処理を実行
        if(GetCurrentEnemyCount() > 0)
        {
            // 行動 - null参照を防ぐため事前に有効な敵のリストを取得
            var validEnemies = _meleeEnemies.Concat(_rangedEnemies)
                .Where(e => e != null && e.gameObject).ToList();
            foreach (var t in validEnemies)
            {
                t.Action();
                await UniTask.Delay(250);
            }
            
            await UniTask.Delay(100);
            
            // 状態異常を更新
            await GameManager.Instance.Player.UpdateStatusEffects();
            var allTasks = _meleeEnemies.Concat(_rangedEnemies)
                .Where(e => e != null).Select(e => e.UpdateStatusEffects());
            await UniTask.WhenAll(allTasks);
        }

        // 敵の行動が全て終わった後に新しい敵をスポーン
        if (_pendingSpawnCount > 0)
        {
            // スポーンに成功した場合のみカウントを減らす
            if (SpawnSingleEnemy(_spawnStage))
            {
                _pendingSpawnCount--;
            }
        }

        if(GetRemainingEnemyCount() > 0)
            GameManager.Instance.ChangeState(GameManager.GameState.Merge);
    }

    /// <summary>
    /// 敵をターンごとに移動させる（インデックスベース）
    /// </summary>
    public void MoveEnemyOneStep(EnemyBase enemy)
    {
        var currentIndex = GetEnemyIndex(enemy);
        
        if (currentIndex < 0) return;
        
        var isMelee = IsMeleeEnemy(enemy);
        var targetLane = isMelee ? _meleeEnemies : _rangedEnemies;
        var currentLaneIndex = targetLane.IndexOf(enemy);
        
        if (currentLaneIndex <= 0) return;
        if (targetLane[currentLaneIndex - 1]) return;
        
        // 敵を新しい位置に移動
        targetLane[currentLaneIndex] = null;
        targetLane[currentLaneIndex - 1] = enemy;
        
        // 全ての敵の位置を再計算
        UpdateAllEnemyPositions(false);
    }
    
    /// <summary>
    /// 全ての敵の位置を更新
    /// </summary>
    /// <param name="immediate">即座に配置するか（スポーン時）</param>
    private void UpdateAllEnemyPositions(bool immediate)
    {
        // 近接レーンの位置更新
        for (var i = 0; i < _meleeEnemies.Count; i++)
        {
            if (_meleeEnemies[i])
            {
                UpdateEnemyPosition(_meleeEnemies[i], i, true, immediate);
            }
        }
        
        // 遠隔レーンの位置更新
        for (var i = 0; i < _rangedEnemies.Count; i++)
        {
            if (_rangedEnemies[i])
            {
                UpdateEnemyPosition(_rangedEnemies[i], i, false, immediate);
            }
        }
    }
    
    /// <summary>
    /// 敵の位置を更新
    /// </summary>
    /// <param name="enemy">対象の敵</param>
    /// <param name="laneIndex">レーン内のインデックス</param>
    /// <param name="isMelee">近接レーンかどうか</param>
    /// <param name="immediate">即座に配置するか</param>
    private void UpdateEnemyPosition(EnemyBase enemy, int laneIndex, bool isMelee, bool immediate)
    {
        var screenPosition = isMelee ? laneIndex * 2 : laneIndex * 2 + 1;
        var distanceFromPlayer = screenPosition * alignment;
        var targetPosition = this.transform.position + new Vector3(distanceFromPlayer, enemy.Data.enemyYOffset, 0);
        
        if (immediate)
        {
            enemy.transform.position = targetPosition;
            enemy.UpdateHpBarPosition();
        }
        else
        {
            enemy.transform.DOMove(targetPosition, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnUpdate(enemy.UpdateHpBarPosition)
                .SetLink(enemy.gameObject);
        }
    }
}
