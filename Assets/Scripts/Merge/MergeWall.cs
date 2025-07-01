using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MergeWall : MonoBehaviour
{
    [SerializeField] private List<float> wallWidths = new() { 3.0f, 3.5f, 4.5f, 5.5f, 5.5f, 6.0f, 6.5f, 7.0f, 7.5f };
    [SerializeField] private int wallWidthLevel = 0;
    
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject floor;
    [SerializeField] private RectTransform image;

    public float WallWidth { get; private set; } = 2.5f;
    
    private float _defaultY = 0.0f;
    private const float MIN_WIDTH = 80.0f;
    private const float MAX_WIDTH = 365.0f;
    
    public int WallWidthLevel => wallWidthLevel;
    public float CurrentLevelWidth => wallWidths[wallWidthLevel];

    /// <summary>
    /// 壁幅をレベルアップする
    /// </summary>
    public void LevelUpWallWidth()
    {
        if (wallWidthLevel < wallWidths.Count - 1)
        {
            wallWidthLevel++;
            SetWallWidth(wallWidths[wallWidthLevel]);
        }
    }
    
    /// <summary>
    /// 壁幅をランダムに変更する（現在のレベルを基準に）
    /// </summary>
    public void RandomizeWallWidth(float randomOffset)
    {
        var newWidth = randomOffset + wallWidths[wallWidthLevel];
        SetWallWidth(newWidth);
    }
    
    private void SetWallWidth(float width)
    {
        WallWidth = width;
        // 1.5から10をminWidthからmaxWidthの範囲に収める
        var ratio = (width - 1.5f) / 8.5f;
        image.DOSizeDelta(new Vector2(MIN_WIDTH + (MAX_WIDTH - MIN_WIDTH) * ratio, image.sizeDelta.y), 0.5f);
        
        leftWall.transform.DOMoveX(-WallWidth / 2, 0.5f);
        rightWall.transform.DOMoveX(WallWidth / 2, 0.5f);
        floor.transform.DOScaleX(WallWidth, 0.5f);
    }

    private void Awake()
    {
        _defaultY = leftWall.transform.position.y;
        leftWall.transform.position = new Vector3(-WallWidth / 2, _defaultY, 0);
        rightWall.transform.position = new Vector3(WallWidth / 2, _defaultY, 0);
        floor.transform.localScale = new Vector3(WallWidth, 0.4f, 1);
        
        // 初期の壁幅を設定
        SetWallWidth(wallWidths[wallWidthLevel]);
    }
}
