using UnityEngine;
using System.Collections.Generic;

public class MergeManager : MonoBehaviour
{
    [System.Serializable]
    public struct BallData
    {
        public GameObject prefab;
        public float probability;
    }

    public static MergeManager Instance;
    private static readonly int ratio = Shader.PropertyToID("_Ratio");

    [SerializeField] private MergeWall wall;
    [SerializeField] private GameObject mergeParticle;
    [SerializeField] private GameObject fallAnchor;

    public float moveSpeed = 1.0f;
    public float coolTime = 1.0f;
    public float attackMagnification = 1.0f;
    private float limit = -2.5f;


    public GameObject currentBall;
    public GameObject nextBall;
    private GameObject ballContainer;
    private float lastFallTime;

    private readonly List<float> moveSpeeds = new() { 0.5f, 1.0f, 1.5f, 2.0f, 2.5f };
    private int moveSpeedLevel;
    private readonly List<float> wallWidths = new() { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f, 5.5f, 6.0f, 6.5f };
    private int wallWidthLevel;
    private readonly List<float> coolTimes = new() { 3.0f, 2.5f, 2.0f, 1.5f, 1.25f, 1.0f, 0.75f, 0.5f, 0.25f, 0.1f };
    private int coolTimeLevel;
    private readonly List<float> attacks = new() { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };
    private int attackLevel;
    private Vector3 currentBallPosition = new(0, 1.0f, 0);
    private readonly Vector3 nextBallPosition = new(-2, 1, 0);

    public void LevelUpMoveSpeed()
    {
        if (moveSpeedLevel < moveSpeeds.Count - 1)
        {
            moveSpeed = moveSpeeds[++moveSpeedLevel];
        }
        EndLevelUp();
    }

    public void LevelUpWallWidth()
    {
        if (wallWidthLevel < wallWidths.Count - 1)
        {
            wall.SetWallWidth(wallWidths[++wallWidthLevel]);
        }
        EndLevelUp();
    }

    public void LevelUpCoolTime()
    {
        if (coolTimeLevel < coolTimes.Count - 1)
        {
            coolTime = coolTimes[++coolTimeLevel];
        }
        EndLevelUp();
    }

    public void LevelUpAttack()
    {
        if (attackLevel < attacks.Count - 1)
        {
            attackMagnification = attacks[++attackLevel];
        }
        EndLevelUp();
    }
    
    private void EndLevelUp()
    {
        GameManager.Instance.uiManager.remainingLevelUps--;
        if (GameManager.Instance.uiManager.remainingLevelUps > 0) return;
        
        GameManager.Instance.uiManager.EnableCanvasGroup("LevelUp", false);
        GameManager.Instance.ChangeState(GameManager.GameState.StageMoving);
    }

    public void SpawnBall(int level, Vector3 p, Quaternion q)
    {
        var ball = InventoryManager.instance.GetBallByLevel(level);
        if (ball == null) return;

        ball.transform.position = p;
        ball.transform.rotation = q;
        ball.transform.SetParent(ballContainer.transform);
        int i = Random.Range(0, 5);
        SeManager.Instance.PlaySe("ball" + i);
        Instantiate(mergeParticle, p, Quaternion.identity);
    }

    public void Attack(float atk)
    {
        foreach (var e in GameManager.Instance.enemyContainer.GetAllEnemies())
        {
            var a = (int)(atk * attackMagnification);
            e.TakeDamage(a); 
        }

        Camera.main?.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.3f);
    }

    private void FallAndDecideNextBall()
    {
        currentBall.GetComponent<BallBase>().Unfreeze();
        currentBall.GetComponent<CircleCollider2D>().enabled = true;
        currentBall.transform.SetParent(ballContainer.transform);
        currentBall = nextBall;
        currentBall.transform.position = currentBallPosition;
        nextBall = InventoryManager.instance.GetRandomBall();
        nextBall.transform.position = nextBallPosition;
        nextBall.GetComponent<CircleCollider2D>().enabled = false;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ballContainer = new GameObject("BallContainer");

        moveSpeed = moveSpeeds[0];
        wall.SetWallWidth(wallWidths[0]);
        coolTime = coolTimes[0];
    }

    void Start()
    {
        if (Application.isEditor) coolTime = 0.1f;

        nextBall = InventoryManager.instance.GetRandomBall();
        nextBall.transform.position = nextBallPosition;
        currentBall = InventoryManager.instance.GetRandomBall();
        currentBall.transform.position = currentBallPosition;
    }

    void Update()
    {
        limit = wall.WallWidth / 2 + 0.05f;
        float size = currentBall.transform.localScale.x + 0.5f;
        float r = Mathf.Min(1, (Time.time - lastFallTime) / coolTime);
        fallAnchor.GetComponent<SpriteRenderer>().material.SetFloat(ratio, r);
        fallAnchor.transform.localScale = currentBall.transform.localScale * 1.01f;

        if (Input.GetKey(KeyCode.A) && currentBallPosition.x - size / 2 > -limit)
        {
            currentBallPosition += Vector3.left * (moveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D) && currentBallPosition.x + size / 2 < limit)
        {
            currentBallPosition += Vector3.right * (moveSpeed * Time.deltaTime);
        }

        if (GameManager.Instance.state == GameManager.GameState.Battle ||
            GameManager.Instance.state == GameManager.GameState.BattlePreparation)
        {
            if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastFallTime > coolTime)
            {
                SeManager.Instance.PlaySe("fall");
                lastFallTime = Time.time;
                FallAndDecideNextBall();
            }
        }

        currentBall.transform.position = currentBallPosition;
        fallAnchor.transform.position = currentBallPosition;
    }
}