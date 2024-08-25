using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OverRayWindow : MonoBehaviour
{
    [SerializeField]
    public Vector2 size = new Vector2(200, 200);
    private string _text;
    [SerializeField]
    public string text
    {
        set
        {
            _text = value;
            if (t != null)
            {
                t.text = value;
            }
        }
        get { return _text; }
    }
    [SerializeField]
    public TMP_FontAsset font;
    [SerializeField]
    public float fontSize = 24;
    [SerializeField]
    public Vector2 position = new Vector2(0, 0);

    private GameObject window;
    private Image bgImage;
    private CanvasGroup cg;
    private TextMeshProUGUI t;

    void Awake()
    {
        window = new GameObject("Window");
        window.transform.SetParent(transform);
        window.transform.localPosition = new Vector3(position.x, position.y, -10);
        window.transform.localScale = Vector3.one;

        cg = window.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.blocksRaycasts = false;

        var g = new GameObject("BG");
        g.transform.SetParent(window.transform);
        g.transform.localScale = Vector3.one;
        bgImage = g.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform rectTransform = bgImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = new Vector2(0, 0);
        bgImage.raycastTarget = false;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(window.transform);
        textObject.transform.localScale = Vector3.one;
        t = textObject.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = _text;
        t.fontSize = this.fontSize;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        RectTransform textRectTransform = t.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = size;
        textRectTransform.anchoredPosition = new Vector2(0, 0);
        t.raycastTarget = false;

        // マウスオーバー時にウィンドウを表示
        EventTrigger trigger = this.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { cg.alpha = 1; });
        trigger.triggers.Add(entry);
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((data) => { cg.alpha = 0; });
        trigger.triggers.Add(entry);
    }
}
