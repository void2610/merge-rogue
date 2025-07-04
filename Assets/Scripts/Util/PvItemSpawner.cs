using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PvItemSpawner : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private List<Sprite> itemSprites = new List<Sprite>();
    [SerializeField] private GameObject itemPrefab; // SpriteRendererを持つプレハブ
    [SerializeField] private bool infiniteSpawn = true; // 無限に生成するか
    [SerializeField] private int spawnCount = 30; // 生成する数（infiniteSpawnがfalseの場合）
    [SerializeField] private float spawnInterval = 0.1f; // 生成間隔
    
    [Header("生成範囲")]
    [SerializeField] private float spawnRangeX = 10f; // X軸の生成範囲
    [SerializeField] private float spawnHeight = 10f; // 生成する高さ
    
    [Header("落下設定")]
    [SerializeField] private float fallSpeed = 5f; // 落下速度
    [SerializeField] private float rotationSpeed = 180f; // 回転速度（度/秒）
    [SerializeField] private float destroyHeight = -10f; // 削除する高さ
    
    [Header("ランダム設定")]
    [SerializeField] private Vector2 scaleRange = new Vector2(0.5f, 1.5f); // スケールの範囲
    [SerializeField] private Vector2 fallSpeedRange = new Vector2(3f, 7f); // 落下速度の範囲
    [SerializeField] private Vector2 rotationSpeedRange = new Vector2(90f, 270f); // 回転速度の範囲
    
    [Header("奥行き設定")]
    [SerializeField] private Vector2 depthRange = new Vector2(0f, 5f); // 奥行きの範囲（0が最前面）
    [SerializeField] private float depthScaleFactor = 0.8f; // 奥行き1あたりのスケール減少率
    [SerializeField] private float depthColorFactor = 0.7f; // 奥行き1あたりの色の暗さ
    
    [Header("追加のスケール設定")]
    [SerializeField] private Vector2 additionalScaleRange = new Vector2(0.8f, 1.2f); // 追加のランダムスケール範囲

    private int _currentSpriteIndex = 0; // 現在のスプライトインデックス

    void Start()
    {
        // プレハブが設定されていない場合、自動生成
        if (!itemPrefab)
        {
            CreateDefaultPrefab();
        }
        
        // アイテム生成を開始
        if (itemSprites.Count > 0)
        {
            StartCoroutine(SpawnItems());
        }
        else
        {
            Debug.LogWarning("PVItemSpawner: アイテムスプライトが設定されていません");
        }
    }

    /// <summary>
    /// デフォルトのプレハブを作成
    /// </summary>
    private void CreateDefaultPrefab()
    {
        itemPrefab = new GameObject("ItemPrefab");
        itemPrefab.AddComponent<SpriteRenderer>();
        itemPrefab.SetActive(false);
    }

    /// <summary>
    /// アイテムを定期的に生成
    /// </summary>
    private IEnumerator SpawnItems()
    {
        if (infiniteSpawn)
        {
            // 無限に生成
            while (true)
            {
                SpawnSingleItem();
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        else
        {
            // 指定回数だけ生成
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnSingleItem();
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    /// <summary>
    /// 単一のアイテムを生成
    /// </summary>
    private void SpawnSingleItem()
    {
        // ランダムな位置を計算
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0);
        
        // アイテムを生成
        GameObject item = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        item.SetActive(true);
        
        // SpriteRendererの設定
        SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
        if (spriteRenderer && itemSprites.Count > 0)
        {
            // 順番にスプライトを設定
            spriteRenderer.sprite = itemSprites[_currentSpriteIndex];
            
            // 次のインデックスに進む（ループ）
            _currentSpriteIndex = (_currentSpriteIndex + 1) % itemSprites.Count;
            
            // 奥行きをランダムに設定
            float depth = Random.Range(depthRange.x, depthRange.y);
            
            // 奥行きに基づいてスケールを調整
            float randomScale = Random.Range(scaleRange.x, scaleRange.y);
            float depthScale = Mathf.Pow(depthScaleFactor, depth);
            float additionalScale = Random.Range(additionalScaleRange.x, additionalScaleRange.y);
            item.transform.localScale = Vector3.one * (randomScale * depthScale * additionalScale);
            
            // 奥行きに基づいて色を暗くする
            float colorMultiplier = Mathf.Pow(depthColorFactor, depth);
            spriteRenderer.color = new Color(colorMultiplier, colorMultiplier, colorMultiplier, 1f);
            
            // ソートオーダーを奥行きに基づいて設定（奥が後ろ）
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-depth * 10);
        }
        
        // 落下コンポーネントを追加
        FallingItem fallingItem = item.AddComponent<FallingItem>();
        fallingItem.Initialize(
            Random.Range(fallSpeedRange.x, fallSpeedRange.y),
            Random.Range(rotationSpeedRange.x, rotationSpeedRange.y),
            destroyHeight
        );
    }

    /// <summary>
    /// ギズモで生成範囲を表示
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        
        // 生成範囲を表示
        Vector3 leftTop = new Vector3(-spawnRangeX, spawnHeight, 0);
        Vector3 rightTop = new Vector3(spawnRangeX, spawnHeight, 0);
        Vector3 leftBottom = new Vector3(-spawnRangeX, destroyHeight, 0);
        Vector3 rightBottom = new Vector3(spawnRangeX, destroyHeight, 0);
        
        Gizmos.DrawLine(leftTop, rightTop);
        Gizmos.DrawLine(leftTop, leftBottom);
        Gizmos.DrawLine(rightTop, rightBottom);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftBottom, rightBottom);
    }
}

/// <summary>
/// 落下するアイテムの挙動を制御
/// </summary>
public class FallingItem : MonoBehaviour
{
    private float _fallSpeed;
    private float _rotationSpeed;
    private float _destroyHeight;

    public void Initialize(float fallSpeed, float rotationSpeed, float destroyHeight)
    {
        _fallSpeed = fallSpeed;
        _rotationSpeed = rotationSpeed;
        _destroyHeight = destroyHeight;
    }

    void Update()
    {
        // 落下
        transform.position += Vector3.down * (_fallSpeed * Time.deltaTime);
        
        // 回転
        transform.Rotate(0, 0, _rotationSpeed * Time.deltaTime);
        
        // 削除判定
        if (transform.position.y < _destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}
