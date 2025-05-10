using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using unityroom.Api;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance;
    private static readonly int _ratio = Shader.PropertyToID("_Ratio");
    private static readonly int _alpha = Shader.PropertyToID("_Alpha");

    [SerializeField] private MergeAreaCursorSetter cursorSetter;
    [SerializeField] private MergeWall wall;
    [SerializeField] public PhysicsMaterial2D wallMaterial;
    [SerializeField] private GameObject fallAnchor;
    [SerializeField] private SpriteRenderer arrow;
    [SerializeField] private GameObject ballGauge;
    [SerializeField] private TextMeshProUGUI ballCountText;
    [SerializeField] private Vector3 nextBallPosition;
    [SerializeField] private AttackCountUI attackCountUI;
    [SerializeField] private BallQte ballQte;
    
    public float attackMagnification = 1.0f;
    public int RemainingBalls { get; private set; } = 0;
    public GameObject CurrentBall { get; private set; } = null;
    public MergeWall Wall => wall;
    
    private const float MOVE_SPEED = 1.0f;
    private readonly List<float> _attacks = new() { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 3.75f, 4.0f};
    private readonly List<float> _wallWidths = new() { 4.75f, 5.0f, 5.25f, 5.5f, 5.75f, 6.0f, 6.25f, 6.5f};
    private int _wallWidthLevel = 0;
    private int _attackLevel = 0;
    private GameObject _ballContainer;
    private int _ballPerOneTurn = 2;
    private Dictionary<AttackType, int> _attackCounts = new();
    private Dictionary<Rigidbody2D, float> _stopTimers;
    private float _fillingRateMagnification;
    
    public void LevelUpWallWidth()
    {
        if (_wallWidthLevel < _wallWidths.Count - 1)
        {
            wall.SetWallWidth(_wallWidths[++_wallWidthLevel]);
        }
    }
    
    public void LevelUpAttack()
    {
        if (_attackLevel < _attacks.Count - 1)
        {
            attackMagnification = _attacks[++_attackLevel];
        }
    }
    
    public void LevelUpBallAmount()
    {
        _ballPerOneTurn++;
    }
    
    public PhysicsMaterial2D GetWallMaterial() => wallMaterial;
    public int GetBallCount() => _ballContainer.GetComponentsInChildren<Rigidbody2D>().Length;
    public void RemoveAllBalls() => _ballContainer.GetComponentsInChildren<Rigidbody2D>().ToList().ForEach(b => Destroy(b.gameObject));
    
    public Vector3 GetValidRandomPosition()
    {
        var width = wall.WallWidth;
        var r = GameManager.Instance.RandomRange(-width / 2 + 0.2f, width / 2 - 0.2f);
        return new Vector3(r, 0.6f, 0);
    }
    
    public void CreateRandomBall()
    {
        var ball = InventoryManager.Instance.GetRandomBall();
        ball.transform.position = GetValidRandomPosition();
        ball.transform.SetParent(_ballContainer.transform);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        ball.GetComponent<BallBase>().Unfreeze();
    }
    
    public void CreateBall(int rank, Vector3 p)
    {
        var ball = InventoryManager.Instance.GetBallByRank(rank);
        if (!ball) return;

        ball.transform.position = p;
        ball.transform.SetParent(_ballContainer.transform);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        ball.GetComponent<BallBase>().Unfreeze();
    }
    
    public void CreateBombBall()
    {
        var bomb = InventoryManager.Instance.GetBombBall();
        bomb.transform.position = GetValidRandomPosition();
        bomb.transform.SetParent(_ballContainer.transform);
        bomb.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }
    
    public void CreateDisturbBall()
    {
        var disturb = InventoryManager.Instance.GetDisturbBall();
        disturb.transform.position = GetValidRandomPosition();
        disturb.transform.SetParent(_ballContainer.transform);
        disturb.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }

    public void MergeAll()
    {
        for(var i = 0; i < _ballContainer.transform.childCount; i++)
        {
            if (!_ballContainer.transform.GetChild(i)) continue;
            var b = _ballContainer.transform.GetChild(i).GetComponent<BallBase>();
            if (!b) continue;
            b.EffectAndDestroy(null);
        }
    }
    
    public async UniTaskVoid StartMerge()
    {
        _fillingRateMagnification = FillingRateManager.Instance.CalcFillingGauge();
        Reset();
        
        for (var i = 0; i < _ballPerOneTurn; i++)
        {
            await DecideNextBall();
            await UniTask.Delay(500);
            await DropBallByQte();
            await UniTask.Delay(500);
            RemainingBalls--;
            ballCountText.text = RemainingBalls + "/" + _ballPerOneTurn;
        }
        
        CurrentBall = null;
        fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = false;
        arrow.DOFade(0, 0.5f).Forget();
        
        await UniTask.WaitUntil(() => IsAllBallsStopped());
        await UniTask.Delay(100);
        EndMerge().Forget();
    }

    private async UniTask DropBallByQte()
    {
        var limit = (wall.WallWidth / 2) + CurrentBall.transform.localScale.x - 0.5f;
        fallAnchor.transform.DOMoveX(-limit, 0f).Forget();
        var t = fallAnchor.transform.DOMoveX(limit, 1 / MOVE_SPEED).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        
        await UniTask.WaitUntil(() => InputProvider.Instance.Gameplay.LeftClick.IsPressed());
        
        t.Kill();
        EventManager.OnBallDrop.Trigger(0);
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = null;
        CurrentBall.GetComponent<BallBase>().Unfreeze();
        CurrentBall.transform.SetParent(_ballContainer.transform); 
    }
    
    private async UniTask DecideNextBall()
    {
        var rank = await ballQte.GetBallRankFromQte();
    
        CurrentBall = InventoryManager.Instance.GetBallByRank(rank + 1);
        CurrentBall.transform.position = fallAnchor.transform.position;
        CurrentBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = CurrentBall.GetComponent<Rigidbody2D>();
    }
    
    public async UniTaskVoid EndMerge()
    {
        if (CurrentBall)
        {
            Destroy(CurrentBall);
            CurrentBall = null;
        }
        
        // ボールのターンエンド時の処理を行う
        for(var i = 0; i < _ballContainer.transform.childCount; i++)
        {
            if (!_ballContainer.transform.GetChild(i)) continue;
            var b = _ballContainer.transform.GetChild(i).GetComponent<BallBase>();
            if (!b) continue;
            b.OnTurnEnd();
        }
        
        ballCountText.text = "0/" + _ballPerOneTurn;
        arrow.DOFade(0, 0.5f).Forget();
        
        await UniTask.Delay(200);
        
        attackCountUI.SetAttackCount(0);
        
        // 敵が残っていたら敵の攻撃へ
        if (EnemyContainer.Instance.GetCurrentEnemyCount() > 0)
        {
            await UniTask.Delay(750);
            GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack);
        }
    }
    
    // 次のボールを生成
    private void Reset()
    {
        RemainingBalls = _ballPerOneTurn;
        ballCountText.text = RemainingBalls + "/" + _ballPerOneTurn;
        fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = true;
        arrow.DOFade(1, 0.5f).Forget();
    }

    public void AddAttackCount(AttackType type, float atk, Vector3 p) => AddAttackCountAsync(type, atk, p).Forget();
    
    private async UniTaskVoid AddAttackCountAsync(AttackType type, float atk, Vector3 p)
    {
        // プレイヤー攻撃力を適用
        atk *= attackMagnification;
        
        _attackCounts[type] = _attackCounts.ContainsKey(type) ? _attackCounts[type] + (int)atk : (int)atk;
        GameManager.Instance.EnemyContainer.AttackEnemy(_attackCounts).Forget();
        
        // ヒットストップ
        Time.timeScale = 0.1f;
        await UniTask.Delay((int)(350 / GameManager.Instance.TimeScale), DelayType.UnscaledDeltaTime);
        Time.timeScale = GameManager.Instance.TimeScale;
        
        ResetAttackCount();
        
        ParticleManager.Instance.MergeText((int)atk, p, type.GetColor());
        // attackCountUI.SetAttackCount(_attackCounts.Sum(a => a.Value));
    }
    
    public void SpawnBallFromLevel(int level, Vector3 p, Quaternion q)
    {
        var ball = InventoryManager.Instance.GetBallByRank(level);
        if (!ball) return;

        ball.transform.position = p;
        ball.transform.rotation = q;
        ball.transform.SetParent(_ballContainer.transform);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }

    public async UniTaskVoid Attack()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack);
        ResetAttackCount();
        return;
        
        
        // 攻撃がない場合は敵の攻撃に移行
        var canAttack = _attackCounts.Any(a => a.Value != 0);
        // Freeze状態なら行動しない
        var freeze = (FreezeEffect)GameManager.Instance.Player.StatusEffects.Find(e => e.Type == StatusEffectType.Freeze);
        var isFrozen = freeze != null && freeze.IsFrozen();
        if (!canAttack || isFrozen)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.EnemyAttack);
            return;
        }
        
        // 充填率のバフを適用
        _attackCounts.MultiplyAll(_fillingRateMagnification);
        
        // 状態異常を適用
        _attackCounts = GameManager.Instance.Player.ModifyOutgoingAttack(_attackCounts);
        
        // イベントを適用
        EventManager.OnPlayerAttack.Trigger(_attackCounts);
        _attackCounts = EventManager.OnPlayerAttack.GetValue();
        
        // ハイスコア更新
        var totalAttack = _attackCounts.Sum(a => a.Value);
        if (PlayerPrefs.GetInt("maxAttack", 0) < totalAttack)
        {
            UnityroomApiClient.Instance.SendScore(2, totalAttack, ScoreboardWriteMode.HighScoreDesc);
            PlayerPrefs.SetInt("maxAttack", totalAttack);
        }
        
        // 攻撃アニメーション
        GameManager.Instance.Player.gameObject.transform.DOMoveX(0.75f, 0.02f).SetRelative(true).OnComplete(() =>
        {
            GameManager.Instance.Player.gameObject.transform.DOMoveX(-0.75f, 0.2f).SetRelative(true)
                .SetEase(Ease.OutExpo);
        }).Forget();
        
        // 実際の攻撃処理
        await GameManager.Instance.EnemyContainer.AttackEnemy(_attackCounts);
        ResetAttackCount();
    }


    
    private bool IsAllBallsStopped()
    {
        if (GameManager.Instance.state != GameManager.GameState.Merge || RemainingBalls != 0) return false;
        
        if(_stopTimers == null || _stopTimers.Count != _ballContainer.GetComponentsInChildren<Rigidbody2D>().Length){
            _stopTimers = _ballContainer.GetComponentsInChildren<Rigidbody2D>().ToDictionary(b => b, _ => Time.time);
        }
        
        foreach (var b in _stopTimers.Keys)
        {
            if(!b) continue;
            if (b.linearVelocity.magnitude > 0.05f) return false;
            if (Time.time - _stopTimers[b] < 1f) return false;
        }

        _stopTimers = null;
        return true;
    }

    private void ResetAttackCount()
    {
        // enumの全要素で、0を代入
        foreach (AttackType type in System.Enum.GetValues(typeof(AttackType)))
        {
            _attackCounts[type] = 0;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _ballContainer = new GameObject("BallContainer");
        wall.SetWallWidth(_wallWidths[0]);
        wallMaterial.bounciness = 0.0f;
        fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = false;
        arrow.DOFade(0, 0).Forget();
        RemainingBalls = 0;
        ballCountText.text = RemainingBalls + "/" + _ballPerOneTurn;
    }

    private void Start()
    {
        ballGauge.GetComponent<SpriteRenderer>().material.SetFloat(_ratio, 1);
        
        ResetAttackCount();
    }
}