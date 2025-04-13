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
    
    public float attackMagnification = 1.0f;
    public int RemainingBalls { get; private set; } = 0;
    public GameObject CurrentBall { get; private set; } = null;
    public GameObject NextBall { get; private set; } = null;
    public MergeWall Wall => wall;
    
    private const float MOVE_SPEED = 1.0f;
    private const float COOL_TIME = 1.0f;
    private readonly List<float> _attacks = new() { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 3.75f, 4.0f};
    private readonly List<float> _wallWidths = new() { 2.0f, 2.75f, 3.5f, 4.25f, 5.0f, 5.75f, 6.0f, 6.25f};
    private int _wallWidthLevel = 0;
    private int _attackLevel = 0;
    private GameObject _ballContainer;
    private float _lastFallTime;
    private float _limit = -2.5f;
    private Vector3 _currentBallPosition = new(0, 1f, 0);
    private int _ballPerOneTurn = 2;
    private Dictionary<AttackType, int> _attackCounts = new();
    private Dictionary<Rigidbody2D, float> _stopTimers;
    private bool _isMovable = false;
    private Camera _mainCamera;
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
    
    public void StartMerge()
    {
        _isMovable = true;
        Reset();
        
        _fillingRateMagnification = FillingRateManager.Instance.CalcFillingGauge();
    }
    
    public async UniTaskVoid EndMerge()
    {
        if(!_isMovable) return;
        
        if (NextBall)
        {
            Destroy(NextBall);
            NextBall = null;
        }
        if (CurrentBall)
        {
            Destroy(CurrentBall);
            CurrentBall = null;
        }
        
        ballCountText.text = "0/" + _ballPerOneTurn;
        arrow.DOFade(0, 0.5f).Forget();
        _isMovable = false;
        
        await UniTask.Delay(1000);
        
        attackCountUI.SetAttackCount(0);
        GameManager.Instance.ChangeState(GameManager.GameState.PlayerAttack);
    }
    
    // 次のボールを生成
    private void Reset()
    {
        RemainingBalls = _ballPerOneTurn;
        if (_ballPerOneTurn > 1)
        {
            NextBall = InventoryManager.Instance.GetRandomBall(nextBallPosition);
        }
        CurrentBall = InventoryManager.Instance.GetRandomBall(fallAnchor.transform.position - Vector3.up * 0.2f);
        CurrentBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = CurrentBall.GetComponent<Rigidbody2D>();
        
        ballCountText.text = RemainingBalls + "/" + _ballPerOneTurn;
        fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = true;
        arrow.DOFade(1, 0.5f).Forget();
    }

    public void AddAttackCount(AttackType type, float atk, Vector3 p)
    {
        // プレイヤー攻撃力を適用
        atk *= attackMagnification;
        
        _attackCounts[type] = _attackCounts.ContainsKey(type) ? _attackCounts[type] + (int)atk : (int)atk;
        ParticleManager.Instance.MergeText((int)atk, p, type.GetColor());
        attackCountUI.SetAttackCount(_attackCounts.Sum(a => a.Value));
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

    private void DropBall()
    {
        EventManager.OnBallDrop.Trigger(0);
        CurrentBall.GetComponent<BallBase>().Unfreeze();
        CurrentBall.transform.SetParent(_ballContainer.transform);
    }

    private void SkipBall()
    {
        EventManager.OnBallSkip.Trigger(0);
        Destroy(CurrentBall);
    }

    private async UniTaskVoid DecideNextBall()
    {
        if(!_isMovable) return;
        
        fallAnchor.GetComponent<HingeJoint2D>().connectedBody = null;

        // リロードするかどうか
        if (--RemainingBalls > 0)
        {
            await UniTask.Delay((int)(COOL_TIME * 500));
            CurrentBall = NextBall;
            CurrentBall.transform.position = fallAnchor.transform.position;
            CurrentBall.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            fallAnchor.GetComponent<HingeJoint2D>().connectedBody = CurrentBall.GetComponent<Rigidbody2D>();
            if (RemainingBalls > 1)
            {
                NextBall = InventoryManager.Instance.GetRandomBall();
                NextBall.transform.position = nextBallPosition;
            }
            else
            {
                NextBall = null;
            }
        }
        else
        {
            CurrentBall = null;
            NextBall = null;
            fallAnchor.GetComponent<HingeJoint2D>().useConnectedAnchor = false;
            arrow.DOFade(0, 0.5f).Forget();
        }
        ballCountText.text = RemainingBalls + "/" + _ballPerOneTurn;
    }
    
    private bool IsAllBallsStopped()
    {
        if (GameManager.Instance.state != GameManager.GameState.Merge || RemainingBalls != 0) return false;
        if(Time.time - _lastFallTime < COOL_TIME) return false;
        
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
        fallAnchor.transform.position = _currentBallPosition;
        _mainCamera = Camera.main;
        
        ResetAttackCount();
    }

    private void Update()
    {
        if (IsAllBallsStopped())
        {
            EndMerge().Forget();
        }
        
        if(!CurrentBall) return;
        if (GameManager.Instance.IsGameOver) return;
        if (GameManager.Instance.state != GameManager.GameState.Merge) return;
        if (!_isMovable) return;
        
        _limit = wall.WallWidth / 2 + 0.05f;
        var size = CurrentBall.transform.localScale.x + 0.5f;
        var r = Mathf.Min(1, (Time.time - _lastFallTime) / COOL_TIME);
        ballGauge.GetComponent<SpriteRenderer>().material.SetFloat(_ratio, r + 0.1f);
        ballGauge.transform.localScale = CurrentBall.transform.localScale * 1.01f;
        ballGauge.transform.position = CurrentBall.transform.position;
        
        // プレイヤー操作        
        if(UIManager.Instance.IsAnyCanvasGroupEnabled()) return;
        if (EventSystem.current.currentSelectedGameObject != cursorSetter.gameObject) return;
        
        var mousePosX = (InputProvider.Instance.GetMousePosition().x - Screen.width / 2) / Screen.width * 20;
        var isMouseOvered = cursorSetter.IsMergeArea;
        if (isMouseOvered)
        {
            mousePosX = Mathf.Clamp(mousePosX, -_limit + size / 2, _limit - size / 2);
            _currentBallPosition = new Vector3(mousePosX, _currentBallPosition.y, _currentBallPosition.z);
        }
        else
        {
            if (InputProvider.Instance.Gameplay.LeftMove.IsPressed() && _currentBallPosition.x - size / 2 > -_limit)
            {
                _currentBallPosition += Vector3.left * (MOVE_SPEED * Time.deltaTime);
            }

            if (InputProvider.Instance.Gameplay.RightMove.IsPressed() && _currentBallPosition.x + size / 2 < _limit)
            {
                _currentBallPosition += Vector3.right * (MOVE_SPEED * Time.deltaTime);
            }
        }

        fallAnchor.transform.position = _currentBallPosition + new Vector3(0, 0, 0);
        
        if (Time.time - _lastFallTime <= COOL_TIME || RemainingBalls < 0) return;

        var left = InputProvider.Instance.Gameplay.LeftClick.IsPressed();
        var right = InputProvider.Instance.Gameplay.RightClick.IsPressed();
        
        if (left)
        {
            SeManager.Instance.PlaySe("fall");
            _lastFallTime = Time.time;
            DropBall();
            DecideNextBall().Forget();
        }
        else if (right)
        {
            SeManager.Instance.PlaySe("alt");
            _lastFallTime = Time.time;
            SkipBall();
            DecideNextBall().Forget();
        }
    }
}