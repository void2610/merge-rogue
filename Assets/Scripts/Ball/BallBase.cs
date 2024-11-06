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

    public string ballName = "ふつうのボール";
    public string description = "ふつう";
    public BallRarity rarity = BallRarity.Common;
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


    protected virtual void Effect()
    {
        // Effect
        var r = new Vector3(Random.Range(-0.75f, 0.75f), Random.Range(-0.75f, 0.75f), 0);
        MergeManager.Instance.AddAttackCount(attack * level, this.transform.position + r);
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
                    Effect();
                    b.Effect();

                    isDestroyed = true;
                    b.isDestroyed = true;
                    Vector3 center = (this.transform.position + other.transform.position) / 2;
                    Quaternion rotation = Quaternion.Lerp(this.transform.rotation, other.transform.rotation, 0.5f);
                    MergeManager.Instance.SpawnBall(level + 1, center, rotation);

                    DestroyBall();
                    b.DestroyBall();
                }
            }
        }
    }
    
    public void DestroyBall()
    {
        transform.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
