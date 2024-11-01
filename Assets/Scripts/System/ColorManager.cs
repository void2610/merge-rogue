using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    [SerializeField]
    private Color primaryColor;
    [SerializeField]
    private List<GameObject> primaryColorObjects;
    [SerializeField]
    private Color secondaryColor;
    [SerializeField]
    private List<GameObject> secondaryColorObjects;
    [SerializeField]
    private Color accentColor;
    [SerializeField]
    private List<GameObject> accentColorObjects;
    [SerializeField]
    private Color backgroundColor;
    [SerializeField]
    private List<GameObject> backgroundColorObjects;
    
    public enum ColorType
    {
        Primary,
        Secondary,
        Accent,
        Background
    }
    
    public void AddColorObject(GameObject obj, ColorType colorType)
    {
        switch (colorType)
        {
            case ColorType.Primary:
                primaryColorObjects.Add(obj);
                SetColor(primaryColorObjects, primaryColor);
                break;
            case ColorType.Secondary:
                secondaryColorObjects.Add(obj);
                SetColor(secondaryColorObjects, secondaryColor);
                break;
            case ColorType.Accent:
                accentColorObjects.Add(obj);
                SetColor(accentColorObjects, accentColor);
                break;
            case ColorType.Background:
                backgroundColorObjects.Add(obj);
                SetColor(backgroundColorObjects, backgroundColor);
                break;
        }
    }
    
    public Color GetColor(ColorType colorType)
    {
        switch (colorType)
        {
            case ColorType.Primary:
                return primaryColor;
            case ColorType.Secondary:
                return secondaryColor;
            case ColorType.Accent:
                return accentColor;
            case ColorType.Background:
                return backgroundColor;
            default:
                return Color.white;
        }
    }
    
    private void SetColor(List<GameObject> list, Color color)
    {
        for(int i = 0; i < list.Count; i++)
        {
            var obj = list[i];
            if (!obj)
            {
                list.Remove(obj);
                continue;
            }
            if(obj.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = color;
            }
            if(obj.TryGetComponent(out TMPro.TextMeshProUGUI textMeshProUGUI))
            {
                textMeshProUGUI.color = color;
            }
            if(obj.TryGetComponent(out LineRenderer lineRenderer))
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
            if(obj.TryGetComponent(out Image image))
            {
                image.color = color;
            }
        }
    }

    private void Start()
    {
        SetColor(primaryColorObjects, primaryColor);
        SetColor(secondaryColorObjects, secondaryColor);
        SetColor(accentColorObjects, accentColor);
        SetColor(backgroundColorObjects, backgroundColor);
    }
}
