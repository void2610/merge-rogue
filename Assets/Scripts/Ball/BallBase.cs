using UnityEngine;
using System.Collections.Generic;

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

    //TODO UI表示クラスにおいてもいいかも？
    private static readonly Dictionary<BallRarity, Color> RarityColors = new Dictionary<BallRarity, Color>
    {
        { BallRarity.Common, Color.gray },
        { BallRarity.Uncommon, Color.green },
        { BallRarity.Rare, Color.blue },
        { BallRarity.Epic, new Color(0.5f, 0f, 0.5f) },
        { BallRarity.Legendary, new Color(1f, 0.5f, 0f) }
    };

    private static int _ballSerial = 0;

    public string ballName = "ふつうのボール";
    public string description = "ふつう";
    public BallRarity rarity = BallRarity.Common;
    public int level = 1;
    public float size = 1;
    public int attack = 1;
    public Color color = Color.white;
    public int Serial { get; private set; }
    public bool isDestroyed = false;
    private bool isFrozen = false;
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
        MergeManager.Instance.Attack(attack);
    }

    protected virtual void Awake()
    {
        Serial = _ballSerial++;
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
                if (this.Serial < b.Serial)
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
