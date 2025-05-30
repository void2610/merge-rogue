using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    public void SetText(string seed) => _text.text = $"seed: {seed}";

    private void Awake()
    {
        _text = this.GetComponent<TextMeshProUGUI>();
        
        // クリックしたらシードをコピーする
        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = _text.text.Split(' ')[1];
            NotifyWindow.Instance.Notify(NotifyWindow.NotifyType.SeedCopied);
        });
    }
}
