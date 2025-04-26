using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SpriteSheetAnimator))]
public class SpriteSheetAnimatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(6);
        if (GUILayout.Button("Refresh Frames"))
        {
            (target as SpriteSheetAnimator)?.RefreshFrames();
        }
    }
}
#endif