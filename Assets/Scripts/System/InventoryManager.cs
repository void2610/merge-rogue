using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [SerializeField] public BallDataList allBallDataList;
    [SerializeField] private BallData normalBallData;
    [SerializeField] private GameObject ballBasePrefab;
    [SerializeField] private Vector3 inventoryPosition = new(5.5f, -1.0f, 0);

    public const int INVENTORY_SIZE = 7;
    public InventoryUI inventoryUI => this.GetComponent<InventoryUI>();
    private readonly List<GameObject> inventory = new();
    public readonly List<float> sizes = new() { 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
    private readonly List<float> probabilities = new() { 1f, 0.8f, 0.1f, 0.05f, 0.0f, 0.0f, 0.0f };

    // ボールを入れ替える
    public void SetBall(BallData data, int level)
    {
        if (level is <= 0 or > INVENTORY_SIZE) return;
        var old = inventory[level - 1];
        var ball = CreateBallInstanceFromBallData(data, level);
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        ball.transform.position = CalcInventoryPosition(level - 1);
        inventory[level - 1] = ball;
        inventoryUI.CreateBallUI(ball, level - 1);
        if(old) Destroy(old);
    }

    public List<GameObject> GetInventory()
    {
        return inventory;
    }
    
    public GameObject GetBombBall()
    {
        var bd = allBallDataList.GetBallDataFromClassName("BombBall");
        var ball = CreateBallInstanceFromBallData(bd, 3);
        ball.GetComponent<BallBase>().Unfreeze();
        return ball;
    }

    // マージ時に次のボールを生成
    public GameObject GetBallByLevel(int level)
    {
        if (level is <= 0 or > INVENTORY_SIZE)
        {
            // Debug.LogError("指定されたレベルのボールは存在しません。");
            return null;
        }
        var ball = CopyBall(inventory[level - 1]);
        ball.GetComponent<BallBase>().Unfreeze();
        return ball;
    }

    // 落とすボールを生成してMergeManagerに渡す
    public GameObject GetRandomBall(Vector3 position = default)
    {
        GameObject ball;
        float total = probabilities.Sum();
        float r = GameManager.Instance.RandomRange(0.0f, total);
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (r < probabilities[i])
            {
                ball = CopyBall(inventory[i], position);
                ball.GetComponent<BallBase>().Freeze();
                return ball;
            }
            r -= probabilities[i];
        }
        
        // みつからなかった場合は一番最初のボールを返す
        ball = CopyBall(inventory[0], position);
        ball.GetComponent<BallBase>().Freeze();
        return ball;
    }

    // ボールのコピー元となるオブジェクトを生成、ステータス変化はこのオブジェクトに対して行う
    private GameObject CreateBallInstanceFromBallData(BallData data, int level)
    {
        var ball = Instantiate(ballBasePrefab, this.transform);
        ball.name = $"{data.name} (Level {level})";
        BallBase ballBase;
        if (!string.IsNullOrEmpty(data.className))
        {
            Type type = Type.GetType(data.className);
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
        ball.transform.localScale = Vector3.one * (sizes[level - 1] * ballBase.size);
        // HDRカラーに変換
        var color = MyColors.GetBallColor(level - 1) * 1.05f;
        ball.GetComponent<SpriteRenderer>().color = color;
        ball.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = data.sprite;
        ball.GetComponent<BallBase>().level = level;
        ball.GetComponent<BallBase>().Freeze();
        return ball;
    }

    private GameObject CopyBall(GameObject ball, Vector3 position = default)
    {
        var newBall = Instantiate(ball, position, Quaternion.identity);
        newBall.transform.localScale = ball.transform.localScale;
        newBall.GetComponent<SpriteRenderer>().color = ball.GetComponent<SpriteRenderer>().color;
        Destroy(newBall.GetComponent<EventTrigger>());
        return newBall;
    }
    
    private Vector3 CalcInventoryPosition(int index)
    {
        return inventoryPosition + new Vector3(index * (0.6f + sizes[index] * 0.5f), 0, 0);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        allBallDataList.Register();

        // 全てnormalBallで初期化
        for (var i = 0; i < INVENTORY_SIZE; i++)
        {
            var ball = CreateBallInstanceFromBallData(normalBallData, i + 1);
            ball.transform.position = CalcInventoryPosition(i);
            ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            inventoryUI.CreateBallUI(ball, i);
            inventory.Add(ball);
        }
        
        inventoryUI.SetCursor(0);
    }
}
