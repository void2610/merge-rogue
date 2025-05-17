using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TMP_InputField のキャレットと選択ハイライトの Raycast Target を無効にするコンポーネント
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class TMPInputFieldCaretFixer : MonoBehaviour
{
    private TMP_InputField _inputField;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();

        // フォーカスを受け取ったタイミングで遅延実行
        _inputField.onSelect.AddListener(_ => Invoke(nameof(DisableCaretRaycast), 0.01f));
    }

    private void DisableCaretRaycast()
    {
        // 親オブジェクトの中から "Caret" や "Selection Highlight" を探して raycastTarget を false に
        var parent = _inputField.textComponent.transform.parent;

        var caret = parent.Find("Caret");
        if (caret && caret.TryGetComponent(out Graphic caretGraphic))
        {
            caretGraphic.raycastTarget = false;
        }

        var highlight = parent.Find("Selection Highlight");
        if (highlight && highlight.TryGetComponent(out Graphic highlightGraphic))
        {
            highlightGraphic.raycastTarget = false;
        }
    }
}