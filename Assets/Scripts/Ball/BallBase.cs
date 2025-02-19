using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallBase : MonoBehaviour
{
    private static int _ballSerial;
    public const int MAX_LEVEL = 3;

    public int Rank { get; private set; } = -1;
    public int Level { get; private set; } = 0;
    public float Size { get; private set; } = 0;
    public float Attack { get; private set; } = 0;
    public int Serial { get; private set; } = 0;
    public BallData Data { get; private set; }
    public bool IsFrozen { get; private set; } = false;
    public bool isDestroyed;
    public bool isMergable = true;

    private List<float> _attacks = new();
    private List<float> _sizes = new();
    
    public void Freeze()
    {
        IsFrozen = true;
    }

    public void Unfreeze()
    {
        IsFrozen = false;
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
        this._sizes = d.sizes;
        this._attacks = d.attacks;
        this.Level = level;
        this.Attack = _attacks[level];
        this.Size = _sizes[level];
    }

    private void Start()
    {
        if(Rank == -1)
        {
            Debug.LogError("Ball is not initialized");
            return;
        }
        
        // アニメーション
        var tmp = transform.localScale.x;
        transform.localScale = Vector3.zero;
        transform.DOScale(tmp, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isDestroyed || IsFrozen || !isMergable) return;

        if (other.gameObject.TryGetComponent(out BallBase b))
        {
            if (b.Rank == this.Rank && !b.IsFrozen && !b.isDestroyed && b.isMergable)
            {
                if (this.Serial < b.Serial)
                {
                    var pos = (this.transform.position + b.transform.position) / 2;
                    EventManager.OnBallMerged.Trigger((this, b));
                    
                    var center = (this.transform.position + other.transform.position) / 2;
                    var rotation = Quaternion.Lerp(this.transform.rotation, other.transform.rotation, 0.5f);
                    MergeManager.Instance.SpawnBallFromLevel(Rank + 1, center, rotation);

                    EffectAndDestroy(b);
                    b.EffectAndDestroy(this);
                }
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
    
    protected void DefaultMergeParticle()
    {
        ParticleManager.Instance.MergeParticle(this.transform.position);
        ParticleManager.Instance.MergePowerParticle(this.transform.position, MyColors.GetBallColor(Rank-1));
        
        var i = Random.Range(0, 5);
        SeManager.Instance.PlaySe("ball" + i);
    }
}
