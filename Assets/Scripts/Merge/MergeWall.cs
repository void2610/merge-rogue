using UnityEngine;
using DG.Tweening;

public class MergeWall : MonoBehaviour
{
    [SerializeField]
    private GameObject leftWall;
    [SerializeField]
    private GameObject rightWall;
    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private float xOffset = 0.0f;

    public float WallWidth { get; private set; } = 1.5f;
    private float defaultY = 0.0f;

    public void SetWallWidth(float width)
    {
        WallWidth = width;
        leftWall.transform.DOMoveX(-WallWidth / 2 + xOffset, 0.5f);
        rightWall.transform.DOMoveX(WallWidth / 2 + xOffset, 0.5f);
        floor.transform.DOScaleX(WallWidth, 0.5f);
    }

    private void Awake()
    {
        defaultY = leftWall.transform.position.y;
        leftWall.transform.position = new Vector3(-WallWidth / 2 + xOffset, defaultY, 0);
        rightWall.transform.position = new Vector3(WallWidth / 2 + xOffset, defaultY, 0);
        floor.transform.localScale = new Vector3(WallWidth, 0.4f, 1);
    }
}
