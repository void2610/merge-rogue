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
    private float limit = -2.5f;

    public GameObject currentBall;
    public GameObject nextBall;
    private GameObject ballContainer;
    private float lastFallTime;
    
    private readonly List<float> wallWidths = new() { 2.5f, 2.75f, 3.0f, 3.25f, 3.5f, 3.75f, 4.0f, 4.25f, 4.5f, 4.75f, 5.0f, 5.25f, 5.5f, 5.75f, 6.0f, 6.25f, 6.5f, 6.75f, 7.0f, 7.25f, 7.5f, 7.75f, 8.0f, 8.25f, 8.5f, 8.75f, 9.0f, 9.25f, 9.5f, 9.75f, 10.0f };
    private int wallWidthLevel;
    private readonly List<float> attacks = new() { 1.0f, 1.25f, 1.5f, 1.75f, 2.0f, 2.25f, 2.5f, 2.75f, 3.0f, 3.25f, 3.5f, 3.75f, 4.0f, 4.25f, 4.5f, 4.75f, 5.0f, 5.25f, 5.5f, 5.75f, 6.0f };
    private int attackLevel;
    private Vector3 currentBallPosition = new(0, 1f, 0);
    private const float MOVE_SPEED = 1.0f;
    private const float COOL_TIME = 0.5f;
    private int ballPerOneTurn = 2;
    private int remainingBalls;
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

    public void SpawnBall(int level, Vector3 p, Quaternion q)
    {
        var ball = InventoryManager.Instance.GetBallByLevel(level);
        if (ball == null) return;

        ball.transform.position = p;
        ball.transform.rotation = q;
        ball.transform.SetParent(ballContainer.transform);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
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
        singleAttackCount += (int)atk;
        ParticleManager.Instance.MergeText(singleAttackCount, p);
    }
    
    public void AddAllAttackCount(float atk, Vector3 p)
    {
        allAttackCount += (int)atk;
        ParticleManager.Instance.MergeText(allAttackCount, p, Color.red);
    }

    private void FallAndDecideNextBall()
    {
        EventManager.OnBallDropped.Trigger(0);
        currentBall.GetComponent<BallBase>().Unfreeze();
        currentBall.transform.SetParent(ballContainer.transform);
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = null;
        
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
        ballCountText.text = remainingBalls + "/" + ballPerOneTurn;
    }

    private void Start()
    {
        // if (Application.isEditor) coolTime = 0.1f;
        ballGauge.GetComponent<SpriteRenderer>().material.SetFloat(ratio, 1);
        fallAnchor.transform.position = currentBallPosition;
        // FIXME: currentBallと落としたballが反応して消える
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)) LevelUpWallWidth();
        if (IsAllBallsStopped())
        {
            GameManager.Instance.ChangeState(GameManager.GameState.PlayerAttack);
        }
        if(remainingBalls < 1) return;
        
        limit = wall.WallWidth / 2 + 0.05f;
        float size = currentBall.transform.localScale.x + 0.5f;
        float r = Mathf.Min(1, (Time.time - lastFallTime) / COOL_TIME);
        ballGauge.GetComponent<SpriteRenderer>().material.SetFloat(ratio, r + 0.1f);
        ballGauge.transform.localScale = currentBall.transform.localScale * 1.01f;
        ballGauge.transform.position = currentBall.transform.position;
        
        if (Input.GetKey(KeyCode.A) && currentBallPosition.x - size / 2 > -limit)
        {
            currentBallPosition += Vector3.left * (MOVE_SPEED * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D) && currentBallPosition.x + size / 2 < limit)
        {
            currentBallPosition += Vector3.right * (MOVE_SPEED * Time.deltaTime);
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastFallTime > COOL_TIME && remainingBalls > 0)
        {
            SeManager.Instance.PlaySe("fall");
            lastFallTime = Time.time;
            FallAndDecideNextBall();
        }
        
        if(Input.GetKeyDown(KeyCode.L)) LevelUpWallWidth();
        fallAnchor.transform.position = currentBallPosition + new Vector3(0, 0, 0);
    }
}