using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OverRayWindow : MonoBehaviour
{
    [SerializeField]
    public Vector2 size = new Vector2(200, 200);

    [SerializeField] private GameObject window;
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void ShowWindow(RelicData r, Vector3 pos)
    {
        window.SetActive(true);

        nameText.text = r.name;
        descriptionText.text = r.description;

        window.GetComponent<RectTransform>().position = pos;
    }

    public void HideWindow()
    {
        window.SetActive(false);
    }

    private void Awake()
    {
        window.SetActive(false);
        // cg.alpha = 0;
        // cg.blocksRaycasts = false;
        // cg.interactable = false;
    }
}
