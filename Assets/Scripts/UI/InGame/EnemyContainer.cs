using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
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

    [SerializeField] private float alignment = 4;
    [SerializeField] private Treasure treasure;
    public readonly ReactiveProperty<int> DefeatedEnemyCount = new(0);
    private readonly List<EnemyBase> _currentEnemies = new();
    private readonly List<Vector3> _positions = new();
    private const int ENEMY_NUM = 4;
    private int _gainedExp;

    public int GetCurrentEnemyCount()
    {
        return _currentEnemies.Count;
    }

    public List<EnemyBase> GetAllEnemies()
    {
        return _currentEnemies;
    }

    public void SpawnBoss(int stage)
    {
        var boss = ContentProvider.Instance.GetRandomBoss();
        var bossBase = boss.transform.GetChild(0).GetComponent<EnemyBase>();
        boss.transform.parent = this.transform;
        boss.transform.localScale = new Vector3(1, 1, 1);
        boss.transform.position = _positions[_currentEnemies.Count];
        // 敵の強さパラメータを設定
        var m = ((stage + 1) * 0.6f);
        bossBase.Init(m);
        _currentEnemies.Add(bossBase);
    }

    public void SpawnEnemy(int count, int stage)
    {
        count = count > ENEMY_NUM ? ENEMY_NUM : count;
        if (count <= 0) return;
        for(var i = 0; i < count; i++)
        {
            var e = ContentProvider.Instance.GetRandomEnemy();
            e.transform.parent = this.transform;
            e.transform.position = _positions[_currentEnemies.Count];
            e.transform.localScale = new Vector3(1, 1, 1);
            // 敵の強さパラメータを設定
            var m = ((stage + 1) * 0.6f);
            var enemyBase = e.transform.GetComponentsInChildren<EnemyBase>()[0];
            enemyBase.Init(m);
            _currentEnemies.Add(enemyBase);
        }
    }
    
    public void DamageAllEnemies(int damage)
    {
        for(var i = 0; i < _currentEnemies.Count; i++)
        {
            _currentEnemies[i].Damage(damage);
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        var enemyBase = enemy.GetComponent<EnemyBase>();
        DefeatedEnemyCount.Value++;
        _gainedExp += enemyBase.exp;
        _currentEnemies.Remove(enemyBase);
        enemyBase.OnDisappear();

        // ボスを倒したら全回復してレリック獲得
        if (enemyBase.enemyType == EnemyBase.EnemyType.Boss){
            GameManager.Instance.Player.HealToFull(); 
            treasure.OpenTreasure(Treasure.TreasureType.Boss);
        }
        else
        {
            // 全ての敵を倒したらステージ進行
            if (_currentEnemies.Count == 0) EndBattle().Forget();
        }
    }
    
    public async UniTaskVoid EndBattle()
    {
        MergeManager.Instance.EndMerge().Forget();
        await UniTask.Delay(2000);
        GameManager.Instance.Player.OnBattleEnd();
        GameManager.Instance.Player.AddExp(_gainedExp);
        _gainedExp = 0;
    }

    public void AttackEnemy(int singleDamage, int allDamage)
    {
        AttackEnemyAsync(singleDamage, allDamage).Forget();
    }

    public void AttackEnemyBySkill(int damage, List<bool> isAttacks)
    {
        // listで指定したindexの敵に攻撃
        StartCoroutine(AttackEnemyBySkillCoroutine(damage, isAttacks));
    }
    
    private IEnumerator AttackEnemyBySkillCoroutine(int damage, List<bool> isAttacks)
    {
        if(isAttacks.Count != _currentEnemies.Count) Debug.LogError("The number of bool list does not match the number of enemies");
        
        var es = GetAllEnemies();
        for (var i = 0; i < isAttacks.Count; i++)
        {
            if (!isAttacks[i]) continue;

            es[i].Damage(damage);
            CameraMove.Instance.ShakeCamera(0.5f, damage * 0.01f);
        }
        yield return new WaitForSeconds(0.5f);
        
    }
    
    private async UniTaskVoid AttackEnemyAsync(int singleDamage, int allDamage)
    {
        var es = GetAllEnemies().ToList();
        
        // 全体攻撃
        if (allDamage > 0)
        {
            ParticleManager.Instance.AllHitParticle(new Vector3(-3.7f, 3.3f, 0));
            CameraMove.Instance.ShakeCamera(0.5f, allDamage * 0.03f);
            SeManager.Instance.PlaySe("playerAttack");
            for(var i = 0; i < es.Count; i++)
            {
                es[i].Damage(allDamage);
            }
            await UniTask.Delay(500);
        }

        if (GameManager.Instance.EnemyContainer.GetCurrentEnemyCount() == 0) return;

        // 単体攻撃
        // 一番前の敵を攻撃、攻撃力が残っていたら次の敵を攻撃
        for(var i = 0; i < es.Count; i++)
        {
            var e = es[i];
            if(!e) continue;
            if (singleDamage <= 0) break;
            var actualDamage = singleDamage > e.Health ? e.Health : singleDamage;
            if (es.IndexOf(e) == es.Count - 1) actualDamage = singleDamage;
            
            SeManager.Instance.PlaySe("playerAttack");
            ParticleManager.Instance.DamageText(actualDamage, e.transform.position.x);
            e.Damage(actualDamage);
            singleDamage -= actualDamage;
            
            CameraMove.Instance.ShakeCamera(0.5f, actualDamage * 0.01f);
            await UniTask.Delay(300);
        }
        
        // 敵が残っていたら敵の攻撃へ
        if (GameManager.Instance.EnemyContainer.GetCurrentEnemyCount() > 0)
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
            if (!_currentEnemies[i].gameObject) continue;
            _currentEnemies[i].Action();
            // 0.5秒待つ
            await UniTask.Delay(500);
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
