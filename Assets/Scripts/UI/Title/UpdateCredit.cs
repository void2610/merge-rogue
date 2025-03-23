using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpdateCredit : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextAsset textAsset;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RectTransform content;
    
    private void Start()
    {
        var textData = textAsset.text;
        textData = "\n\n\n\n" + textData;
        
        // URL をリンクとして扱えるようにする
        text.text = ConvertUrlsToLinks(textData);
        
        // テキストのPreferred Valuesを取得
        var preferredHeight = text.GetPreferredValues().y;

        // Contentの高さをPreferred Heightに合わせる
        content.sizeDelta = new Vector2(content.sizeDelta.x, preferredHeight);
    }

    /// <summary>
    /// クリック時にリンク部分を判定し、URLを開く
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, Camera.main);
        if (linkIndex != -1)
        {
            var linkInfo = text.textInfo.linkInfo[linkIndex];
            var url = linkInfo.GetLinkID();
            Application.OpenURL(url);
        }
    }

    /// <summary>
    /// テキスト内のURLをTextMeshProのリンク形式に変換
    /// </summary>
    private string ConvertUrlsToLinks(string textData)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            textData,
            @"(http[s]?:\/\/[^\s]+)",
            "<link=\"$1\"><u>$1</u></link>"
        );
    }
}
