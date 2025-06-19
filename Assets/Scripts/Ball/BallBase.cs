using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using VContainer;

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
        
        // 既に接触しているオブジェクトを取得
        var contacts = new ContactPoint2D[10]; // 最大10個まで接触しているオブジェクトを取得
        var contactCount = GetComponent<Rigidbody2D>().GetContacts(contacts);

        for (var i = 0; i < contactCount; i++)
        {
            var otherCollider = contacts[i].collider;
            if (otherCollider && otherCollider.TryGetComponent(out BallBase b))
            {
                HandleCollision(b);
                b.HandleCollision(this);
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
        polygonCollider2D.pathCount = physicsShapeCount;
        var physicsShape = new List<Vector2>();
        for ( var i = 0; i < physicsShapeCount; i++ )
        {
            physicsShape.Clear();
            d.sprite.GetPhysicsShape( i, physicsShape );
            var points = physicsShape.ToArray();
            polygonCollider2D.SetPath( i, points );
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
        if (b.Rank == this.Rank && !b.IsFrozen && !b.isDestroyed && b.isMergable)
        {
            if (this.Serial < b.Serial)
            {
                var pos = (this.transform.position + b.transform.position) / 2;
                EventManager.OnBallMerged.OnNext((this, b));
                
                var center = (this.transform.position + b.transform.position) / 2;
                var rotation = Quaternion.Lerp(this.transform.rotation, b.transform.rotation, 0.5f);
                MergeManager.Instance.SpawnBallFromLevel(NextRank, center, rotation);

                EffectAndDestroy(b);
                b.EffectAndDestroy(this);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out BallBase b)) HandleCollision(b);
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
}
