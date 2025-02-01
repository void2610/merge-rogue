using UnityEngine;
using DG.Tweening;

public class BallBase : MonoBehaviour
{
    private static int ballSerial;

    public int Level { get; private set; } = -1;
    public float Size { get; private set; } = 0;
    public float Attack { get; private set; } = 0;
    public int Serial { get; private set; } = 0;
    public BallData Data { get; private set; }
    public bool IsFrozen { get; private set; } = false;
    public bool isDestroyed;
    
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

    public void InitBall(BallData d, int level, int rank = 0)
    {
        Serial = ballSerial++;
        this.Data = d;
        this.Level = level;
        this.Size = d.sizes[rank];
        this.Attack = d.attacks[rank];
        transform.localScale = new Vector3(Size, Size, Size);
    }

    private void Start()
    {
        if(Level == -1)
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
        if (isDestroyed || IsFrozen) return;

        if (other.gameObject.TryGetComponent(out BallBase b))
        {
            if (b.Level == this.Level && !b.IsFrozen && !b.isDestroyed)
            {
                if (this.Serial < b.Serial)
                {
                    EventManager.OnBallMerged.Trigger(this.Level);
                    
                    var center = (this.transform.position + other.transform.position) / 2;
                    var rotation = Quaternion.Lerp(this.transform.rotation, other.transform.rotation, 0.5f);
                    MergeManager.Instance.SpawnBallFromLevel(Level + 1, center, rotation);

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
            Destroy(gameObject);
        }).SetLink(gameObject);
    }
    
    protected void DefaultMergeParticle()
    {
        ParticleManager.Instance.MergeParticle(this.transform.position);
        ParticleManager.Instance.MergePowerParticle(this.transform.position, MyColors.GetBallColor(Level-1));
        
        var i = Random.Range(0, 5);
        SeManager.Instance.PlaySe("ball" + i);
    }
}
