using UnityEngine;

public class MouseCursorManager : MonoBehaviour
{
    public static MouseCursorManager Instance { get; private set; }
    
    [SerializeField] private SerializableDictionary<CursorType, Texture2D> cursorTextures;
    
    public void SetCursor(CursorType type)
    {
        Cursor.SetCursor(cursorTextures[type], Vector2.zero, CursorMode.Auto);
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        SetCursor(CursorType.Default);
    }
}
