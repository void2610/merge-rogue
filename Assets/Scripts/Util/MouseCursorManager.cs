using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MouseCursorManager : MonoBehaviour
{
    public static MouseCursorManager Instance { get; private set; }
    
    [SerializeField] private SerializableDictionary<CursorType, Texture2D> cursorTextures;
    [SerializeField] private SerializableDictionary<CursorType, Sprite> cursorSprites;
    
    public void SetCursor(CursorType type)
    {
        Cursor.SetCursor(cursorTextures[type], Vector2.zero, CursorMode.Auto); 
        FindFirstObjectByType<VirtualMouseInput>().GetComponent<Image>().sprite = cursorSprites[type];
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        SetCursor(CursorType.Default);
    }
}
