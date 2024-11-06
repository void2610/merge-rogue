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
    [SerializeField] private GameObject mergeParticle;
    [SerializeField] private GameObject fallAnchor;
    [SerializeField] private Material arrowMaterial;
    [SerializeField] private GameObject ballGauge;
    [SerializeField] private TextMeshProUGUI ballCountText;
    [SerializeField] private AttackCountUI attackCountUI;
    
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
    private Vector3 currentBallPosition = new(0, 1.5f, 0);
    private readonly Vector3 nextBallPosition = new(-3.5f, 1, 0);
    private const float MOVE_SPEED = 1.0f;
    private const float COOL_TIME = 0.5f;
    private int ballPerOneTurn = 2;
    private int remainingBalls;
    private int attackCount;
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
        GameManager.Instance.ChangeState(GameManager.GameState.StageMoving);
    }
    
    public void ResetRemainingBalls()
    {
        remainingBalls = ballPerOneTurn;

        if (ballPerOneTurn > 1)
        {
            nextBall = InventoryManager.instance.GetRandomBall(nextBallPosition);
        }
        currentBall = InventoryManager.instance.GetRandomBall(fallAnchor.transform.position - Vector3.up * 0.2f);
        currentBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = currentBall.GetComponent<Rigidbody2D>();
        
        ballCountText.text = remainingBalls + "/" + ballPerOneTurn;
        fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = true;
        DOTween.To(() => arrowMaterial.GetFloat(alpha), x => arrowMaterial.SetFloat(alpha, x), 1, 0.5f);
    }

    public void SpawnBall(int level, Vector3 p, Quaternion q)
    {
        var ball = InventoryManager.instance.GetBallByLevel(level);
        if (ball == null) return;

        ball.transform.position = p;
        ball.transform.rotation = q;
        ball.transform.SetParent(ballContainer.transform);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        int i = Random.Range(0, 5);
        SeManager.Instance.PlaySe("ball" + i);
        Instantiate(mergeParticle, p, Quaternion.identity);
    }

    public void Attack()
    {
        if (attackCount == 0)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack);
            return;
        }
        
        Debug.Log((int)(attackCount * attackMagnification));
        EventManager.OnPlayerAttack.Trigger((int)(attackCount * attackMagnification));
        var atk = EventManager.OnPlayerAttack.GetAndResetValue();
        Debug.Log(atk);

        foreach (var e in GameManager.Instance.enemyContainer.GetAllEnemies())
        {
            e.TakeDamage(atk);
        }

        GameManager.Instance.player.gameObject.transform.DOMoveX(0.75f, 0.02f).SetRelative(true).OnComplete(() =>
        {
            GameManager.Instance.player.gameObject.transform.DOMoveX(-0.75f, 0.2f).SetRelative(true)
                .SetEase(Ease.OutExpo);
        });

        SeManager.Instance.PlaySe("playerAttack");
        Camera.main?.GetComponent<CameraMove>().ShakeCamera(0.5f, 0.3f);
        attackCount = 0;
        attackCountUI.SetAttackCount(0);

        if (GameManager.Instance.enemyContainer.GetEnemyCount() > 0)
        {
            Utils.Instance.WaitAndInvoke(0.75f,
                () => GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack));
        }
    }

    public void AddAttackCount(float atk)
    {  
        attackCount += (int)atk;
        attackCountUI.SetAttackCount(attackCount);
    }

    private void FallAndDecideNextBall()
    {
        currentBall.GetComponent<BallBase>().Unfreeze();
        currentBall.transform.SetParent(ballContainer.transform);
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = null;
        if (--remainingBalls > 0)
        {
            currentBall = nextBall;
            if(!currentBall) currentBall = InventoryManager.instance.GetRandomBall();
            currentBall.transform.position = fallAnchor.transform.position;
            currentBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            fallAnchor.GetComponent<HingeJoint2D>().connectedBody = currentBall.GetComponent<Rigidbody2D>();
            if (remainingBalls > 1)
            {
                nextBall = InventoryManager.instance.GetRandomBall();
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
        ballGauge.GetComponent<SpriteRenderer>().material.SetFloat(ratio, r);
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
        fallAnchor.transform.position = currentBallPosition + new Vector3(2, 0, 0);
    }
}