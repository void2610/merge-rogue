using UnityEngine;

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
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        isFrozen = true;
    }

    public void Unfreeze()
    {
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        isFrozen = false;
    }


    protected virtual void Effect()
    {
        // Effect
        MergeManager.Instance.AddAttackCount(attack * level);
    }

    protected virtual void Awake()
    {
        serial = ballSerial++;
        GetComponent<SpriteRenderer>().color = color;
        transform.localScale = new Vector3(size, size, size);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isDestroyed || isFrozen) return;

        if (other.gameObject.TryGetComponent(out BallBase b))
        {
            if (b.level == this.level)
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

                    Destroy(gameObject);
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
