using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MouseCursorManager : MonoBehaviour
{
    public static MouseCursorManager Instance { get; private set; }
    
    [SerializeField] private SerializableDictionary<CursorIconType, Texture2D> cursorTextures;
    [SerializeField] private SerializableDictionary<CursorIconType, Sprite> cursorSprites;
    
    public void SetCursor(CursorIconType iconType)
    {
        Cursor.SetCursor(cursorTextures[iconType], Vector2.zero, CursorMode.Auto); 
        FindFirstObjectByType<MyVirtualMouseInput>().GetComponent<Image>().sprite = cursorSprites[iconType];
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        SetCursor(CursorIconType.Default);
    }
}
