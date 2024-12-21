using UnityEngine;
using DG.Tweening;

public class BallBase : MonoBehaviour
{
    private static int ballSerial;

    public int level { get; private set; } = -1;
    public float size { get; private set; } = 0;
    public float attack { get; private set; } = 0;
    public int serial { get; private set; } = 0;
    public BallData data { get; private set; }
    public bool isFrozen { get; private set; } = false;
    public bool isDestroyed;
    
    public void Freeze()
    {
        isFrozen = true;
    }

    public void Unfreeze()
    {
        isFrozen = false;
    }

    protected virtual void Effect(BallBase other)
    {
        // Main Effect
    }
    
    public virtual void AltFire(int enemyCount,  float playerAttack)
    {
        // Alt Effect
        ParticleManager.Instance.MergeParticle(this.transform.position);
        Destroy(this.gameObject);
    }

    public void InitBall(BallData d, int l)
    {
        serial = ballSerial++;
        this.data = d;
        this.level = l;
        this.size = d.size;
        this.attack = d.atk;
        transform.localScale = new Vector3(size, size, size);
    }

    private void Start()
    {
        if(level == -1)
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
        if (isDestroyed || isFrozen) return;

        if (other.gameObject.TryGetComponent(out BallBase b))
        {
            if (b.level == this.level && !b.isFrozen && !b.isDestroyed)
            {
                if (this.serial < b.serial)
                {
                    EventManager.OnBallMerged.Trigger(this.level);
                    
                    var center = (this.transform.position + other.transform.position) / 2;
                    var rotation = Quaternion.Lerp(this.transform.rotation, other.transform.rotation, 0.5f);
                    MergeManager.Instance.SpawnBallFromLevel(level + 1, center, rotation);

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
        ParticleManager.Instance.MergePowerParticle(this.transform.position, MyColors.GetBallColor(level-1));
        
        var i = Random.Range(0, 5);
        SeManager.Instance.PlaySe("ball" + i);
    }
}
