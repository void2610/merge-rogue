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
    private List<float> wallWidths = new List<float> { 4.5f, 5.0f, 5.5f, 6.0f, 6.5f };
    private int wallWidthLevel = 0;
    private List<float> coolTimes = new List<float> { 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };
    private int coolTimeLevel = 0;
    private List<float> attacks = new List<float> { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };
    private int attackLevel = 0;

    public void LevelUpMoveSpeed()
    {
        if (moveSpeedLevel < moveSpeeds.Count - 1)
        {
            moveSpeed = moveSpeeds[++moveSpeedLevel];
        }

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
        SeManager.instance.PlaySe("enemyAttack");
        Camera.main.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.3f);
    }

    private void FallAndDecideNextBall()
    {
        Instantiate(currentBall, fallAnchor.transform.position + Vector3.down, Quaternion.identity, ballContainer.transform);
        fallAnchor.GetComponent<SpriteRenderer>().color = nextBall.GetComponent<Ball>().color;
        fallAnchor.transform.localScale = Vector3.one * nextBall.GetComponent<Ball>().size;

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

        fallAnchor.GetComponent<SpriteRenderer>().color = currentBall.GetComponent<Ball>().color;
        fallAnchor.transform.localScale = Vector3.one * currentBall.GetComponent<Ball>().size;
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
        limit = wall.wallWidth / 2;

        moveSpeed = moveSpeeds[0];
        wall.SetWallWidth(wallWidths[0]);
        coolTime = coolTimes[0];
    }

    void Start()
    {
        DecideBall();
    }

    void Update()
    {
        float size = fallAnchor.transform.localScale.x + 0.5f;
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
