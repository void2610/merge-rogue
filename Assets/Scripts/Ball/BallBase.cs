using UnityEngine;

public class BallBase : MonoBehaviour
{
    [SerializeField]
    private GameObject mergeParticle;
    private static int ball_serial = 0;


    public int level = 1;
    public float size = 1;
    public int attack = 1;
    public float probability = 0.1f;
    public Color color = Color.white;
    public int serial { get; private set; }
    public bool isDestroyed = false;

    protected virtual void Effect()
    {
        // Effect
    }

    private void Awake()
    {
        serial = ball_serial++;
        GetComponent<SpriteRenderer>().color = color;
        transform.localScale = new Vector3(size, size, size);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isDestroyed) return;

        if (other.gameObject.TryGetComponent(out BallBase b))
        {
            if (b.level == this.level)
            {
                if (this.serial < b.serial)
                {
                    Effect();
                    Instantiate(mergeParticle, transform.position, Quaternion.identity);
                    isDestroyed = true;
                    b.isDestroyed = true;
                    Vector3 center = (this.transform.position + other.transform.position) / 2;
                    Quaternion rotation = Quaternion.Lerp(this.transform.rotation, other.transform.rotation, 0.5f);
                    MergeManager.instance.SpawnBall(level + 1, attack, center, rotation);

                    Destroy(gameObject);
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
