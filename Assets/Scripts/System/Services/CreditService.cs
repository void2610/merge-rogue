using UnityEngine;
using TMPro;

public class CreditService : ICreditService
{
    private readonly TextAsset _textAsset;
    public CreditService(TextAsset textAsset)
    {
        _textAsset = textAsset;
    }
    
    public string GetCreditText()
    {
        var textData = _textAsset.text;
        textData = "\n\n\n\n" + textData;
        
        return ConvertUrlsToLinks(textData);
    }

    public string ConvertUrlsToLinks(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            text,
            @"(http[s]?:\/\/[^\s]+)",
            "<link=\"$1\"><u>$1</u></link>"
        );
    }

    /// <summary>
    /// 指定された位置からURLを取得を試みます。現在は実装されていません。
    /// </summary>
    /// <param name="text">テキスト</param>
    /// <param name="position">位置</param>
    /// <param name="url">取得されたURL</param>
    /// <returns>URL取得の成否</returns>
    public bool TryGetUrlFromPosition(string text, Vector2 position, out string url)
    {
        url = string.Empty;
        
        // 注意: このメソッドはTMP_Textコンポーネントへのアクセスが必要
        // 現在はfalseを返し、UIプレゼンターで実際のリンク検出を処理
        return false;
    }
}