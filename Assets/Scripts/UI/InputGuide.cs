using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
// using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

public class InputGuide : MonoBehaviour
{
    public enum InputGuideType
    {
        Merge,
        Navigate,
    }
    
    //InputActionReferenceはInputActionAsset内に存在する特定のInputActionへの参照をシリアライズできる
    [SerializeField] private GameObject inputGuidePrefab;
    [SerializeField] private TMP_SpriteAsset spriteAsset;
    [SerializeField] private Vector2 position;
    [SerializeField] private float alignment;
    private static readonly StringBuilder _tempStringBuilder = new();
    private InputGuideType _currentType = InputGuideType.Navigate;

    private void Awake()
    {
        UpdateText(_currentType);
    }

    public void UpdateText(InputGuideType type)
    {
        _currentType = type;
        switch (type)
        {
            case InputGuideType.Merge:
                var t1 = GetMergeGuideTexts();
                TextMeshProUGUI last1 = null;
                for(var i = 0; i < t1.Count; i++)
                {
                    var obj = Instantiate(inputGuidePrefab, this.transform);
                    var a = i == 0 ? position.x : (last1.transform.localPosition.x + alignment * last1.GetPreferredValues().x);
                    obj.transform.localPosition = new Vector2(a, position.y);
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
                    var a = i == 0 ? position.x : (last2.transform.localPosition.x + alignment * last2.GetPreferredValues().x);
                    obj.transform.localPosition = new Vector2(a, position.y);
                    obj.GetComponent<TextMeshProUGUI>().text = t2[i];
                    last2 = obj.GetComponent<TextMeshProUGUI>();
                }
                break;
        }
    }

    private void ProcessAction(InputAction action)
    {
        _tempStringBuilder.Clear();

        foreach (var binding in action.bindings)
        {
            //InputBindingのパスを取得する
            //effectivePathは、ランタイムのRebindingなどでoverridePathが設定されている場合も実効的なパスを取得できる
            var path = binding.effectivePath;

            //パスに合致するcontrolを取得する
            var matchedControls = action.controls.Where(control => InputControlPath.Matches(path, control));

            foreach (var control in matchedControls)
            {
                if (control is InputDevice) continue;

                // controlのpathは "/[デバイス名]/[パス]" のようフォーマットになっている
                // このデバイス名は"XInputControllerWindows" や "DualShock4GamepadHID"のように具体的すぎるので、
                // "XInputController"や"DualShockGamepad"のように、アイコンを表示するうえで適度に抽象化されたデバイス名に置き換える

                var deviceIconGroup = GetDeviceIconGroup(control.device);
                if (string.IsNullOrEmpty(deviceIconGroup)) continue;

                var controlPathContent = control.path.Substring(control.device.name.Length + 2);
                var iconName = $"{deviceIconGroup}-{controlPathContent}";
                var spriteIndex = GetSpriteCharacterIndex(iconName);
                Debug.Log($"{iconName}: {spriteIndex}");
                
                if (spriteIndex >= 0)
                {
                    _tempStringBuilder.Append("<sprite=");
                    _tempStringBuilder.Append(spriteIndex);
                    _tempStringBuilder.Append(">");
                }
            }
        }

        _tempStringBuilder.Append(" ");
        _tempStringBuilder.Append(action.name);

        // text.text = _tempStringBuilder.ToString();
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
        Debug.Log("GetNavigateGuideTexts");
        var navigaTetexts = new List<string>();
        navigaTetexts.Add("選択: <sprite name=\"Keyboard-leftArrow\"><sprite name=\"Keyboard-rightArrow\">");
        navigaTetexts.Add("決定: <sprite name=\"Keyboard-space\">");
        return navigaTetexts;
    }
    
    private int GetSpriteCharacterIndex(string name)
    {
        var t = spriteAsset.spriteCharacterTable.FirstOrDefault(character => character.name == name);
        return spriteAsset.spriteCharacterTable.IndexOf(t);
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
    
    private int GetTextLength(string inputText)
    {
        var pattern = @"<sprite\s+name=""[^""]*"">";
        var replacement = "<sprite>";
        return Regex.Replace(inputText, pattern, replacement).Length;
    }
}