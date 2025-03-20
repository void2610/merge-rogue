using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectionMarker : MonoBehaviour
{
    [SerializeField] private Vector2 offset;
    [SerializeField] private Camera uiCamera;
    private RectTransform _marker;
    private Image _image;

    private void Awake()
    {
        _marker = this.GetComponent<RectTransform>();
        _image = this.GetComponent<Image>();
    }
    
    
    private void Update()
    {
        // 現在選択されているUI要素を取得
        var selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject && selectedObject.TryGetComponent<RectTransform>(out RectTransform selectedRect))
        {
            // UI 要素のワールド座標を取得
            var corners = new Vector3[4];
            selectedRect.GetWorldCorners(corners);

            // ワールド座標 → スクリーン座標 → ローカル座標へ変換
            var min = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

            // マーカーの位置とサイズを更新
            _marker.position = selectedRect.position; // 位置を中央に
            _marker.sizeDelta = new Vector2(max.x - min.x, max.y - min.y) + offset; // 実際の表示サイズを取得

            _image.color = new Color(1, 1, 1, 1);
        }
        else
        {
            _image.color = new Color(1, 1, 1, 0);
        }
    }
}