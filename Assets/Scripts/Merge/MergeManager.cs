using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;

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
    private static readonly int alpha = Shader.PropertyToID("_Alpha");

    [SerializeField] private MergeWall wall;
    [SerializeField] public PhysicsMaterial2D wallMaterial;
    [SerializeField] private GameObject fallAnchor;
    [SerializeField] private Material arrowMaterial;
    [SerializeField] private GameObject ballGauge;
    [SerializeField] private TextMeshProUGUI ballCountText;
    [SerializeField] private Vector3 nextBallPosition;
    
    public float attackMagnification = 1.0f;
    public int remainingBalls { get; private set; } = 0;
    public GameObject currentBall;
    public GameObject nextBall;
    
    private GameObject ballContainer;
    private float lastFallTime;
    private float limit = -2.5f;
    private readonly List<float> wallWidths = new() { 2.0f, 2.75f, 3.5f, 4.25f, 5.0f, 5.75f, 6.0f};
    private int wallWidthLevel;
    private readonly List<float> attacks = new() { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 3.75f, 4.0f, 4.25f, 4.5f, 4.75f, 5.0f, 5.25f, 5.5f, 5.75f, 6.0f };
    private int attackLevel;
    private Vector3 currentBallPosition = new(0, 1f, 0);
    private const float MOVE_SPEED = 1.0f;
    private const float COOL_TIME = 0.5f;
    private int ballPerOneTurn = 2;
    private int singleAttackCount;
    private int allAttackCount;
    private Dictionary<Rigidbody2D, float> stopTimers;
    private int timerCount;
    
    public void LevelUpWallWidth()
    {
        if (wallWidthLevel < wallWidths.Count - 1)
        {
            wall.SetWallWidth(wallWidths[++wallWidthLevel]);
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
    
    public void LevelUpBallAmount()
    {
        ballPerOneTurn++;
        EndLevelUp();
    }
    
    private static void EndLevelUp()
    {
        GameManager.Instance.uiManager.remainingLevelUps--;
        if (GameManager.Instance.uiManager.remainingLevelUps > 0) return;
        
        GameManager.Instance.uiManager.EnableCanvasGroup("LevelUp", false);
        GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
    }
    
    public PhysicsMaterial2D GetWallMaterial()
    {
        return wallMaterial;
    }
    
    public void ResetRemainingBalls()
    {
        remainingBalls = ballPerOneTurn;

        if (ballPerOneTurn > 1)
        {
            nextBall = InventoryManager.Instance.GetRandomBall(nextBallPosition);
        }
        currentBall = InventoryManager.Instance.GetRandomBall(fallAnchor.transform.position - Vector3.up * 0.2f);
        currentBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = currentBall.GetComponent<Rigidbody2D>();
        
        ballCountText.text = remainingBalls + "/" + ballPerOneTurn;
        fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = true;
        DOTween.To(() => arrowMaterial.GetFloat(alpha), x => arrowMaterial.SetFloat(alpha, x), 1, 0.5f);
    }
    

    public void SpawnBallFromLevel(int level, Vector3 p, Quaternion q)
    {
        var ball = InventoryManager.Instance.GetBallByLevel(level);
        if (ball == null) return;

        ball.transform.position = p;
        ball.transform.rotation = q;
        ball.transform.SetParent(ballContainer.transform);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }
    
    public void CreateBomb(Vector3 p)
    {
        var bomb = InventoryManager.Instance.GetBombBall();
        bomb.transform.position = p;
        bomb.transform.SetParent(ballContainer.transform);
        bomb.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }

    public void Attack()
    {
        if (singleAttackCount <= 0 && allAttackCount <= 0)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack);
            return;
        }
        
        // イベントでパラメータを更新
        var p = ((int)(singleAttackCount * attackMagnification), (int)(allAttackCount * attackMagnification));
        EventManager.OnPlayerAttack.Trigger(p);
        var atk = EventManager.OnPlayerAttack.GetAndResetValue();
        
        // 攻撃処理
        GameManager.Instance.enemyContainer.AttackEnemy(atk.Item1, atk.Item2);
        // 攻撃アニメーション
        GameManager.Instance.player.gameObject.transform.DOMoveX(0.75f, 0.02f).SetRelative(true).OnComplete(() =>
        {
            GameManager.Instance.player.gameObject.transform.DOMoveX(-0.75f, 0.2f).SetRelative(true)
                .SetEase(Ease.OutExpo);
        });
        // SeManager.Instance.PlaySe("playerAttack");
        singleAttackCount = 0;
        allAttackCount = 0;
    }

    public void AddSingleAttackCount(float atk, Vector3 p)
    {  
        singleAttackCount += Mathf.CeilToInt(atk);
        ParticleManager.Instance.MergeText(singleAttackCount, p);
    }
    
    public void AddAllAttackCount(float atk, Vector3 p)
    {
        allAttackCount += Mathf.CeilToInt(atk);
        ParticleManager.Instance.MergeText(allAttackCount, p, Color.red);
    }

    private void Main()
    {
        EventManager.OnBallMain.Trigger(0);
        currentBall.GetComponent<BallBase>().Unfreeze();
        currentBall.transform.SetParent(ballContainer.transform);
    }

    private void Alt()
    {
        EventManager.OnBallAlt.Trigger(0);
        var enemyCount = GameManager.Instance.enemyContainer.GetCurrentEnemyCount();
        currentBall.GetComponent<BallBase>().AltFire(enemyCount, attackMagnification);
        currentBall = null;
        // alt発動してボールも落ちてくる
    }

    private void DecideNextBall()
    {
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = null;

        // リロードするかどうか
        if (--remainingBalls > 0)
        {
            currentBall = nextBall;
            if(!currentBall) currentBall = InventoryManager.Instance.GetRandomBall();
            currentBall.transform.position = fallAnchor.transform.position;
            currentBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            fallAnchor.GetComponent<HingeJoint2D>().connectedBody = currentBall.GetComponent<Rigidbody2D>();
            if (remainingBalls > 1)
            {
                nextBall = InventoryManager.Instance.GetRandomBall();
                nextBall.transform.position = nextBallPosition;
            }
            else
            {
                nextBall = null;
            }
        }
        else
        {
            currentBall = null;
            nextBall = null;
            fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = false;
            DOTween.To(() => arrowMaterial.GetFloat(alpha), x => arrowMaterial.SetFloat(alpha, x), 0, 0.5f);
        }
        ballCountText.text = remainingBalls + "/" + ballPerOneTurn;
    }
    
    private bool IsAllBallsStopped()
    {
        if (GameManager.Instance.state != GameManager.GameState.Merge || remainingBalls != 0) return false;
        
        if(stopTimers == null || timerCount != ballContainer.GetComponentsInChildren<Rigidbody2D>().Length){
            stopTimers = ballContainer.GetComponentsInChildren<Rigidbody2D>().ToDictionary(b => b, _ => Time.time);
            timerCount = stopTimers.Count;
        }
        
        foreach (var b in stopTimers.Keys)
        {
            if (b.velocity.magnitude > 0.05f) return false;
            if (Time.time - stopTimers[b] < 0.5f) return false;
        }

        stopTimers = null;
        return true;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ballContainer = new GameObject("BallContainer");
        wall.SetWallWidth(wallWidths[0]);
        wallMaterial.bounciness = 0.0f;
        fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = false;
        arrowMaterial.SetFloat(alpha, 0);
        remainingBalls = 0;
        ballCountText.text = remainingBalls + "/" + ballPerOneTurn;
    }

    private void Start()
    {
        // if (Application.isEditor) coolTime = 0.1f;
        ballGauge.GetComponent<SpriteRenderer>().material.SetFloat(ratio, 1);
        fallAnchor.transform.position = currentBallPosition;
    }

    private void Update()
    {
        if (IsAllBallsStopped())
        {
            GameManager.Instance.ChangeState(GameManager.GameState.PlayerAttack);
        }
        
        if(!currentBall) return;
        if (GameManager.Instance.isGameOver) return;

        
        limit = wall.WallWidth / 2 + 0.05f;
        var size = currentBall.transform.localScale.x + 0.5f;
        var r = Mathf.Min(1, (Time.time - lastFallTime) / COOL_TIME);
        ballGauge.GetComponent<SpriteRenderer>().material.SetFloat(ratio, r + 0.1f);
        ballGauge.transform.localScale = currentBall.transform.localScale * 1.01f;
        ballGauge.transform.position = currentBall.transform.position;
        

        var mousePosX = GameManager.Instance.uiCamera.ScreenToWorldPoint(Input.mousePosition).x;
        var isMouseOvered = mousePosX > -limit + size / 2 && mousePosX < limit - size / 2;
        if (isMouseOvered)
        {
            mousePosX = Mathf.Clamp(mousePosX, -limit + size / 2, limit - size / 2);
            currentBallPosition = new Vector3(mousePosX, currentBallPosition.y, currentBallPosition.z);
        }
        else
        {
            if (Input.GetKey(KeyCode.A) && currentBallPosition.x - size / 2 > -limit)
            {
                currentBallPosition += Vector3.left * (MOVE_SPEED * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D) && currentBallPosition.x + size / 2 < limit)
            {
                currentBallPosition += Vector3.right * (MOVE_SPEED * Time.deltaTime);
            }
        }

        if (Time.time - lastFallTime <= COOL_TIME || remainingBalls < 0) return;

        var isMain = Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButton(0) && isMouseOvered);
        var isAlt = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || (Input.GetMouseButton(1) && isMouseOvered);
        
        if (isMain)
        {
            SeManager.Instance.PlaySe("fall");
            lastFallTime = Time.time;
            Main();
            DecideNextBall();
        }
        else if (isAlt)
        {
            SeManager.Instance.PlaySe("fall");
            lastFallTime = Time.time;
            Alt();
            DecideNextBall();
        }
        
        fallAnchor.transform.position = currentBallPosition + new Vector3(0, 0, 0);
    }
}