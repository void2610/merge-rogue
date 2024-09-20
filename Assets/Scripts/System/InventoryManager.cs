using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [SerializeField]
    private int inventorySize = 6;
    [SerializeField]
    private List<float> probabilities = new List<float> { 1f, 0.8f, 0.6f, 0.4f, 0.2f, 0.0f };
    [SerializeField]
    private List<float> sizes = new List<float> { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
    [SerializeField]
    private List<Color> colors = new List<Color> { Color.white, Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
    [SerializeField]
    private List<BallData> inventory = new List<BallData>();
    [SerializeField]
    private BallDataList allBallDataList;
    [SerializeField]
    private GameObject ballBasePrefab;

    public List<BallData> GetInventory()
    {
        return inventory;
    }

    public GameObject GetRandomBall()
    {
        float total = probabilities.Sum();
        float r = GameManager.instance.RandomRange(0.0f, total);
        for (int i = 0; i < inventorySize; i++)
        {
            if (r < probabilities[i])
            {
                return CreateBallInstanceFromBallData(inventory[i], i + 1);
            }
            r -= probabilities[i];
        }
        return CreateBallInstanceFromBallData(inventory[0], 1);
    }

    public GameObject GetBallByLevel(int level)
    {
        if (level > 0 && level <= inventorySize)
        {
            return CreateBallInstanceFromBallData(inventory[level - 1], level);
        }
        return null;
    }

    private GameObject CreateBallInstanceFromBallData(BallData data, int level)
    {
        GameObject ball = Instantiate(ballBasePrefab);
        if (!string.IsNullOrEmpty(data.className))
        {
            System.Type type = System.Type.GetType(data.className);
            if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                ball.AddComponent(type);
            }
            else
            {
                Debug.LogError("指定されたクラスは存在しないか、MonoBehaviourではありません: " + data.className);
                return null;
            }
        }
        else
        {
            Debug.LogError("behaviourClassNameが指定されていません。");
            return null;
        }

        ball.GetComponent<BallBase>().level = level;
        ball.GetComponent<SpriteRenderer>().sprite = data.sprite;
        ball.GetComponent<CircleCollider2D>().radius = data.size * sizes[level - 1];
        ball.GetComponent<BallBase>().color = colors[level - 1];

        return ball;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        Debug.Log(allBallDataList.GetNormalBallData());

        for (int i = 0; i < inventorySize; i++)
        {
            inventory.Add(allBallDataList.GetNormalBallData());
        }
    }

    private void Start()
    {

    }
}
