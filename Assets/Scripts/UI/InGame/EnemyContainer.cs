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
    [SerializeField] private int spawnDistance = 5; // 敵がスポーンするインデックス（後方から）
    public readonly ReactiveProperty<int> DefeatedEnemyCount = new(0);
    private List<EnemyBase> _currentEnemies;
    private const int MAX_ENEMY_COUNT = 6; // 最大敵数 (spawnDistance + 1)
    private int _gainedExp;
    private int _pendingSpawnCount; // スポーン待ちの敵の数
    private int _spawnStage; // スポーンする敵のステージレベル
    
    private IContentService _contentService;
    private IRandomService _randomService;
    private IObjectResolver _resolver;
    
    protected override void Awake()
    {
        base.Awake();
        // 固定サイズのリストを初期化（全てnullで埋める）
        _currentEnemies = new List<EnemyBase>(new EnemyBase[MAX_ENEMY_COUNT]);
    }
    
    [Inject]
    public void InjectDependencies(IContentService contentService, IRandomService randomService, IObjectResolver resolver)
    {
        _contentService = contentService;
        _randomService = randomService;
        _resolver = resolver;
    }

    public int GetCurrentEnemyCount() => _currentEnemies.Count(e => e);
    public int GetRemainingEnemyCount() => GetCurrentEnemyCount() + _pendingSpawnCount;
    public List<EnemyBase> GetAllEnemies() => _currentEnemies.Where(e => e).ToList();
    public EnemyBase GetRandomEnemy() 
    {
        var validEnemies = GetAllEnemies();
        return validEnemies.Count > 0 ? validEnemies[_randomService.RandomRange(0, validEnemies.Count)] : null;
    }
    public int GetEnemyIndex(EnemyBase enemy) => _currentEnemies.IndexOf(enemy);
    public GameObject GetHpSliderPrefab() => enemyHpSliderPrefab;
    public GameObject GetCoinPrefab() => coinPrefab;

    public void SpawnBoss(int stage)
    {
        // 最も後ろの空いている位置を見つける（スポーン前にチェック）
        var spawnIndex = -1;
        
        // 最低spawnDistance以降の位置を探す（MAX_ENEMY_COUNTまで）
        for (var i = spawnDistance; i < MAX_ENEMY_COUNT; i++)
        {
            if (!_currentEnemies[i])
            {
                spawnIndex = i;
                break;
            }
        }
        
        // 空いている位置がない場合は、スポーンをスキップ
        if (spawnIndex == -1)
        {
            return;
        }
        
        // スポーン可能な場合のみボスを作成
        var bossData = _contentService.GetRandomBoss();
        
        // ボスの場合はEnemyBaseのサブクラスを取得して使用する
        var e = Instantiate(enemyBasePrefab, this.transform);
        Destroy(e.GetComponent<EnemyBase>());
        var type = Type.GetType(bossData.className);
        var behaviour = e.AddComponent(type) as EnemyBase;
        
        if (!behaviour) return;
        
        // VContainerで依存性を注入
        _resolver.Inject(behaviour);
        
        e.transform.localScale = new Vector3(1, 1, 1);
        
        // ボスの初期化
        behaviour.Init(bossData, stage);
        
        _currentEnemies[spawnIndex] = behaviour;
        
        // 全ての敵の位置を更新（スポーン時は即座に配置）
        for (var i = 0; i < _currentEnemies.Count; i++)
        {
            if (_currentEnemies[i])
            {
                SetEnemyPositionImmediate(i);
            }
        }
    }

    /// <summary>
    /// スポーン待ちの敵を設定する
    /// </summary>
    public void SpawnEnemy(int count, int stage)
    {
        count = count > MAX_ENEMY_COUNT ? MAX_ENEMY_COUNT : count;
        if (count <= 0) return;
        
        _pendingSpawnCount = count;
        _spawnStage = stage;
        
        // 最初の1体は即座にスポーン（ステージ開始時）
        if (_pendingSpawnCount > 0)
        {
            if (SpawnSingleEnemy(_spawnStage))
            {
                _pendingSpawnCount--;
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
        
        // 最低spawnDistance以降の位置を探す（MAX_ENEMY_COUNTまで）
        for (var i = spawnDistance; i < MAX_ENEMY_COUNT; i++)
        {
            if (!_currentEnemies[i])
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
        var enemyData = _contentService.GetRandomEnemy();
        var e = Instantiate(enemyBasePrefab, this.transform).GetComponent<EnemyBase>();
        
        // VContainerで依存性を注入
        _resolver.Inject(e);
        
        e.transform.localScale = new Vector3(1, 1, 1);
        
        // 敵の初期化
        e.Init(enemyData, stage);
        
        _currentEnemies[spawnIndex] = e;
        
        // 全ての敵の位置を更新（スポーン時は即座に配置）
        for (var i = 0; i < _currentEnemies.Count; i++)
        {
            if (_currentEnemies[i])
            {
                SetEnemyPositionImmediate(i);
            }
        }
        
        return true;
    }
    
    public void DamageAllEnemies(int damage)
    {
        foreach (var e in _currentEnemies)
            e?.Damage(AttackType.All, damage);
    }
    
    public void HealAllEnemies(int heal)
    {
        foreach (var e in _currentEnemies)
            e?.Heal(heal);
    }
    
    public void HealEnemy(int index, int heal)
    {
        if (index < 0 || index >= _currentEnemies.Count || !_currentEnemies[index]) return;
        _currentEnemies[index].Heal(heal);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        var enemyBase = enemy.GetComponent<EnemyBase>();
        DefeatedEnemyCount.Value++;
        _gainedExp += enemyBase.Exp;
        
        // リストから削除（nullに置き換え）
        var index = _currentEnemies.IndexOf(enemyBase);
        if (index >= 0)
        {
            _currentEnemies[index] = null;
        }
        
        enemyBase.OnDisappear();

        // 全ての敵を倒し、スポーン予定の敵もいなければステージ進行
        if (GetRemainingEnemyCount() == 0)
        {
            // リストを再初期化（Clearは不要、新しいリストで上書き）
            _currentEnemies = new List<EnemyBase>(new EnemyBase[MAX_ENEMY_COUNT]);
            
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
            var validEnemies = _currentEnemies.Where(e => e != null && e.gameObject).ToList();
            foreach (var t in validEnemies)
            {
                t.Action();
                await UniTask.Delay(250);
            }
            
            await UniTask.Delay(100);
            
            // 状態異常を更新
            await GameManager.Instance.Player.UpdateStatusEffects();
            var tasks = _currentEnemies.Where(e => e != null).Select(e => e.UpdateStatusEffects());
            await UniTask.WhenAll(tasks);
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
        
        if (currentIndex < 0) 
        {
            return; // 見つからない場合は移動しない
        }
        
        // 1体だけの場合はインデックスベースで移動判定
        var actualEnemyCount = _currentEnemies.Count(e => e != null);
        if (actualEnemyCount == 1)
        {
            // 1体だけの場合は位置を直接移動
            var currentEnemyIndex = GetEnemyIndex(enemy);
            if (currentEnemyIndex > 0)
            {
                var newIndex = currentEnemyIndex - 1;
                _currentEnemies[currentEnemyIndex] = null;
                _currentEnemies[newIndex] = enemy;
                
                // 全ての敵の位置を更新
                for (var i = 0; i < _currentEnemies.Count; i++)
                {
                    if (_currentEnemies[i])
                    {
                        UpdateEnemyPosition(i);
                    }
                }
                
                return;
            }
        }
        
        if (currentIndex <= 0) 
        {
            return; // 既に最前列の場合は移動しない
        }
        
        // 前の位置に敵がいるかチェック（追い越し防止）
        var frontIndex = currentIndex - 1;
        if (_currentEnemies[frontIndex])
        {
            return;
        }
        
        
        // 敵を新しい位置に移動（nullエントリーに配慮したシンプルな移動）
        _currentEnemies[currentIndex] = null;
        _currentEnemies[currentIndex - 1] = enemy;
        
        // すべての敵の位置を再計算
        for (var i = 0; i < _currentEnemies.Count; i++)
        {
            if (_currentEnemies[i])
            {
                UpdateEnemyPosition(i);
            }
        }
    }
    
    /// <summary>
    /// インデックスに基づいて敵の位置を即座に設定（スポーン時用）
    /// </summary>
    private void SetEnemyPositionImmediate(int enemyIndex)
    {
        if (enemyIndex < 0 || enemyIndex >= _currentEnemies.Count) return;
        
        var enemy = _currentEnemies[enemyIndex];
        if (!enemy) return;
        
        // インデックスに応じて距離を計算
        var distanceFromPlayer = enemyIndex * alignment;
        // EnemyDataのenemyYOffsetを適用
        var targetPosition = this.transform.position + new Vector3(distanceFromPlayer, enemy.Data.enemyYOffset, 0);

        // 即座に位置を設定
        enemy.transform.position = targetPosition;
        enemy.UpdateHpBarPosition();
    }

    /// <summary>
    /// インデックスに基づいて敵の実際の座標を計算・更新（アニメーション付き）
    /// </summary>
    private void UpdateEnemyPosition(int enemyIndex)
    {
        if (enemyIndex < 0 || enemyIndex >= _currentEnemies.Count) return;
        
        var enemy = _currentEnemies[enemyIndex];
        if (!enemy) return;
        
        // インデックスに応じて距離を計算
        var distanceFromPlayer = enemyIndex * alignment;
        // EnemyDataのenemyYOffsetを適用
        var targetPosition = this.transform.position + new Vector3(distanceFromPlayer, enemy.Data.enemyYOffset, 0);

        // DOTweenで滑らかな移動アニメーション
        enemy.transform.DOMove(targetPosition, 0.5f)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // アニメーション中もHPバーの位置を更新
                enemy.UpdateHpBarPosition();
            })
            .SetLink(enemy.gameObject);
    }
}
