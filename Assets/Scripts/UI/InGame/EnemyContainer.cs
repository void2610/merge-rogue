using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private List<EnemyData> enemies;
    [SerializeField] private List<EnemyData> bosses = new();
    [SerializeField] private float alignment = 4;
    public ReactiveProperty<int> defeatedEnemyCount = new(0);
    private readonly List<GameObject> currentEnemies = new();
    private const int ENEMY_NUM = 4;
    private int gainedExp;
    private readonly List<Vector3> positions = new();

    public int GetCurrentEnemyCount()
    {
        return currentEnemies.Count;
    }

    public List<EnemyBase> GetAllEnemies()
    {
        var enemyBases = new List<EnemyBase>();
        foreach (var enemy in currentEnemies)
        {
            enemyBases.Add(enemy.transform.GetChild(0).GetComponent<EnemyBase>());
        }
        return enemyBases;
    }

    public void SpawnBoss()
    {
        float total = 0;
        foreach (var enemyData in bosses)
        {
            total += enemyData.probability;
        }
        var randomPoint = GameManager.Instance.RandomRange(0.0f, total);

        foreach (var enemyData in bosses)
        {
            if (randomPoint < enemyData.probability)
            {
                var e = Instantiate(enemyData.prefab, this.transform);
                currentEnemies.Add(e);
                e.transform.position = positions[1];
                break;
            }
            randomPoint -= enemyData.probability;
        }
    }

    public void SpawnEnemy(int count = 1, int stage = 0)
    {
        for (int i = 0; i < count; i++)
        {
            if (currentEnemies.Count >= ENEMY_NUM) return;
            var total = enemies.Sum(enemyData => enemyData.probability);
            var randomPoint = GameManager.Instance.RandomRange(0.0f, total);

            foreach (var enemyData in enemies)
            {
                if (randomPoint < enemyData.probability)
                {
                    var e = Instantiate(enemyData.prefab, this.transform);
                    // 敵の強さパラメータを設定
                    var m = ((stage + 1) * 0.6f);
                    e.transform.GetComponentsInChildren<EnemyBase>()[0].Init(m);
                    currentEnemies.Add(e);
                    e.transform.position = positions[currentEnemies.Count - 1];
                    break;
                }
                randomPoint -= enemyData.probability;
            }
        }
    }

    public void SpawnEnemyByBoss(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            if (currentEnemies.Count >= ENEMY_NUM) return;
            var total = enemies.Sum(enemyData => enemyData.probability);
            var randomPoint = GameManager.Instance.RandomRange(0.0f, total);

            foreach (var enemyData in enemies)
            {
                if (randomPoint < enemyData.probability)
                {
                    var e = Instantiate(enemyData.prefab, this.transform);
                    currentEnemies.Add(e);
                    if (currentEnemies.Count == 2)
                        e.transform.position = positions[0];
                    else if (currentEnemies.Count == 3)
                        e.transform.position = positions[2];
                    break;
                }
                randomPoint -= enemyData.probability;
            }
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        defeatedEnemyCount.Value++;
        gainedExp += enemy.GetComponent<EnemyBase>().exp;
        var g = enemy.transform.parent.gameObject;
        currentEnemies.Remove(g);
        enemy.GetComponent<EnemyBase>().OnDisappear();
        
        // 全ての敵を倒したらステージ進行
        if (currentEnemies.Count == 0)
        {
            MergeManager.Instance.EndMerge();
            Utils.Instance.WaitAndInvoke(2.0f, () =>
            {
                GameManager.Instance.Player.AddExp(gainedExp);
                gainedExp = 0;
            });
        }
    }

    public void AttackEnemy(int singleDamage, int allDamage)
    {
        StartCoroutine(AttackEnemyCoroutine(singleDamage, allDamage));
    }

    public void AttackEnemyBySkill(int damage, List<bool> isAttacks)
    {
        // listで指定したindexの敵に攻撃
        StartCoroutine(AttackEnemyBySkillCoroutine(damage, isAttacks));
    }
    
    private IEnumerator AttackEnemyBySkillCoroutine(int damage, List<bool> isAttacks)
    {
        if(isAttacks.Count != currentEnemies.Count) Debug.LogError("The number of bool list does not match the number of enemies");
        
        var es = GetAllEnemies();
        for (var i = 0; i < isAttacks.Count; i++)
        {
            if (!isAttacks[i]) continue;
            
            es[i].TakeDamage(damage, true);
            SeManager.Instance.PlaySe("playerAttack");
            CameraMove.Instance.ShakeCamera(0.5f, damage * 0.01f);
        }
        yield return new WaitForSeconds(0.5f);
        
    }
    
    private IEnumerator AttackEnemyCoroutine(int singleDamage, int allDamage)
    {
        var es = GetAllEnemies();
        
        // 全体攻撃
        if (allDamage > 0)
        {
            ParticleManager.Instance.AllHitParticle(new Vector3(-3.7f, 3.3f, 0));
            SeManager.Instance.PlaySe("playerAttack");
            CameraMove.Instance.ShakeCamera(0.5f, allDamage * 0.03f);
            foreach (var e in es)
            {
                e.TakeDamage(allDamage, false);
            }
            yield return new WaitForSeconds(0.5f);
        }

        if (GameManager.Instance.EnemyContainer.GetCurrentEnemyCount() == 0) yield break;

        // 単体攻撃
        // 一番前の敵を攻撃、攻撃力が残っていたら次の敵を攻撃
        foreach (var e in es)
        {
            if (singleDamage <= 0) break;
            var actualDamage = singleDamage > e.Health ? e.Health : singleDamage;
            if (es.IndexOf(e) == es.Count - 1) actualDamage = singleDamage;
            e.TakeDamage(actualDamage, true);
            singleDamage -= actualDamage;
            
            SeManager.Instance.PlaySe("playerAttack");
            // TODO: もう少しなだらかな上昇幅がいいかも
            CameraMove.Instance.ShakeCamera(0.5f, actualDamage * 0.01f);
            yield return new WaitForSeconds(0.3f);
        }
        
        // 敵が残っていたら敵の攻撃へ
        if (GameManager.Instance.EnemyContainer.GetCurrentEnemyCount() > 0)
        {
            Utils.Instance.WaitAndInvoke(0.75f,
                () => GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack));
        }
    }
    
    public void Action()
    {
        StartCoroutine(AttackPlayerCoroutine());
    }

    private IEnumerator AttackPlayerCoroutine()
    {
        foreach (var enemy in currentEnemies)
        {
            var enemyBase = enemy.transform.GetChild(0).GetComponent<EnemyBase>();
            enemyBase.Action();
            // 0.5秒待つ
            yield return new WaitForSeconds(0.5f);
        }

        if(currentEnemies.Count > 0)
            GameManager.Instance.ChangeState(GameManager.GameState.Merge);
    }

    public void Awake()
    {
        positions.Add(this.transform.position + new Vector3(-alignment * 2, 0, 0));
        positions.Add(this.transform.position + new Vector3(-alignment, 0, 0));
        positions.Add(this.transform.position);
        positions.Add(this.transform.position + new Vector3(alignment, 0, 0));
    }
}
