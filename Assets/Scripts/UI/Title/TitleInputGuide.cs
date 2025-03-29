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
using UnityEngine.Serialization;

public class TitleInputGuide : MonoBehaviour
{
    [SerializeField] private GameObject inputGuidePrefab;
    [SerializeField] private TMP_SpriteAsset spriteAsset;
    [SerializeField] private Vector2 leftPos;
    [SerializeField] private Vector2 rightPos;
    [SerializeField] private float alignment;
    
    private static readonly StringBuilder _tempStringBuilder = new();

    private void Start()
    {
        var list = new List<string>();
        list.Add("選択: <sprite name=\"Keyboard-leftArrow\"><sprite name=\"Keyboard-rightArrow\">/<sprite name=\"Keyboard-a\"><sprite name=\"Keyboard-d\">/<sprite name=\"Mouse-position\">");
        list.Add("決定: <sprite name=\"Keyboard-space\">/<sprite name=\"Keyboard-space\">/<sprite name=\"Mouse-leftButton\">");
        list.Add("カーソルをリセット: <sprite name=\"Keyboard-r\">");
        
        TextMeshProUGUI last = null;
        for(var i = 0; i < list.Count; i++)
        {
            var obj = Instantiate(inputGuidePrefab, this.transform);
            var a = i == 0 ? leftPos.x : (last.transform.localPosition.x + alignment * last.GetPreferredValues().x);
            obj.transform.localPosition = new Vector2(a, leftPos.y);
            obj.GetComponent<TextMeshProUGUI>().text = list[i];
            last = obj.GetComponent<TextMeshProUGUI>();
        }
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