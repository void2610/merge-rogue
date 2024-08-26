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

    public float wallWidth { get; private set; } = 4.5f;
    private float defaultY = 0.0f;

    public void SetWallWidth(float width)
    {
        wallWidth = width;
        leftWall.transform.DOMoveX(-wallWidth / 2, 0.5f);
        rightWall.transform.DOMoveX(wallWidth / 2, 0.5f);
        floor.transform.DOScaleX(wallWidth, 0.5f);
    }

    private void Awake()
    {
        defaultY = leftWall.transform.position.y;
        leftWall.transform.position = new Vector3(-wallWidth / 2, defaultY, 0);
        rightWall.transform.position = new Vector3(wallWidth / 2, defaultY, 0);
        floor.transform.localScale = new Vector3(wallWidth, 0.4f, 1);
    }
}
