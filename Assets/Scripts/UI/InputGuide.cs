using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.DualShock;
// using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Serialization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class InputGuide : MonoBehaviour
{
    public enum InputGuideType
    {
        Merge,
        Navigate,
    }
    
    public enum InputSchemeType
    {
        KeyboardAndMouse,
        Gamepad
    }

    [Serializable]
    public class InputGuideData
    {
        public LocalizedString localizedName;
        public InputActionReference action;
    }

    [SerializeField] private List<InputGuideData> mergeGuides = new();
    [SerializeField] private List<InputGuideData> shortcutGuides = new();
    [SerializeField] private LocalizedString dropText;
    [SerializeField] private LocalizedString submitText;
    [SerializeField] private LocalizedString moveText;
    [SerializeField] private GameObject inputGuidePrefab;
    [SerializeField] private Vector2 leftPos;
    [SerializeField] private Vector2 rightPos;
    [SerializeField] private float alignment;
    public event Action<InputSchemeType> OnSchemeChanged;
    private Action<InputSchemeType> _onSchemeChanged;
    private InputGuideType _currentType = InputGuideType.Navigate;
    private InputSchemeType _scheme = InputSchemeType.KeyboardAndMouse;

    private InputSchemeType Scheme
    {
        get => _scheme;
        set
        {
            if (_scheme == value) return;
            _scheme = value;
            OnSchemeChanged?.Invoke(_scheme);
        }
    }

    // デバイスからの生の入力を受け取って現在のスキーマを更新する
    private void OnEvent(InputEventPtr eventPtr, InputDevice device)
    {
        var eventType = eventPtr.type;
        if (eventType != StateEvent.Type && eventType != DeltaStateEvent.Type)
            return;

        var anyControl = eventPtr.EnumerateControls(
            InputControlExtensions.Enumerate.IncludeNonLeafControls |
            InputControlExtensions.Enumerate.IncludeSyntheticControls |
            InputControlExtensions.Enumerate.IgnoreControlsInCurrentState |
            InputControlExtensions.Enumerate.IgnoreControlsInDefaultState
        ).GetEnumerator().MoveNext();

        if (!anyControl) return;

        Scheme = device switch
        {
            Keyboard or Mouse => InputSchemeType.KeyboardAndMouse,
            Gamepad => InputSchemeType.Gamepad,
            _ => Scheme
        };
    }
    
    private void OnEnable()
    {
        UpdateText(_currentType);
        OnSchemeChanged += _onSchemeChanged = _ => UpdateText(_currentType);
        
        InputSystem.onEvent += OnEvent;
        
        // ロケール変更時のイベントを購読
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        if (_onSchemeChanged != null)
        {
            OnSchemeChanged -= _onSchemeChanged;
            _onSchemeChanged = null;
        }
        
        InputSystem.onEvent -= OnEvent;
        
        // ロケール変更時のイベントの購読解除
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    
    private void OnLocaleChanged(Locale locale)
    {
        UpdateText(_currentType);
    }

    private void Start()
    {
        UpdateText(InputGuideType.Navigate);
    }

    public void UpdateText(InputGuideType type)
    {
        foreach (Transform child in this.transform)
        {
            if(!child.TryGetComponent<Image>(out _)) Destroy(child.gameObject);
        }
        
        _currentType = type;
        var t = GetOperationDisplay(_currentType);
        t.AddRange(GetGuideTexts(mergeGuides));
        TextMeshProUGUI last1 = null;
        for(var i = 0; i < t.Count; i++)
        {
            var obj = Instantiate(inputGuidePrefab, this.transform);
            var a = i == 0 ? leftPos.x : (last1.transform.localPosition.x + alignment * last1.GetPreferredValues().x);
            obj.transform.localPosition = new Vector2(a, leftPos.y);
            obj.GetComponent<TextMeshProUGUI>().text = t[i];
            last1 = obj.GetComponent<TextMeshProUGUI>();
        }

        var t3 = GetGuideTexts(shortcutGuides);
        TextMeshProUGUI last3 = null;
        for(var i = 0; i < t3.Count; i++)
        {
            var obj = Instantiate(inputGuidePrefab, this.transform);
            var a = i == 0 ? rightPos.x : (last3.transform.localPosition.x + alignment * last3.GetPreferredValues().x);
            obj.transform.localPosition = new Vector2(a, rightPos.y);
            obj.GetComponent<TextMeshProUGUI>().text = t3[i];
            last3 = obj.GetComponent<TextMeshProUGUI>();
        }
    }
    
    private List<string> GetGuideTexts(List<InputGuideData> list)
    {
        var shortcutTexts = new List<string>();
        foreach (var data in list)
        {
            var displaySprite = "";
            var action = data.action.action;
            // 現在のスキーマに合致するバインディングを探す
            foreach (var binding in action.bindings)
            {
                if (IsBindingForCurrentScheme(binding))
                {
                    // binding.pathからスプライト用の名前を作成する
                    string spriteName = GetSpriteNameFromBinding(binding);
                    // スプライトタグとして出力（例: <sprite name="keyboard-shift">）
                    displaySprite = $"<sprite name=\"{spriteName}\">";
                    break;
                }
            }
            shortcutTexts.Add($"{data.localizedName.GetLocalizedString()}: {displaySprite}");
        }
        return shortcutTexts;
    }

    /// <summary>
    /// binding.path から、スプライト命名規則に沿った名前を生成する。
    /// 例: binding.path が "<Keyboard>/shift" の場合 "keyboard-shift" を返す。
    /// </summary>
    private string GetSpriteNameFromBinding(InputBinding binding)
    {
        if (string.IsNullOrEmpty(binding.path)) return "";
        
        // 例: "<Keyboard>/shift" から "Keyboard" を抽出
        var start = binding.path.IndexOf('<') + 1;
        var end = binding.path.IndexOf('>');
        if (start < 0 || end < 0 || end <= start) return "";
        
        var device = binding.path.Substring(start, end - start);

        // '/' 以降のコントロール名を抽出（例: "shift"）
        var slashIndex = binding.path.IndexOf('/');
        var control = "";
        if (slashIndex >= 0 && slashIndex < binding.path.Length - 1)
        {
             control = binding.path[(slashIndex + 1)..];
        }

        // スプライト命名は小文字に変換して、"device-control" の形式にする
        // 必要に応じて、特殊な名称の変換もここで実施可能
        return $"{device}-{control}".ToLower();
    }

    /// <summary>
    /// 現在の入力スキーマに該当するかを、バインディングの path を元に判定する
    /// </summary>
    private bool IsBindingForCurrentScheme(InputBinding binding)
    {
        if (string.IsNullOrEmpty(binding.path))
            return false;

        if (_scheme == InputSchemeType.KeyboardAndMouse)
        {
            // 例: "<Keyboard>/p" または "<Mouse>/leftButton" なら KeyboardAndMouse と判定
            return binding.path.StartsWith("<Keyboard>") || binding.path.StartsWith("<Mouse>");
        }
        else if (_scheme == InputSchemeType.Gamepad)
        {
            // 例: "<Gamepad>/buttonSouth" などの場合
            return binding.path.StartsWith("<Gamepad>") ||
                   binding.path.StartsWith("<XInputController>") ||
                   binding.path.StartsWith("<DualShockGamepad>");
        }
        return false;
    }

    private List<string> GetOperationDisplay(InputGuideType type)
    {
        var t2 = type == InputGuideType.Merge ? dropText.GetLocalizedString() : submitText.GetLocalizedString();
        var list = new List<string>();
        if (_scheme == InputSchemeType.KeyboardAndMouse)
        {
            list.Add(t2 + ": <sprite name=\"Keyboard-space\">/<sprite name=\"Mouse-leftButton\">");
        }
        else
        {
            list.Add(t2 + ": <sprite name=\"Gamepad-buttonSouth\">");
        }
        return list;
    }
}

