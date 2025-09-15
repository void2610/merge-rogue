using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using VContainer;
using R3;

public class BallBase : MonoBehaviour
{
    private static int _ballSerial;
    public const int MAX_LEVEL = 3;

    public int Rank { get; private set; } = -1;
    public int NextRank { get; protected set; } = 0;
    public int Level { get; private set; } = 0;
    public float Size { get; private set; } = 0;
    public float Attack { get; private set; } = 0;
    public float Weight { get; private set; } = 0;
    public int Serial { get; private set; } = 0;
    public BallData Data { get; private set; }
    public bool IsFrozen { get; private set; } = false;
    public bool isDestroyed;
    public bool isMergable = true;
    public int elapsedTurns = 0;

    private List<float> _attacks = new();
    private List<float> _sizes = new();
    private List<float> _weights = new();
    
    protected IRandomService RandomService;
    
    [Inject]
    public void InjectDependencies(IRandomService randomService)
    {
        RandomService = randomService;
    }
    
    public void Freeze() => IsFrozen = true;
    public void Unfreeze() => UnfreezeAsync().Forget();

    private async UniTask UnfreezeAsync()
    {
        await UniTask.Delay(500);
        if (!this || isDestroyed) return;
            
        IsFrozen = false;
        
        // GetContactsは重い処理のため、マージ可能でない場合はスキップ
        if (!isMergable) return;
        
        // 既に接触しているオブジェクトを取得
        var contacts = new ContactPoint2D[10]; // 最大10個まで接触しているオブジェクトを取得
        var contactCount = GetComponent<Rigidbody2D>().GetContacts(contacts);

        for (var i = 0; i < contactCount; i++)
        {
            var otherCollider = contacts[i].collider;
            if (otherCollider && otherCollider.TryGetComponent(out BallBase b))
            {
                // 基本チェックを追加
                if (b.isDestroyed || b.IsFrozen || !b.isMergable) continue;
                
                // Serial番号が小さい方のみがHandleCollisionを実行することで、
                // 重複実行を避けてPhysics処理負荷を軽減する
                if (this.Serial < b.Serial)
                {
                    HandleCollision(b);
                }
            }
        }
    }

    protected virtual void Effect(BallBase other)
    {
        // Main Effect
    }
    
    protected virtual void TurnEndEffect()
    {
        // Turn End Effect
    }
    
    public void Upgrade()
    {
        if (Level < MAX_LEVEL - 1)
        {
            Level++;
            Attack = _attacks[Level];
            Size = _sizes[Level];
            Weight = _weights[Level];
        }
    }
    
    public void OnTurnEnd()
    {
        if (IsFrozen || isDestroyed) return;
        
        elapsedTurns++;
        TurnEndEffect();
    }

    public virtual void InitBall(BallData d, int rank, int level = 0)
    {
        Serial = _ballSerial++;
        this.Data = d;
        this.Rank = rank;
        this.NextRank = rank + 1;
        this._sizes = d.sizes;
        this._attacks = d.attacks;
        this._weights = d.weights;
        this.Level = level;
        this.Attack = _attacks[level];
        this.Size = _sizes[level];
        this.Weight = d.weights[level];
        
        // コライダーの設定
        var polygonCollider2D = this.GetComponent<PolygonCollider2D>();
        var physicsShapeCount = d.sprite.GetPhysicsShapeCount();
        
        if (physicsShapeCount == 0)
        {
            // physics shapeが見つからない場合は円形のコライダーを設定
            var circlePoints = new List<Vector2>();
            const int segments = 32;
            const float radius = 0.5f; // スプライトの半分のサイズ
            
            for (int i = 0; i < segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                circlePoints.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }
            
            polygonCollider2D.pathCount = 1;
            polygonCollider2D.SetPath(0, circlePoints.ToArray());
        }
        else
        {
            // physics shapeが存在する場合は通常通り設定
            polygonCollider2D.pathCount = physicsShapeCount;
            var physicsShape = new List<Vector2>();
            for (var i = 0; i < physicsShapeCount; i++)
            {
                physicsShape.Clear();
                d.sprite.GetPhysicsShape(i, physicsShape);
                var points = physicsShape.ToArray();
                polygonCollider2D.SetPath(i, points);
            }
        }
        
        // 画像の設定
        this.GetComponent<SpriteRenderer>().sprite = d.sprite;
    }

    private void Start()
    {
        if(Rank == -1) throw new System.Exception("Rank is not set.");
        // アニメーション
        var tmp = transform.localScale.x;
        transform.localScale = Vector3.zero;
        transform.DOScale(tmp, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject);
    }
    
    protected　virtual void HandleCollision(BallBase b)
    {
        if (isDestroyed || IsFrozen || !isMergable) return;
        if (b.isDestroyed || b.IsFrozen || !b.isMergable) return;
        
        if (b.Rank == this.Rank)
        {
            EventManager.OnBallMerged.OnNext((this, b));
            
            MergeManager.Instance.SpawnBallFromLevel(NextRank, this.transform.position, this.transform.rotation);

            EffectAndDestroy(b);
            b.EffectAndDestroy(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // 基本的な早期リターンチェックを追加（物理演算負荷軽減のため）
        if (isDestroyed || IsFrozen) return;
        
        if (other.gameObject.TryGetComponent(out BallBase b))
        {
            // 相手のボールも基本チェック
            if (b.isDestroyed || b.IsFrozen) return;
            
            // Serial番号が小さい方のみがHandleCollisionを実行することで、重複実行を避けてPhysics処理負荷を軽減する
            if (this.Serial < b.Serial)
            {
                HandleCollision(b);
            }
        }
    }
    
    public void EffectAndDestroy(BallBase other)
    {
        Effect(other);
        this.isDestroyed = true;
        
        transform.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(this.gameObject);
        }).SetLink(gameObject);
    }
    
    public void DestroyWithNoEffect()
    {
        this.isDestroyed = true;
        transform.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(this.gameObject);
        }).SetLink(gameObject);
    }
    
    protected void DefaultMergeParticle()
    {
        ParticleManager.Instance.MergeParticle(this.transform.position);
        // ParticleManager.Instance.MergePowerParticle(this.transform.position, MyEnumUtil.GetBallColor(Rank-1));
        SeManager.Instance.PlaySe("ball" + RandomService.RandomRange(0, 5));
    }
    
    private bool IsNearbyMerge((BallBase ball1, BallBase ball2) mergeData, float range = 1f)
    {
        var (b1, b2) = mergeData;
        if (!b1 || !b2 || !this) return false;
        
        var mergePosition = (b1.transform.position + b2.transform.position) * 0.5f;
        var rangeSqr = range * range;
        return (this.transform.position - mergePosition).sqrMagnitude < rangeSqr;
    }
    
    /// <summary>
    /// 近くのマージを監視するSubscriptionを作成
    /// </summary>
    /// <param name="onNearbyMerge">近くでマージが発生した時のコールバック</param>
    /// <param name="range">検知範囲</param>
    /// <returns>Subscription</returns>
    protected System.IDisposable SubscribeNearbyMerge(System.Action<(BallBase ball1, BallBase ball2)> onNearbyMerge, float range = 1f)
    {
        return EventManager.OnBallMerged
            .Subscribe(mergeData =>
            {
                if (IsNearbyMerge(mergeData, range))
                {
                    onNearbyMerge(mergeData);
                }
            })
            .AddTo(this);
    }
}
