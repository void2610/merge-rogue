using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [SerializeField]
    private Vector3 inventoryPosition = new Vector3(0, -4, 0);
    [SerializeField]
    private List<GameObject> inventory = new List<GameObject>();
    [SerializeField]
    private BallDataList allBallDataList;
    [SerializeField]
    private GameObject ballBasePrefab;
    private int inventorySize = 6;
    private List<float> sizes = new List<float> { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
    private List<float> probabilities = new List<float> { 1f, 0.8f, 0.1f, 0.05f, 0.0f, 0.0f };


    public List<GameObject> GetInventory()
    {
        return inventory;
    }

    // マージ時に次のボールを生成
    public GameObject GetBallByLevel(int level)
    {
        if (level > 0 && level <= inventorySize)
        {
            var ball = CopyBall(inventory[level - 1]);
            ball.GetComponent<BallBase>().Unfreeze();
            return ball;
        }
        return null;
    }

    // 落とすボールを生成してMergeManagerに渡す
    public GameObject GetRandomBall()
    {
        GameObject ball;
        float total = probabilities.Sum();
        float r = GameManager.instance.RandomRange(0.0f, total);
        for (int i = 0; i < inventorySize; i++)
        {
            if (r < probabilities[i])
            {
                ball = CopyBall(inventory[i]);
                ball.GetComponent<BallBase>().Freeze();
                return ball;
            }
            r -= probabilities[i];
        }
        ball = CopyBall(inventory[0]);
        ball.GetComponent<BallBase>().Freeze();
        return ball;
    }

    // ボールのコピー元となるオブジェクトを生成、ステータス変化はこのオブジェクトに対して行う
    private GameObject CreateBallInstanceFromBallData(BallData data, int level)
    {
        GameObject ball = Instantiate(ballBasePrefab, this.transform);
        ball.name = $"{data.name} (Level {level})";
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
        ball.transform.localScale = Vector3.one * sizes[level - 1];
        ball.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(GameManager.instance.RandomRange(0.0f, 1.0f), GameManager.instance.RandomRange(0.0f, 1.0f), 1.0f);
        ball.GetComponent<BallBase>().level = level;
        ball.GetComponent<BallBase>().Freeze();
        return ball;
    }

    private GameObject CopyBall(GameObject ball)
    {
        GameObject newBall = Instantiate(ball);
        newBall.transform.localScale = ball.transform.localScale;
        newBall.GetComponent<SpriteRenderer>().color = ball.GetComponent<SpriteRenderer>().color;
        return newBall;
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

        // 全てnormalBallで初期化
        for (int i = 0; i < inventorySize; i++)
        {
            var bd = allBallDataList.GetNormalBallData();
            var ball = CreateBallInstanceFromBallData(bd, i + 1);
            ball.transform.position = inventoryPosition + new Vector3(i * 1f, 0, 0);
            inventory.Add(ball);
        }
        GameManager.instance.GetComponent<InventoryUI>().SetItem(inventory);
        GameManager.instance.GetComponent<InventoryUI>().SetCursor(0);
    }
}
