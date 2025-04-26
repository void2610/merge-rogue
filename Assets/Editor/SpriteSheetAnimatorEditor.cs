using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(EnemyData))]
public class SpriteSheetAnimatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(6);
        if (GUILayout.Button("Refresh Frames"))
        {
            (target as EnemyData)?.RefreshFrames();
        }
    }
}
#endif