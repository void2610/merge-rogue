using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    public static CameraMove Instance { get; private set; }
    private Vector3 initPosition;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        // 初期位置を保存
        initPosition = this.transform.position;
    }

    public void ShakeCamera(float duration, float strength)
    {
        this.transform.DOShakePosition(duration, strength, 10, 0, false).OnComplete(() =>
        {
            this.transform.position = initPosition;
        });
    }
}
