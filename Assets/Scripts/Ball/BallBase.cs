using UnityEngine;
using DG.Tweening;

public class BallBase : MonoBehaviour
{
    public enum BallRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    private static int ballSerial;

    public int level = 1;
    public float size = 1;
    public float attack = 1;
    public Color color = Color.white;
    public int serial { get; private set; }
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
        // Effect
    }

    protected virtual void Awake()
    {
        serial = ballSerial++;
        GetComponent<SpriteRenderer>().color = color;
        transform.localScale = new Vector3(size, size, size);
    }

    private void Start()
    {
        var t = transform.localScale.x;
        transform.localScale = Vector3.zero;
        transform.DOScale(t, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
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
                    var center = (this.transform.position + other.transform.position) / 2;
                    var rotation = Quaternion.Lerp(this.transform.rotation, other.transform.rotation, 0.5f);
                    MergeManager.Instance.SpawnBall(level + 1, center, rotation);

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
        });
    }
    
    protected void DefaultMergeParticle()
    {
        ParticleManager.Instance.MergeParticle(this.transform.position);
        ParticleManager.Instance.MergePowerParticle(this.transform.position, MyColors.GetBallColor(level-1));
        
        var i = Random.Range(0, 5);
        SeManager.Instance.PlaySe("ball" + i);
    }
}
