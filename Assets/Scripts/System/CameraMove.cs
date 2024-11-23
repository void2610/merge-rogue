using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    private Vector3 initPosition;

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
