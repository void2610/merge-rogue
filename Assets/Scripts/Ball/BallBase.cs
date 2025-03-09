using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class BallBase : MonoBehaviour
{
    private static int _ballSerial;
    public const int MAX_LEVEL = 3;

    public int Rank { get; private set; } = -1;
    public int NextRank { get; protected set; } = 0;
    public int Level { get; private set; } = 0;
    public float Size { get; private set; } = 0;
    public float Attack { get; private set; } = 0;
    public int Serial { get; private set; } = 0;
    public BallData Data { get; private set; }
    public bool IsFrozen { get; private set; } = false;
    public bool isDestroyed;
    public bool isMergable = true;
    public bool useIcon = true;

    private List<float> _attacks = new();
    private List<float> _sizes = new();
    
    public void Freeze()
    {
        IsFrozen = true;
    }

    public void Unfreeze()
    {
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
    
    public void Upgrade()
    {
        if (Level < MAX_LEVEL - 1)
        {
            Level++;
            Attack = _attacks[Level];
            Size = _sizes[Level];
        }
    }

    public virtual void InitBall(BallData d, int rank, int level = 0)
    {
        Serial = _ballSerial++;
        this.Data = d;
        this.Rank = rank;
        this.NextRank = rank + 1;
        this._sizes = d.sizes.Select(x => x * 1.2f).ToList();
        this._attacks = d.attacks;
        this.Level = level;
        this.Attack = _attacks[level];
        this.Size = _sizes[level];

        // 画像の設定
        if (useIcon)
        {
            this.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = d.sprite;
            this.transform.Find("IconShadow").GetComponent<SpriteRenderer>().sprite = d.sprite;
        }
        else
        {
            this.transform.Find("Icon").GetComponent<SpriteRenderer>().enabled = false;
            this.transform.Find("IconShadow").GetComponent<SpriteRenderer>().enabled = false;
            this.GetComponent<SpriteRenderer>().sprite = d.sprite;
        }

        this.GetComponent<SpriteRenderer>().sprite = ContentProvider.Instance.GetBallBaseImage(d.shapeType);
        
        // コライダーの設定
        if (d.shapeType != BallShapeType.Circle)
        {
            if(TryGetComponent(out CircleCollider2D tmp)) Destroy(tmp);
            
            switch (d.shapeType)
            {
                case BallShapeType.Square:
                    var bc = this.gameObject.AddComponent<BoxCollider2D>();
                    bc.size = new Vector2(1, 1);
                    break;
                case BallShapeType.Triangle:
                    var pc = this.gameObject.AddComponent<PolygonCollider2D>();
                    pc.points = new Vector2[]
                    {
                        new Vector2(0, 0.5f),
                        new Vector2(0.5f, -0.5f),
                        new Vector2(-0.5f, -0.5f),
                    };
                    break;
                case BallShapeType.Rectangle:
                    var bc2 = this.gameObject.AddComponent<BoxCollider2D>();
                    bc2.size = new Vector2(1, 0.5f);
                    break;
                case BallShapeType.Circle:
                    break;
                default:
                    break;
            }
        }
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
                EventManager.OnBallMerged.Trigger((this, b));
                
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
    
    protected void DefaultMergeParticle()
    {
        ParticleManager.Instance.MergeParticle(this.transform.position);
        ParticleManager.Instance.MergePowerParticle(this.transform.position, MyEnumUtil.GetBallColor(Rank-1));
        
        var i = Random.Range(0, 5);
        SeManager.Instance.PlaySe("ball" + i);
    }
}
