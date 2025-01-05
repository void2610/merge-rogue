using TMPro;
using UnityEngine;

public class UpdateCredit : MonoBehaviour
{
    [SerializeField] private TextAsset textAsset;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RectTransform content;
    
    private void Start()
    {
        var textData = textAsset.text;
        textData = "\n\n\n\n" + textData;
        text.text = textData;
        
        // テキストのPreferred Valuesを取得
        var preferredHeight = text.GetPreferredValues().y;

        // Contentの高さをPreferred Heightに合わせる
        content.sizeDelta = new Vector2(content.sizeDelta.x, preferredHeight);
    }
}
