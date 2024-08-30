using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MergeManager : MonoBehaviour
{
    [System.Serializable]
    public struct BallData
    {
        public GameObject prefab;
        public float probability;
    }

    public static MergeManager instance;

    [SerializeField]
    private MergeWall wall;
    [SerializeField]
    private GameObject fallAnchor;
    [SerializeField]
    private List<BallData> balls;
    [SerializeField]
    private List<float> ballAttacks;

    public float moveSpeed = 1.0f;
    public float coolTime = 1.0f;
    private float limit = -2.5f;

    private GameObject currentBall;
    private GameObject nextBall;
    private float probabilitySum;
    private GameObject ballContainer;
    private float lastFallTime = 0;

    private List<float> moveSpeeds = new List<float> { 0.5f, 1.0f, 1.5f, 2.0f, 2.5f };
    private int moveSpeedLevel = 0;
    private List<float> wallWidths = new List<float> { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f, 5.5f, 6.0f, 6.5f };
    private int wallWidthLevel = 0;
    private List<float> coolTimes = new List<float> { 3.0f, 2.75f, 2.5f, 2.25f, 2.0f, 1.75f, 1.5f, 1.25f, 1.0f, 0.75f, 0.5f, 0.25f, 0.1f };
    private int coolTimeLevel = 0;
    private List<float> attacks = new List<float> { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };
    private int attackLevel = 0;

    public void LevelUpMoveSpeed()
    {
        if (moveSpeedLevel < moveSpeeds.Count - 1)
        {
            moveSpeed = moveSpeeds[++moveSpeedLevel];
        }
        GameManager.instance.uiManager.EnableLevelUpOptions(false);
        Time.timeScale = 1.0f;
    }

    public void LevelUpWallWidth()
    {
        if (wallWidthLevel < wallWidths.Count - 1)
        {
            wall.SetWallWidth(wallWidths[++wallWidthLevel]);
        }
        GameManager.instance.uiManager.EnableLevelUpOptions(false);
        Time.timeScale = 1.0f;
    }

    public void LevelUpCoolTime()
    {
        if (coolTimeLevel < coolTimes.Count - 1)
        {
            coolTime = coolTimes[++coolTimeLevel];
        }
        GameManager.instance.uiManager.EnableLevelUpOptions(false);
        Time.timeScale = 1.0f;
    }

    public void LevelUpAttack()
    {
        if (attackLevel < attacks.Count - 1)
        {
            ballAttacks[attackLevel] = attacks[++attackLevel];
        }
        GameManager.instance.uiManager.EnableLevelUpOptions(false);
        Time.timeScale = 1.0f;
    }

    public void SpawnBall(int level, Vector3 p = default, Quaternion q = default)
    {
        if (level < 1 || level > balls.Count) return;

        GameObject b = balls[level - 1].prefab;
        Instantiate(b, p, q, ballContainer.transform);
        int atk = (int)(ballAttacks[level - 1] * level * attacks[attackLevel]);
        Attack(atk);
    }

    private void Attack(int atk)
    {
        foreach (var e in GameManager.instance.enemyContainer.GetAllEnemies())
        {
            e.TakeDamage(atk);
        }
        int i = Random.Range(0, 5);
        SeManager.instance.PlaySe("ball" + i);
        Camera.main.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.3f);
    }

    private void FallAndDecideNextBall()
    {
        Instantiate(currentBall, fallAnchor.transform.position + Vector3.down, Quaternion.identity, ballContainer.transform);
        fallAnchor.GetComponent<SpriteRenderer>().color = nextBall.GetComponent<BallBase>().color;
        fallAnchor.transform.localScale = Vector3.one * nextBall.GetComponent<BallBase>().size;

        float r = GameManager.instance.RandomRange(0.0f, probabilitySum);
        foreach (var ball in balls)
        {
            r -= ball.probability;
            if (r <= 0)
            {
                currentBall = nextBall;
                nextBall = ball.prefab;
                break;
            }
        }
    }

    private void DecideBall()
    {
        float r = GameManager.instance.RandomRange(0.0f, probabilitySum);
        foreach (var ball in balls)
        {
            r -= ball.probability;
            if (r <= 0)
            {
                currentBall = ball.prefab;
                break;
            }
        }

        r = GameManager.instance.RandomRange(0.0f, probabilitySum);
        foreach (var ball in balls)
        {
            r -= ball.probability;
            if (r <= 0)
            {
                nextBall = ball.prefab;
                break;
            }
        }

        fallAnchor.GetComponent<SpriteRenderer>().color = currentBall.GetComponent<BallBase>().color;
        fallAnchor.transform.localScale = Vector3.one * currentBall.GetComponent<BallBase>().size;
    }

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        probabilitySum = 0;
        for (int i = 0; i < balls.Count; i++)
        {
            var ball = balls[i];
            probabilitySum += ball.probability;
            ballAttacks.Add(1.0f);
        }

        ballContainer = new GameObject("BallContainer");
        fallAnchor.transform.position = new Vector3(0, 1.5f, 0);

        moveSpeed = moveSpeeds[0];
        wall.SetWallWidth(wallWidths[0]);
        coolTime = coolTimes[0];
    }

    void Start()
    {
        DecideBall();
        // if (Application.isEditor) coolTime = 0.1f;
    }

    void Update()
    {
        limit = wall.wallWidth / 2;
        float size = fallAnchor.transform.localScale.x + 0.5f;
        float r = 1 - Mathf.Min(1, (Time.time - lastFallTime) / coolTime);
        fallAnchor.GetComponent<SpriteRenderer>().material.SetFloat("_Ratio", r);


        if (GameManager.instance.state == GameManager.GameState.Battle || GameManager.instance.state == GameManager.GameState.BattlePreparation)
        {
            if (Input.GetKey(KeyCode.A) && fallAnchor.transform.position.x - size / 2 > -limit)
            {
                fallAnchor.transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D) && fallAnchor.transform.position.x + size / 2 < limit)
            {
                fallAnchor.transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastFallTime > coolTime)
            {
                lastFallTime = Time.time;
                FallAndDecideNextBall();
            }
        }
    }
}
