using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    private Tweener _shakeTweener;
    private Vector3 _initPosition;

    void Start()
    {
        // 初期位置を保存
        _initPosition = this.transform.position;
    }

    public void ShakeCamera(float duration, float strength)
    {
        // if (_shakeTweener != null && _shakeTweener.IsActive())
        // {
        //     _shakeTweener.Kill();
        //     this.transform.position = _initPosition;
        // }

        _shakeTweener = this.transform.DOShakePosition(duration, strength, 10, 0, false).OnComplete(() =>
        {
            this.transform.position = _initPosition;
        });
    }
}
