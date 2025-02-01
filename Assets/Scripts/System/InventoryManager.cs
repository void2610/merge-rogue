using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [SerializeField] public BallDataList allBallDataList;
    [SerializeField] private BallData normalBallData;
    [SerializeField] private GameObject ballBasePrefab;
    [SerializeField] private Vector3 inventoryPosition = new(5.5f, -1.0f, 0);
    [SerializeField] private List<BallData> testBalls;

    private const int MAX_INVENTORY_SIZE = 8;
    private const int MIN_INVENTORY_SIZE = 1;
    private const int FIRST_INVENTORY_SIZE = 4;
    public int InventorySize { get; private set; } = FIRST_INVENTORY_SIZE;
    public InventoryUI InventoryUI => this.GetComponent<InventoryUI>();
    private readonly List<GameObject> _inventory = new();
    public readonly List<float> Sizes = new() { 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f , 1.1f};
    public readonly List<float> Probabilities = new() { 1f, 0.8f, 0.1f, 0.05f, 0.0f, 0.0f, 0.0f, 0.0f };

    // ボールを任意の位置に追加する
    public void SetBall(BallData data, int level)
    {
        if (level is <= 0 or > MAX_INVENTORY_SIZE) return;
        
        var old = _inventory[level - 1];
        var newBall = CreateBallInstanceFromBallData(data, level);
        
        _inventory[level - 1] = newBall;
        InventoryUI.CreateBallUI(newBall, level - 1, newBall.GetComponent<BallBase>());
        if(old) Destroy(old);
    }
    
    // ボールを最後尾に追加する
    public void AddBall(BallData data)
    {
        if (InventorySize >= MAX_INVENTORY_SIZE) return;
        var ball = CreateBallInstanceFromBallData(data, InventorySize + 1);
        _inventory[InventorySize] = ball;
        InventorySize++;
        InventoryUI.CreateBallUI(ball, InventorySize - 1, ball.GetComponent<BallBase>());
    }
    
    // 2つのボールを入れ替える
    public void SwapBall(int index1, int index2)
    {
        if (index1 < 0 || index1 >= InventorySize || index2 < 0 || index2 >= InventorySize) return;
       
        var data1 = _inventory[index1].GetComponent<BallBase>().Data;
        var data2 = _inventory[index2].GetComponent<BallBase>().Data;
        
        Destroy(_inventory[index1]);
        Destroy(_inventory[index2]);
        
        _inventory[index1] = CreateBallInstanceFromBallData(data2, index1 + 1);
        _inventory[index2] = CreateBallInstanceFromBallData(data1, index2 + 1);
        
        InventoryUI.CreateBallUI(_inventory[index1], index1, _inventory[index1].GetComponent<BallBase>());
        InventoryUI.CreateBallUI(_inventory[index2], index2, _inventory[index2].GetComponent<BallBase>());
    }

    // 任意の場所のボールを削除し、後ろのボールを前に詰める
    public void RemoveAndShiftBall(int index)
    {
        Destroy(_inventory[index]);
        InventoryUI.RemoveBallUI(index);
        
        for (var i = index; i < InventorySize - 1; i++)
        {
            var data = _inventory[i + 1].GetComponent<BallBase>().Data;
            Destroy(_inventory[i + 1]);
            _inventory[i] = CreateBallInstanceFromBallData(data, i + 1);
            InventoryUI.CreateBallUI(_inventory[i], i, _inventory[i].GetComponent<BallBase>());
        }
        InventorySize--;
    }
    
    public GameObject GetBombBall()
    {
        var bd = allBallDataList.GetBallDataFromClassName("BombBall");
        var ball = CreateBallInstanceFromBallData(bd, 3);
        ball.GetComponent<BallBase>().Unfreeze();
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        return ball;
    }

    // マージ時に次のボールを生成
    public GameObject GetBallByLevel(int level)
    {
        if (level < MIN_INVENTORY_SIZE || level > InventorySize)  
        {
            return null;
        }
        var ball = CopyBall(_inventory[level - 1]);
        ball.GetComponent<BallBase>().Unfreeze();
        return ball;
    }

    // 落とすボールを生成してMergeManagerに渡す
    public GameObject GetRandomBall(Vector3 position = default)
    {
        GameObject ball;
        var total = Probabilities.Sum();
        var r = GameManager.Instance.RandomRange(0.0f, total);
        for (var i = 0; i < InventorySize; i++)
        {
            if (r < Probabilities[i])
            {
                ball = CopyBall(_inventory[i], position);
                ball.GetComponent<BallBase>().Freeze();
                return ball;
            }
            r -= Probabilities[i];
        }
        
        // みつからなかった場合は一番最初のボールを返す
        ball = CopyBall(_inventory[0], position);
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

        ballBase.InitBall(data, level);
        
        ball.transform.localScale = Vector3.one * (Sizes[level - 1] * ballBase.Size);
        // HDRカラーに変換
        var color = MyColors.GetBallColor(level - 1) * 1.05f;
        ball.GetComponent<SpriteRenderer>().color = color;
        ball.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = data.sprite;
        ball.GetComponent<BallBase>().Freeze();
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        ball.transform.position = CalcInventoryPosition(level - 1);
        return ball;
    }

    private GameObject CopyBall(GameObject ball, Vector3 position = default)
    {
        var newBall = Instantiate(ball, position, Quaternion.identity);
        var level = ball.GetComponent<BallBase>().Level;
        var data = ball.GetComponent<BallBase>().Data;
        newBall.GetComponent<BallBase>().InitBall(data, level);
        newBall.transform.localScale = ball.transform.localScale;
        newBall.GetComponent<SpriteRenderer>().color = ball.GetComponent<SpriteRenderer>().color;
        return newBall;
    }
    
    private Vector3 CalcInventoryPosition(int index)
    {
        return inventoryPosition + new Vector3(index * (0.6f + Sizes[index] * 0.5f), 0, 0);
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

        for(var i = 0; i < MAX_INVENTORY_SIZE; i++) _inventory.Add(null);
        
        // 全てnormalBallで初期化
        for (var i = 0; i < FIRST_INVENTORY_SIZE; i++)
        {
            var ball = CreateBallInstanceFromBallData(normalBallData, i + 1);
            InventoryUI.CreateBallUI(ball, i, ball.GetComponent<BallBase>());
            _inventory[i] = ball;
        }
        
        // テスト用
        if(Application.isEditor)
        {
            for(var i = 0; i < MAX_INVENTORY_SIZE; i++)
            {
                if(testBalls[i]) SetBall(testBalls[i], i + 1);
            }
        }
        
        InventoryUI.SetCursor(0);
    }
}
