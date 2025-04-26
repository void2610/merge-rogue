using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using Unity.VisualScripting;
using UnityEngine;

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
    [SerializeField] private GameObject bossBasePrefab;
    [SerializeField] private float alignment = 4;
    [SerializeField] private Treasure treasure;
    public readonly ReactiveProperty<int> DefeatedEnemyCount = new(0);
    private readonly List<EnemyBase> _currentEnemies = new();
    private readonly List<Vector3> _positions = new();
    private const int ENEMY_NUM = 4;
    private int _gainedExp;

    public int GetCurrentEnemyCount() => _currentEnemies.Count;
    public List<EnemyBase> GetAllEnemies() => _currentEnemies;
    public EnemyBase GetRandomEnemy() => _currentEnemies[GameManager.Instance.RandomRange(0, _currentEnemies.Count)];
    public int GetEnemyIndex(EnemyBase enemy) => _currentEnemies.IndexOf(enemy);

    public void SpawnBoss(int stage)
    {
        var bossData = ContentProvider.Instance.GetRandomBoss();
        var e = Instantiate(bossBasePrefab, this.transform).GetComponent<EnemyBase>();
        e.transform.localScale = new Vector3(1, 1, 1);
        e.transform.position = _positions[_currentEnemies.Count];
        e.Init(bossData, stage);
        _currentEnemies.Add(e);
    }

    public void SpawnEnemy(int count, int stage)
    {
        count = count > ENEMY_NUM ? ENEMY_NUM : count;
        if (count <= 0) return;
        for(var i = 0; i < count; i++)
        {
            var enemyData = ContentProvider.Instance.GetRandomEnemy();
            var e = Instantiate(enemyBasePrefab, this.transform).transform.GetChild(0).GetComponent<EnemyBase>();
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
            _currentEnemies[i].Damage(damage);
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
        _gainedExp += enemyBase.exp;
        _currentEnemies.Remove(enemyBase);
        enemyBase.OnDisappear();

        // 全ての敵を倒したらステージ進行
        if (_currentEnemies.Count == 0)
        {
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
            ContentProvider.Instance.AddAct();
            Debug.Log(ContentProvider.Instance.Act);
        }
    }
    
    public async UniTask AttackEnemy(Dictionary<AttackType, int> damages)
    {
        var es = GetAllEnemies().ToList();
        
        // 全体攻撃
        if (damages[AttackType.All] > 0)
        {
            ParticleManager.Instance.AllHitParticle(new Vector3(-3.7f, 3.3f, 0));
            CameraMove.Instance.ShakeCamera(0.5f, damages[AttackType.All] * 0.03f);
            SeManager.Instance.PlaySe("playerAttack");
            for(var i = 0; i < es.Count; i++)
            {
                es[i].Damage(damages[AttackType.All], AttackType.All);
            }
            await UniTask.Delay(250);
        }

        if (GetCurrentEnemyCount() == 0) return;
        es = GetAllEnemies().ToList();
        
        // 一番後ろの敵を攻撃
        if (damages[AttackType.Last] > 0)
        {
            var lastEnemy = es.Last();
            SeManager.Instance.PlaySe("playerAttack");
            lastEnemy.Damage(damages[AttackType.Last], AttackType.Last);
            CameraMove.Instance.ShakeCamera(0.5f, damages[AttackType.Last] * 0.01f);
            await UniTask.Delay(250);
        }
        
        if (GetCurrentEnemyCount() == 0) return;
        es = GetAllEnemies().ToList();

        // 通常攻撃
        // 一番前の敵を攻撃、攻撃力が残っていたら次の敵を攻撃
        if (damages[AttackType.Normal] > 0)
        {
            var singleDamage = damages[AttackType.Normal];
            for (var i = 0; i < es.Count; i++)
            {
                var e = es[i];
                if (!e) continue;
                if (singleDamage <= 0) break;
                var actualDamage = singleDamage > e.Health ? e.Health : singleDamage;
                if (es.IndexOf(e) == es.Count - 1) actualDamage = singleDamage;

                SeManager.Instance.PlaySe("playerAttack");
                e.Damage(actualDamage);
                singleDamage -= actualDamage;

                CameraMove.Instance.ShakeCamera(0.5f, actualDamage * 0.01f);
                await UniTask.Delay(200);
            }
        }
        await UniTask.Delay(250);
        
        if (GetCurrentEnemyCount() == 0) return;
        es = GetAllEnemies().ToList();
        
        // ランダム攻撃
        if (damages[AttackType.Random] > 0)
        {
            var randomEnemy = GetRandomEnemy();
            SeManager.Instance.PlaySe("playerAttack");
            randomEnemy.Damage(damages[AttackType.Random], AttackType.Random);
            CameraMove.Instance.ShakeCamera(0.5f, damages[AttackType.Random] * 0.01f);
            await UniTask.Delay(250);
        }
        
        // 敵が残っていたら敵の攻撃へ
        if (GetCurrentEnemyCount() > 0)
        {
            Observable.Timer(TimeSpan.FromSeconds(0.75f), destroyCancellationToken)
                .Subscribe(_ => {GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack);});
        }
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
