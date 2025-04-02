using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.DualShock;
// using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Serialization;

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
    
    [SerializeField] private GameObject inputGuidePrefab;
    [SerializeField] private TMP_SpriteAsset spriteAsset;
    [SerializeField] private Vector2 leftPos;
    [SerializeField] private Vector2 rightPos;
    [SerializeField] private float alignment;
    
    public event Action<InputSchemeType> OnSchemeChanged;
    private Action<InputSchemeType> _onSchemeChanged;
    private InputGuideType _currentType = InputGuideType.Navigate;
    private InputSchemeType _scheme = InputSchemeType.KeyboardAndMouse;

    public InputSchemeType Scheme
    {
        get => _scheme;
        private set
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
    
    private static string GetDeviceIconGroup(InputDevice device)
    {
        return device switch
        {
            Keyboard => "Keyboard",
            Mouse => "Mouse",
            XInputController => "XInputController",
            DualShockGamepad => "DualShockGamepad",
            // SwitchProControllerHID => "SwitchProController",
            _ => null
        };
    }
    
    private void OnEnable()
    {
        UpdateText(_currentType);
        OnSchemeChanged += _onSchemeChanged = _ => UpdateText(_currentType);
        
        InputSystem.onEvent += OnEvent;
    }

    private void OnDisable()
    {
        if (_onSchemeChanged != null)
        {
            OnSchemeChanged -= _onSchemeChanged;
            _onSchemeChanged = null;
        }
        
        InputSystem.onEvent -= OnEvent;
    }

    private void Start()
    {
        UpdateText(InputGuideType.Navigate);
    }

    public void UpdateText(InputGuideType type)
    {
        foreach(Transform child in this.transform) Destroy(child.gameObject);
        
        _currentType = type;
        switch (type)
        {
            case InputGuideType.Merge:
                var t1 = GetMergeGuideTexts();
                TextMeshProUGUI last1 = null;
                for(var i = 0; i < t1.Count; i++)
                {
                    var obj = Instantiate(inputGuidePrefab, this.transform);
                    var a = i == 0 ? leftPos.x : (last1.transform.localPosition.x + alignment * last1.GetPreferredValues().x);
                    obj.transform.localPosition = new Vector2(a, leftPos.y);
                    obj.GetComponent<TextMeshProUGUI>().text = t1[i];
                    last1 = obj.GetComponent<TextMeshProUGUI>();
                }
                break;
            case InputGuideType.Navigate:
                var t2 = GetNavigateGuideTexts();
                TextMeshProUGUI last2 = null;
                for(var i = 0; i < t2.Count; i++)
                {
                    var obj = Instantiate(inputGuidePrefab, this.transform);
                    var a = i == 0 ? leftPos.x : (last2.transform.localPosition.x + alignment * last2.GetPreferredValues().x);
                    obj.transform.localPosition = new Vector2(a, leftPos.y);
                    obj.GetComponent<TextMeshProUGUI>().text = t2[i];
                    last2 = obj.GetComponent<TextMeshProUGUI>();
                }
                break;
        }

        var t3 = GetShortcutGuideTexts();
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
    
    private List<string> GetShortcutGuideTexts()
    {
        var shortcutTexts = new List<string>();
        shortcutTexts.Add("カーソルをリセット: <sprite name=\"Keyboard-r\">");
        shortcutTexts.Add("カーソル位置を切り替え: <sprite name=\"Keyboard-n\">");
        shortcutTexts.Add("仮想マウス切り替え: <sprite name=\"Keyboard-l\">");
        shortcutTexts.Add("チュートリアル: <sprite name=\"Keyboard-q\">");
        shortcutTexts.Add("マップ: <sprite name=\"Keyboard-m\">");
        shortcutTexts.Add("倍速: <sprite name=\"Keyboard-t\">");
        shortcutTexts.Add("ポーズ: <sprite name=\"Keyboard-p\">");
        return shortcutTexts;
    }
    
    private List<string> GetMergeGuideTexts()
    {
        var mergeTexts = new List<string>();
        mergeTexts.Add("移動: <sprite name=\"Keyboard-leftArrow\"><sprite name=\"Keyboard-rightArrow\">/<sprite name=\"Keyboard-a\"><sprite name=\"Keyboard-d\">/<sprite name=\"Mouse-position\">");
        mergeTexts.Add("ドロップ: <sprite name=\"Keyboard-space\">/<sprite name=\"Mouse-leftButton\">");
        mergeTexts.Add("スキップ: <sprite name=\"Keyboard-shift\">/<sprite name=\"Mouse-rightButton\">");
        return mergeTexts;
    }
    
    private List<string> GetNavigateGuideTexts()
    {
        var navigaTetexts = new List<string>();
        navigaTetexts.Add("選択: <sprite name=\"Keyboard-leftArrow\"><sprite name=\"Keyboard-rightArrow\">/<sprite name=\"Keyboard-a\"><sprite name=\"Keyboard-d\">/<sprite name=\"Mouse-position\">");
        navigaTetexts.Add("決定: <sprite name=\"Keyboard-space\">/<sprite name=\"Keyboard-space\">/<sprite name=\"Mouse-leftButton\">");
        return navigaTetexts;
    }
}