using UnityEngine;
using TMPro;

public class TextHighLighter : MonoBehaviour
{
    private void Start()
    {
        var targetText = GetComponent<TextMeshProUGUI>();
        targetText.text = Utils.GetHighlightWords(targetText.text);
    }
}
