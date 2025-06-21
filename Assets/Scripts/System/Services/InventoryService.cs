using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

/// <summary>
/// インベントリ管理の完全なサービス実装
/// InventoryManagerを置き換える純粋なサービスクラス
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly InventoryConfiguration _config;
    private readonly IContentService _contentService;
    private readonly IRandomService _randomService;
    private readonly List<GameObject> _inventory;

    public int InventorySize { get; private set; }
    public bool IsFull => InventorySize >= _config.MaxInventorySize;
    private readonly InventoryUI _inventoryUI;
    public List<float> Sizes => _config.Sizes.ToList();
    
    public InventoryService(InventoryConfiguration config, IContentService contentService, IRandomService randomService, InventoryUI inventoryUI)
    {
        _config = config;
        _contentService = contentService;
        _randomService = randomService;
        _inventoryUI = inventoryUI;
        
        InventorySize = _config.FirstInventorySize;

        _inventory = new List<GameObject>(InventorySize);
        for (var i = 0; i < _config.MaxInventorySize; i++) _inventory.Add(null);
        
        Initialize();
    }
    
    
    /// <summary>
    /// インベントリの初期化
    /// </summary>
    private void Initialize()
    {
        // 全てnormalBallで初期化
        for (var i = 0; i < _config.FirstInventorySize; i++)
        {
            var ball = CreateBallInstanceFromBallData(_config.NormalBallData, i + 1);
            _inventoryUI?.CreateBallUI(ball, i, ball.GetComponent<BallBase>());
            _inventory[i] = ball;
        }
        
        // テスト用（エディタのみ）
        if (Application.isEditor && _config.TestBalls != null)
        {
            for (var i = 0; i < _config.MaxInventorySize && i < _config.TestBalls.Count; i++)
            {
                if (_config.TestBalls[i] != null)
                {
                    SetBall(_config.TestBalls[i], i + 1);
                }
            }
        }
    }
    
    /// <summary>
    /// 指定位置のボールをアップグレード
    /// </summary>
    /// <param name="index">インベントリインデックス</param>
    public void UpgradeBall(int index)
    {
        if (index < 0 || index >= InventorySize) 
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
            
        var ballBase = _inventory[index].GetComponent<BallBase>();
        ballBase.Upgrade();
        var tmp = _inventory[index];
        _inventory[index] = CreateBallInstanceFromBallData(ballBase.Data, ballBase.Rank, ballBase.Level);
        _inventoryUI?.CreateBallUI(_inventory[index], index, ballBase);
        Object.Destroy(tmp);
    }
    
    /// <summary>
    /// ボールを最後尾に追加
    /// </summary>
    /// <param name="data">追加するボールデータ</param>
    public void AddBall(BallData data)
    {
        if (IsFull) 
            throw new InvalidOperationException("Inventory is full.");
            
        var ball = CreateBallInstanceFromBallData(data, InventorySize + 1);
        _inventory[InventorySize] = ball;
        InventorySize++;
        _inventoryUI?.CreateBallUI(ball, InventorySize - 1, ball.GetComponent<BallBase>());
    }
    
    /// <summary>
    /// 任意の位置のボールを新しいものに置き換え
    /// </summary>
    /// <param name="data">新しいボールデータ</param>
    /// <param name="rank">置き換え位置（1ベース）</param>
    public void ReplaceBall(BallData data, int rank)
    {
        var index = rank - 1;
        if (index < 0 || index >= InventorySize) 
            throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 1 and InventorySize.");
        
        var old = _inventory[index];
        var newBall = CreateBallInstanceFromBallData(data, rank);
        
        _inventory[index] = newBall;
        _inventoryUI?.CreateBallUI(newBall, index, newBall.GetComponent<BallBase>());
        if (old) Object.Destroy(old);
    }
    
    /// <summary>
    /// 2つのボールを入れ替え
    /// </summary>
    /// <param name="index1">位置1</param>
    /// <param name="index2">位置2</param>
    public async UniTask SwapBall(int index1, int index2)
    {
        if (index1 < 0 || index1 >= InventorySize || index2 < 0 || index2 >= InventorySize) 
            throw new ArgumentOutOfRangeException(nameof(index1), "Index out of range");
       
        var data1 = _inventory[index1].GetComponent<BallBase>().Data;
        var level1 = _inventory[index1].GetComponent<BallBase>().Level;
        var data2 = _inventory[index2].GetComponent<BallBase>().Data;
        var level2 = _inventory[index2].GetComponent<BallBase>().Level;
        
        Object.Destroy(_inventory[index1]);
        Object.Destroy(_inventory[index2]);
        
        _inventory[index1] = CreateBallInstanceFromBallData(data2, index1 + 1, level2);
        _inventory[index2] = CreateBallInstanceFromBallData(data1, index2 + 1, level1);
        
        if (_inventoryUI != null)
        {
            _inventoryUI.CreateBallUITween(_inventory[index1], index2, index1, _inventory[index1].GetComponent<BallBase>()).Forget();
            await _inventoryUI.CreateBallUITween(_inventory[index2], index1, index2, _inventory[index2].GetComponent<BallBase>());
        }
        
        await UniTask.Delay(1000);
    }
    
    /// <summary>
    /// 任意の場所のボールを削除し、後ろのボールを前に詰める
    /// </summary>
    /// <param name="index">削除位置</param>
    public void RemoveAndShiftBall(int index)
    {
        if (index < 0 || index >= InventorySize) 
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
            
        Object.Destroy(_inventory[index]);
        _inventoryUI?.RemoveBallUI(index);
        
        for (var i = index; i < InventorySize - 1; i++)
        {
            var data = _inventory[i + 1].GetComponent<BallBase>().Data;
            var level = _inventory[i + 1].GetComponent<BallBase>().Level;
            Object.Destroy(_inventory[i + 1]);
            _inventory[i] = CreateBallInstanceFromBallData(data, i + 1, level);
            _inventoryUI?.CreateBallUI(_inventory[i], i, _inventory[i].GetComponent<BallBase>());
        }
        InventorySize--;
    }
    
    /// <summary>
    /// ボールタイプを指定して取得
    /// </summary>
    /// <param name="ballClassName">ボールクラス名</param>
    /// <param name="rank">ランク</param>
    /// <returns>生成されたボール</returns>
    public GameObject GetSpecialBallByClassName(string ballClassName, int rank)
    {
        BallData bd = _contentService?.GetBallDataFromClassName(ballClassName) ?? 
                     _config.AllBallDataList?.GetBallDataFromClassName(ballClassName);
        
        if (bd == null)
        {
            Debug.LogError($"BallData not found for className: {ballClassName}");
            return null;
        }
        
        var ball = CreateBallInstanceFromBallData(bd, rank);
        ball.GetComponent<BallBase>().Unfreeze();
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        return ball;
    }
    
    /// <summary>
    /// マージ時に次のボールを生成
    /// </summary>
    /// <param name="rank">ランク</param>
    /// <returns>生成されたボール</returns>
    public GameObject GetBallByRank(int rank)
    {
        if (rank < _config.MinInventorySize || rank > InventorySize)  
        {
            return null;
        }
        var ball = CopyBall(_inventory[rank - 1]);
        ball.GetComponent<BallBase>().Unfreeze();
        return ball;
    }
    
    /// <summary>
    /// 落とすボールを生成してMergeManagerに渡す
    /// </summary>
    /// <param name="position">生成位置</param>
    /// <returns>生成されたボール</returns>
    public GameObject GetRandomBall(Vector3 position = default)
    {
        GameObject ball;
        var total = _config.Probabilities.Take(InventorySize).Sum();
        var r = _randomService.RandomRange(0.0f, total);
        
        for (var i = 0; i < InventorySize; i++)
        {
            if (r < _config.Probabilities[i])
            {
                ball = CopyBall(_inventory[i], position);
                ball.GetComponent<BallBase>().Freeze();
                return ball;
            }
            r -= _config.Probabilities[i];
        }
        
        // 見つからなかった場合は一番最初のボールを返す
        ball = CopyBall(_inventory[0], position);
        ball.GetComponent<BallBase>().Freeze();
        return ball;
    }
    
    /// <summary>
    /// 指定位置のボールデータを取得
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <returns>ボールデータ</returns>
    public BallData GetBallData(int index)
    {
        if (index < 0 || index >= InventorySize) 
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
        return _inventory[index].GetComponent<BallBase>().Data;
    }
    
    /// <summary>
    /// 指定位置のボールレベルを取得
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <returns>ボールレベル</returns>
    public int GetBallLevel(int index)
    {
        if (index < 0 || index >= InventorySize) 
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
        return _inventory[index].GetComponent<BallBase>().Level;
    }
    
    /// <summary>
    /// ボールを任意の位置に設定（内部メソッド）
    /// </summary>
    /// <param name="data">ボールデータ</param>
    /// <param name="rank">ランク（1ベース）</param>
    private void SetBall(BallData data, int rank)
    {
        if (rank <= 0 || rank > _config.MaxInventorySize) 
            throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 1 and MAX_INVENTORY_SIZE.");
        
        var index = rank - 1;
        var old = _inventory[index];
        var newBall = CreateBallInstanceFromBallData(data, rank);
        
        _inventory[index] = newBall;
        _inventoryUI?.CreateBallUI(newBall, index, newBall.GetComponent<BallBase>());
        if (old) Object.Destroy(old);
    }
    
    /// <summary>
    /// ボールのコピー元となるオブジェクトを生成
    /// </summary>
    /// <param name="data">ボールデータ</param>
    /// <param name="rank">ランク</param>
    /// <param name="level">レベル</param>
    /// <returns>生成されたボール</returns>
    private GameObject CreateBallInstanceFromBallData(BallData data, int rank, int level = 0)
    {
        if (!data)
        {
            Debug.LogError("BallData is null in CreateBallInstanceFromBallData");
            return null;
        }
        
        var ball = Object.Instantiate(_config.BallBasePrefab);
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

        // BallBaseへのサービス注入
        ballBase.InjectDependencies(_randomService);
        
        ballBase.InitBall(data, rank, level);
        
        ball.transform.localScale = Vector3.one * (_config.Sizes[rank - 1] * ballBase.Size * 0.8f);
        ball.GetComponent<Rigidbody2D>().mass = ballBase.Weight * _config.Weights[rank - 1];
        ball.GetComponent<BallBase>().Freeze();
        ball.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        ball.transform.position = _config.CalcInventoryPosition(rank - 1);
        
        return ball;
    }
    
    /// <summary>
    /// ボールのコピーを作成
    /// </summary>
    /// <param name="ball">コピー元ボール</param>
    /// <param name="position">配置位置</param>
    /// <returns>コピーされたボール</returns>
    private GameObject CopyBall(GameObject ball, Vector3 position = default)
    {
        var newBall = Object.Instantiate(ball, position, Quaternion.identity);
        var rank = ball.GetComponent<BallBase>().Rank;

        var data = ball.GetComponent<BallBase>().Data;
        var level = ball.GetComponent<BallBase>().Level;
        var newBallBase = newBall.GetComponent<BallBase>();
        
        // BallBaseへのサービス注入
        newBallBase.InjectDependencies(_randomService);
        
        newBallBase.InitBall(data, rank, level);
        newBall.transform.localScale = ball.transform.localScale;
        newBall.GetComponent<SpriteRenderer>().color = ball.GetComponent<SpriteRenderer>().color;
        return newBall;
    }
    
    // UI操作のラッパーメソッド
    
    /// <summary>
    /// スワップ編集モードを開始
    /// </summary>
    public void StartEditSwap()
    {
        _inventoryUI.StartEdit(InventoryUI.InventoryUIState.Swap);
    }
    
    /// <summary>
    /// アップグレード編集モードを開始
    /// </summary>
    public void StartEditUpgrade()
    {
        _inventoryUI.StartEdit(InventoryUI.InventoryUIState.Upgrade);
    }
    
    /// <summary>
    /// 置換編集モードを開始
    /// </summary>
    /// <param name="ballData">置換するボールデータ</param>
    public void StartEditReplace(BallData ballData)
    {
        _inventoryUI.StartEditReplace(ballData);
    }
    
    /// <summary>
    /// 削除編集モードを開始
    /// </summary>
    public void StartEditRemove()
    {
        _inventoryUI.StartEdit(InventoryUI.InventoryUIState.Remove);
    }
    
    /// <summary>
    /// 編集モードをキャンセル
    /// </summary>
    public void CancelEdit()
    {
        _inventoryUI.CancelEdit();
    }
}