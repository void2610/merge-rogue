using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyContainer : MonoBehaviour
{
    [System.Serializable]
    internal class EnemyData
    {
        public GameObject prefab;
        public float probability;
    }

    [SerializeField]
    private List<EnemyData> enemies;
    [SerializeField]
    private List<EnemyData> bosses = new List<EnemyData>();
    [SerializeField]
    private float alignment = 4;
    private readonly List<GameObject> currentEnemies = new List<GameObject>();
    private const int ENEMY_NUM = 4;
    private int gainedExp;
    private readonly List<Vector3> positions = new List<Vector3>();

    public int GetEnemyCount()
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

    public void SpawnEnemy(int count = 1)
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
        gainedExp += enemy.GetComponent<EnemyBase>().exp;
        GameManager.Instance.coin.Value += enemy.GetComponent<EnemyBase>().coin;
        var g = enemy.transform.parent.gameObject;
        currentEnemies.Remove(g);
        enemy.GetComponent<EnemyBase>().OnDisappear();
        if (currentEnemies.Count == 0)
        {
            Utils.Instance.WaitAndInvoke(1.0f, () =>
            {
                GameManager.Instance.player.AddExp(gainedExp);
                GameManager.Instance.ChangeState(GameManager.GameState.StageMoving);
                gainedExp = 0;
            });
        }
    }

    public void Awake()
    {
        positions.Add(this.transform.position + new Vector3(-alignment * 2, 0, 0));
        positions.Add(this.transform.position + new Vector3(-alignment, 0, 0));
        positions.Add(this.transform.position);
        positions.Add(this.transform.position + new Vector3(alignment, 0, 0));
    }

    public void Update()
    {
        // エディタだけ
        if (!Application.isEditor) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnEnemy();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            //PlayerPrefsをリセット
            PlayerPrefs.DeleteAll();
        }
    }
}
