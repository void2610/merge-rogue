using UnityEngine;

public class CameraAspectRatioHandler : MonoBehaviour
{
    public float aspectWidth = 16.0f;
    public float aspectHeight = 9.0f;
    private float _targetAspect;
    private float _lastScreenWidth;
    private float _lastScreenHeight;

    private void Awake()
    {
        _targetAspect = aspectWidth / aspectHeight;
        AdjustCameraSize();
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

    private void Update()
    {
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            AdjustCameraSize();
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
        }
    }

    private void AdjustCameraSize()
    {
        var windowAspect = (float)Screen.width / (float)Screen.height;
        var scaleHeight = windowAspect / _targetAspect;

        var c = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            var rect = c.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            c.rect = rect;
        }
        else
        {
            var scaleWidth = 1.0f / scaleHeight;
            var rect = c.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            c.rect = rect;
        }
    }

    private void OnPreCull()
    {
        GL.Clear(true, true, Color.black);
    }
}