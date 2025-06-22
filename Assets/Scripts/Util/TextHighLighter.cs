using UnityEngine;
using TMPro;

public class TextHighLighter : MonoBehaviour
{
    [SerializeField] private WordDictionary wordDictionary;
    private void Start()
    {
        var targetText = GetComponent<TextMeshProUGUI>();
        targetText.text = Utils.GetHighlightWords(wordDictionary, targetText.text);
    }
}
