using UnityEngine;

public interface ICreditService
{
    string GetCreditText();
    string ConvertUrlsToLinks(string text);
    bool TryGetUrlFromPosition(string text, Vector2 position, out string url);
}