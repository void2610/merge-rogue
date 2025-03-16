
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
// using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

public class InputGuide : MonoBehaviour
{
    //InputActionReferenceはInputActionAsset内に存在する特定のInputActionへの参照をシリアライズできる
    [SerializeField] private InputActionReference actionReference = default;

    [SerializeField] private TextMeshProUGUI text = default;

    private static readonly StringBuilder TempStringBuilder = new StringBuilder();

    private void OnEnable()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        ProcessAction(actionReference.action);
    }

    private void ProcessAction(InputAction action)
    {
        TempStringBuilder.Clear();

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
                    TempStringBuilder.Append("<sprite=");
                    TempStringBuilder.Append(spriteIndex);
                    TempStringBuilder.Append(">");
                }
            }
        }

        TempStringBuilder.Append(" ");
        TempStringBuilder.Append(action.name);

        text.text = TempStringBuilder.ToString();
    }
    
    private int GetSpriteCharacterIndex(string name)
    {
        var t = text.spriteAsset.spriteCharacterTable.FirstOrDefault(character => character.name == name);
        return text.spriteAsset.spriteCharacterTable.IndexOf(t);
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
}