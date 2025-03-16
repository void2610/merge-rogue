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