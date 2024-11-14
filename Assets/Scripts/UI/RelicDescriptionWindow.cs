using UnityEngine;
using TMPro;

public class RelicDescriptionWindow : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Vector2 minPos; // RectTransform上の座標で指定
    [SerializeField] private Vector2 maxPos; // RectTransform上の座標で指定

    public void ShowWindow(RelicData r, Vector3 pos)
    {
        this.gameObject.SetActive(true);

        nameText.text = r.displayName;
        descriptionText.text = r.description;

        // ワールド座標をRectTransformのローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.gameObject.transform.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(Camera.main, pos),
            Camera.main,
            out Vector2 localPos
        );

        // ローカル座標で位置をクランプ
        float clampedX = Mathf.Clamp(localPos.x, minPos.x, maxPos.x);
        float clampedY = Mathf.Clamp(localPos.y, minPos.y, maxPos.y);

        this.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(clampedX, clampedY, 0);
    }

    public void HideWindow()
    {
        this.gameObject.SetActive(false);
    }

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }
}