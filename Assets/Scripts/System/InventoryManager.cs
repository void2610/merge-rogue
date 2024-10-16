using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }


    [SerializeField]
    private BallDataList allBallDataList;
    [SerializeField]
    private GameObject ballBasePrefab;

    private const int INVENTORY_SIZE = 9;
    private readonly List<GameObject> inventory = new();
    private readonly Vector3 inventoryPosition = new(5.5f, -1.0f, 0);
    private readonly List<float> sizes = new() { 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.1f, 1.2f };
    private readonly List<float> probabilities = new() { 1f, 0.8f, 0.1f, 0.05f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    private readonly List<Color> colors = new() { new Color(0.7f,0.7f,0.7f), Color.green, Color.blue,  Color.magenta, Color.yellow, Color.red, Color.cyan, Color.black, Color.white };

    // ボールを入れ替える
    public void SetBall(BallData data, int level)
    {
        if (level is <= 0 or > INVENTORY_SIZE) return;
        var old = inventory[level - 1];
        var ball = CreateBallInstanceFromBallData(data, level);
        ball.transform.position = inventoryPosition + new Vector3(level - 1, 0, 0);
        inventory[level - 1] = ball;
        GameManager.Instance.GetComponent<InventoryUI>().SetItem(inventory);
        Destroy(old);
    }

    public List<GameObject> GetInventory()
    {
        return inventory;
    }

    // マージ時に次のボールを生成
    public GameObject GetBallByLevel(int level)
    {
        if (level is <= 0 or > INVENTORY_SIZE)
        {
            Debug.LogError("指定されたレベルのボールは存在しません。");
            return null;
        }
        var ball = CopyBall(inventory[level - 1]);
        ball.GetComponent<BallBase>().Unfreeze();
        return ball;
    }

    // 落とすボールを生成してMergeManagerに渡す
    public GameObject GetRandomBall()
    {
        GameObject ball;
        float total = probabilities.Sum();
        float r = GameManager.Instance.RandomRange(0.0f, total);
        for (int i = 0; i < INVENTORY_SIZE; i++)
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
        BallBase ballBase;
        if (!string.IsNullOrEmpty(data.className))
        {
            System.Type type = System.Type.GetType(data.className);
            if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                ball.AddComponent(type);
                ballBase = ball.GetComponent<BallBase>();
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
        ball.transform.localScale = Vector3.one * sizes[level - 1] * ballBase.size;
        ball.GetComponent<SpriteRenderer>().color = colors[level - 1];
        ball.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = data.sprite;
        ball.GetComponent<BallBase>().level = level;
        ball.GetComponent<BallBase>().Freeze();
        return ball;
    }

    private GameObject CopyBall(GameObject ball)
    {
        GameObject newBall = Instantiate(ball);
        newBall.transform.localScale = ball.transform.localScale;
        newBall.GetComponent<SpriteRenderer>().color = ball.GetComponent<SpriteRenderer>().color;
        Destroy(newBall.GetComponent<EventTrigger>());
        return newBall;
    }

    private void SetEvent(GameObject ball, int index)
    {
        ball.AddComponent<EventTrigger>().triggers = new List<EventTrigger.Entry>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };

        entry.callback.AddListener(_ => { GameManager.Instance.GetComponent<InventoryUI>().SetCursor(index); });
        ball.GetComponent<EventTrigger>().triggers.Add(entry);
        entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener(_ => { Shop.Instance.BuyBall(index); });
        ball.GetComponent<EventTrigger>().triggers.Add(entry);
        inventory.Add(ball);
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
        var bd = allBallDataList.GetNormalBallData();
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            float angle = -i * Mathf.PI * 2 / INVENTORY_SIZE;
            float x = Mathf.Cos(angle) * 2;
            float y = Mathf.Sin(angle) * 2;
            var ball = CreateBallInstanceFromBallData(bd, i + 1);
            ball.transform.position = inventoryPosition + new Vector3(x, y, 0);
            SetEvent(ball, i);
        }
        GameManager.Instance.GetComponent<InventoryUI>().SetItem(inventory);
        GameManager.Instance.GetComponent<InventoryUI>().SetCursor(0);
    }
}
