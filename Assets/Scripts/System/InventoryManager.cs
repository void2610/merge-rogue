using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [SerializeField] public BallDataList allBallDataList;
    [SerializeField] private BallData normalBallData;
    [SerializeField] private GameObject ballBasePrefab;
    [SerializeField] private Vector3 inventoryPosition = new(5.5f, -1.0f, 0);
    [SerializeField] private List<BallData> testBalls;

    private const int MAX_INVENTORY_SIZE = 10;
    private const int MIN_INVENTORY_SIZE = 1;
    private const int FIRST_INVENTORY_SIZE = 7;
    public int InventorySize { get; private set; } = FIRST_INVENTORY_SIZE;
    public InventoryUI InventoryUI => this.GetComponent<InventoryUI>();
    private readonly List<GameObject> _inventory = new();
    public readonly List<float> Sizes = new() { 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f , 1.1f};
    private readonly List<float> _probabilities = new() { 1f, 0.8f, 0.1f, 0.05f, 0.0f, 0.0f, 0.0f, 0.0f };
    
    public bool IsFull => InventorySize >= MAX_INVENTORY_SIZE;
    public bool IsOnlyOne => InventorySize == 1;
    
    public void UpgradeBall(int index)
    {
        if (index < 0 || index >= InventorySize) throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
        var ballBase = _inventory[index].GetComponent<BallBase>();
        ballBase.Upgrade();
        var tmp = _inventory[index];
        _inventory[index] = CreateBallInstanceFromBallData(ballBase.Data, ballBase.Rank, ballBase.Level);
        InventoryUI.CreateBallUI(_inventory[index], index, ballBase);
        Destroy(tmp);
    }

    // ボールを任意の位置に追加する
    private void SetBall(BallData data, int rank)
    {
        if (rank is <= 0 or > MAX_INVENTORY_SIZE) throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 1 and MAX_INVENTORY_SIZE.");
        
        var old = _inventory[rank - 1];
        var newBall = CreateBallInstanceFromBallData(data, rank);
        
        _inventory[rank - 1] = newBall;
        InventoryUI.CreateBallUI(newBall, rank - 1, newBall.GetComponent<BallBase>());
        if(old) Destroy(old);
    }
    
    // ボールを最後尾に追加する
    public void AddBall(BallData data)
    {
        if (InventorySize >= MAX_INVENTORY_SIZE) throw new InvalidOperationException("Inventory is full.");
        var ball = CreateBallInstanceFromBallData(data, InventorySize + 1);
        _inventory[InventorySize] = ball;
        InventorySize++;
        InventoryUI.CreateBallUI(ball, InventorySize - 1, ball.GetComponent<BallBase>());
    }

    // 任意の位置のボールを新しいものに置き換える
    public void ReplaceBall(BallData data, int rank)
    {
        if (rank < 0 || rank >= InventorySize) throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 0 and InventorySize.");
        
        var old = _inventory[rank];
        var newBall = CreateBallInstanceFromBallData(data, rank + 1);
        
        _inventory[rank] = newBall;
        InventoryUI.CreateBallUI(newBall, rank, newBall.GetComponent<BallBase>());
        Destroy(old);
    }
    
    // 2つのボールを入れ替える
    public async UniTask SwapBall(int index1, int index2)
    {
        if (index1 < 0 || index1 >= InventorySize || index2 < 0 || index2 >= InventorySize) throw new ArgumentOutOfRangeException("Index out of range");
       
        var data1 = _inventory[index1].GetComponent<BallBase>().Data;
        var level1 = _inventory[index1].GetComponent<BallBase>().Level;
        var data2 = _inventory[index2].GetComponent<BallBase>().Data;
        var level2 = _inventory[index2].GetComponent<BallBase>().Level;
        
        Destroy(_inventory[index1]);
        Destroy(_inventory[index2]);
        
        _inventory[index1] = CreateBallInstanceFromBallData(data2, index1 + 1, level2);
        _inventory[index2] = CreateBallInstanceFromBallData(data1, index2 + 1, level1);
        
        InventoryUI.CreateBallUITween(_inventory[index1], index2,index1, _inventory[index1].GetComponent<BallBase>()).Forget();
        await InventoryUI.CreateBallUITween(_inventory[index2], index1,index2, _inventory[index2].GetComponent<BallBase>());
        await UniTask.Delay(1000);
    }

    // 任意の場所のボールを削除し、後ろのボールを前に詰める
    public void RemoveAndShiftBall(int index)
    {
        Destroy(_inventory[index]);
        InventoryUI.RemoveBallUI(index);
        
        for (var i = index; i < InventorySize - 1; i++)
        {
            var data = _inventory[i + 1].GetComponent<BallBase>().Data;
            var level = _inventory[i + 1].GetComponent<BallBase>().Level;
            Destroy(_inventory[i + 1]);
            _inventory[i] = CreateBallInstanceFromBallData(data, i + 1, level);
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

    public GameObject GetDisturbBall()
    {
        var bd = allBallDataList.GetBallDataFromClassName("DisturbBall");
        var ball = CreateBallInstanceFromBallData(bd, 1);
        ball.GetComponent<BallBase>().Unfreeze();
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        return ball;
    }

    // マージ時に次のボールを生成
    public GameObject GetBallByRank(int rank)
    {
        if (rank < MIN_INVENTORY_SIZE || rank > InventorySize)  
        {
            return null;
        }
        var ball = CopyBall(_inventory[rank - 1]);
        ball.GetComponent<BallBase>().Unfreeze();
        return ball;
    }

    // 落とすボールを生成してMergeManagerに渡す
    public GameObject GetRandomBall(Vector3 position = default)
    {
        GameObject ball;
        var total = _probabilities.Sum();
        var r = GameManager.Instance.RandomRange(0.0f, total);
        for (var i = 0; i < InventorySize; i++)
        {
            if (r < _probabilities[i])
            {
                ball = CopyBall(_inventory[i], position);
                ball.GetComponent<BallBase>().Freeze();
                return ball;
            }
            r -= _probabilities[i];
        }
        
        // みつからなかった場合は一番最初のボールを返す
        ball = CopyBall(_inventory[0], position);
        ball.GetComponent<BallBase>().Freeze();
        return ball;
    }
    
    public BallData GetBallData(int index) => _inventory[index].GetComponent<BallBase>().Data;
    public int GetBallLevel(int index) => _inventory[index].GetComponent<BallBase>().Level;

    // ボールのコピー元となるオブジェクトを生成、ステータス変化はこのオブジェクトに対して行う
    private GameObject CreateBallInstanceFromBallData(BallData data, int rank, int level = 0)
    {
        var ball = Instantiate(ballBasePrefab, this.transform);
        ball.name = $"{data.name} (Rank{rank}, Level{level+1})";
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

        ballBase.InitBall(data, rank, level);
        
        ball.transform.localScale = Vector3.one * (Sizes[rank - 1] * ballBase.Size * 0.75f);
        ball.GetComponent<BallBase>().Freeze();
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        ball.transform.position = CalcInventoryPosition(rank - 1);
        return ball;
    }

    private GameObject CopyBall(GameObject ball, Vector3 position = default)
    {
        var newBall = Instantiate(ball, position, Quaternion.identity);
        var rank = ball.GetComponent<BallBase>().Rank;

        EventManager.OnBallCreate.Trigger(ball.GetComponent<BallBase>().Data);
        var data = EventManager.OnBallCreate.GetAndResetValue();
        
        var level = ball.GetComponent<BallBase>().Level;
        newBall.GetComponent<BallBase>().InitBall(data, rank, level);
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

        for(var i = 0; i < MAX_INVENTORY_SIZE; i++) _inventory.Add(null);
    }

    private void Start()
    {
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
        
    }
}
