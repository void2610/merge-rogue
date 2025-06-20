using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EnemyContainer : MonoBehaviour
{
    [System.Serializable]
    internal class EnemyData
    {
        public GameObject prefab;
        public float probability;
    }
    
    public static EnemyContainer Instance { get; private set; }

    [SerializeField] private GameObject enemyBasePrefab;
    [SerializeField] private GameObject enemyHpSliderPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float alignment = 4;
    [SerializeField] private Treasure treasure;
    public readonly ReactiveProperty<int> DefeatedEnemyCount = new(0);
    private readonly List<EnemyBase> _currentEnemies = new();
    private readonly List<Vector3> _positions = new();
    private const int ENEMY_NUM = 4;
    private int _gainedExp;
    
    private IContentService _contentService;
    private IRandomService _randomService;
    private IObjectResolver _resolver;
    
    [Inject]
    public void InjectDependencies(IContentService contentService, IRandomService randomService, IObjectResolver resolver)
    {
        _contentService = contentService;
        _randomService = randomService;
        _resolver = resolver;
    }

    public int GetCurrentEnemyCount() => _currentEnemies.Count;
    public List<EnemyBase> GetAllEnemies() => _currentEnemies;
    public EnemyBase GetRandomEnemy() => _currentEnemies[_randomService.RandomRange(0, _currentEnemies.Count)];
    public int GetEnemyIndex(EnemyBase enemy) => _currentEnemies.IndexOf(enemy);
    public GameObject GetHpSliderPrefab() => enemyHpSliderPrefab;
    public GameObject GetCoinPrefab() => coinPrefab;

    public void SpawnBoss(int stage)
    {
        var bossData = _contentService.GetRandomBoss();
        
        // ボスの場合はEnemyBaseのサブクラスを取得して使用する
        var e = Instantiate(enemyBasePrefab, this.transform);
        Destroy(e.GetComponent<EnemyBase>());
        var type = System.Type.GetType(bossData.className);
        Debug.Log(bossData.className);
        var behaviour = e.AddComponent(type) as EnemyBase;
        
        // VContainerで依存性を注入
        _resolver.Inject(behaviour);
        
        e.transform.localScale = new Vector3(1, 1, 1);
        e.transform.position = _positions[_currentEnemies.Count];
        behaviour.Init(bossData, stage);
        _currentEnemies.Add(behaviour);
    }

    public void SpawnEnemy(int count, int stage)
    {
        count = count > ENEMY_NUM ? ENEMY_NUM : count;
        if (count <= 0) return;
        for(var i = 0; i < count; i++)
        {
            var enemyData = _contentService.GetRandomEnemy();
            var e = Instantiate(enemyBasePrefab, this.transform).GetComponent<EnemyBase>();
            
            // VContainerで依存性を注入
            _resolver.Inject(e);
            
            e.transform.position = _positions[_currentEnemies.Count];
            e.transform.localScale = new Vector3(1, 1, 1);
            e.Init(enemyData, stage);
            _currentEnemies.Add(e);
        }
    }
    
    public void DamageAllEnemies(int damage)
    {
        for(var i = 0; i < _currentEnemies.Count; i++)
        {
            _currentEnemies[i].Damage(AttackType.All, damage);
        }
    }
    
    public void HealAllEnemies(int heal)
    {
        for(var i = 0; i < _currentEnemies.Count; i++)
        {
            _currentEnemies[i].Heal(heal);
        }
    }
    
    public void HealEnemy(int index, int heal)
    {
        if (index < 0 || index >= _currentEnemies.Count) return;
        _currentEnemies[index].Heal(heal);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        var enemyBase = enemy.GetComponent<EnemyBase>();
        DefeatedEnemyCount.Value++;
        _gainedExp += enemyBase.Exp;
        _currentEnemies.Remove(enemyBase);
        enemyBase.OnDisappear();

        // 全ての敵を倒したらステージ進行
        if (_currentEnemies.Count == 0)
        {
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
                for(var i = 0; i < es.Count; i++) es[i].Damage(type, atk);
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
        if(_currentEnemies.Count == 0) return;
        // 行動
        for(var i = 0; i < _currentEnemies.Count; i ++)
        {
            if (!_currentEnemies[i]?.gameObject) continue;
            _currentEnemies[i].Action();
            // 0.5秒待つ
            await UniTask.Delay(250);
        }
        
        await UniTask.Delay(100);
        
        // 状態異常を更新
        await GameManager.Instance.Player.UpdateStatusEffects();
        var tasks = _currentEnemies.ToList().Select(e => e.UpdateStatusEffects());
        await UniTask.WhenAll(tasks);

        if(_currentEnemies.Count > 0)
            GameManager.Instance.ChangeState(GameManager.GameState.Merge);
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
        
        _positions.Add(this.transform.position + new Vector3(-alignment * 2, 0, 0));
        _positions.Add(this.transform.position + new Vector3(-alignment, 0, 0));
        _positions.Add(this.transform.position);
        _positions.Add(this.transform.position + new Vector3(alignment, 0, 0));
    }
}
