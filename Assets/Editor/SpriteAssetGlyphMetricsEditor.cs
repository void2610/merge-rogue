using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore;

public class SpriteAssetSelectiveEditor : EditorWindow
{
    [SerializeField] private TMP_SpriteAsset spriteAsset;

    // ------ GlyphRect ------
    private bool overrideRectX;
    private bool overrideRectY;
    private bool overrideRectW;
    private bool overrideRectH;
    private int newRectX;
    private int newRectY;
    private int newRectW;
    private int newRectH;

    // ------ GlyphMetrics ------
    private bool overrideMetricsW;
    private bool overrideMetricsH;
    private bool overrideMetricsXOffset;
    private bool overrideMetricsYOffset;
    private bool overrideMetricsXAdvance;
    private float newMetricsW;
    private float newMetricsH;
    private float newMetricsXOffset;   // horizontalBearingX
    private float newMetricsYOffset;   // horizontalBearingY
    private float newMetricsXAdvance;  // horizontalAdvance

    // ------ Scale ------
    private bool overrideScale;
    private float newScale;

    // ------ その他 (例: AtlasIndex, ID はSpriteCharacterではなくSpriteGlyphに相当) ------
    // 実運用ではIDを一括変更すると文字コードが被る可能性があるので注意が必要。
    private bool overrideAtlasIndex;
    private bool overrideID;
    private int newAtlasIndex;
    private uint newID;

    [MenuItem("Tools/TMP Sprite Asset Selective Editor")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAssetSelectiveEditor>("Sprite Asset Selective Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("TMP Sprite Asset - 選択的一括編集", EditorStyles.boldLabel);

        // 対象のSprite Asset
        spriteAsset = (TMP_SpriteAsset)EditorGUILayout.ObjectField("Sprite Asset", spriteAsset, typeof(TMP_SpriteAsset), false);
        if (spriteAsset == null)
        {
            EditorGUILayout.HelpBox("編集対象のTMP_SpriteAssetを指定してください。", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();

        // -------------------------
        // GlyphRect
        // -------------------------
        GUILayout.Label("GlyphRect (x, y, w, h)", EditorStyles.boldLabel);

        overrideRectX = EditorGUILayout.Toggle("Override X", overrideRectX);
        if (overrideRectX)
            newRectX = EditorGUILayout.IntField("X", newRectX);

        overrideRectY = EditorGUILayout.Toggle("Override Y", overrideRectY);
        if (overrideRectY)
            newRectY = EditorGUILayout.IntField("Y", newRectY);

        overrideRectW = EditorGUILayout.Toggle("Override W", overrideRectW);
        if (overrideRectW)
            newRectW = EditorGUILayout.IntField("W", newRectW);

        overrideRectH = EditorGUILayout.Toggle("Override H", overrideRectH);
        if (overrideRectH)
            newRectH = EditorGUILayout.IntField("H", newRectH);

        EditorGUILayout.Space();

        // -------------------------
        // GlyphMetrics
        // -------------------------
        GUILayout.Label("GlyphMetrics (width, height, xOffset, yOffset, xAdvance)", EditorStyles.boldLabel);

        overrideMetricsW = EditorGUILayout.Toggle("Override Width", overrideMetricsW);
        if (overrideMetricsW)
            newMetricsW = EditorGUILayout.FloatField("Width", newMetricsW);

        overrideMetricsH = EditorGUILayout.Toggle("Override Height", overrideMetricsH);
        if (overrideMetricsH)
            newMetricsH = EditorGUILayout.FloatField("Height", newMetricsH);

        overrideMetricsXOffset = EditorGUILayout.Toggle("Override XOffset", overrideMetricsXOffset);
        if (overrideMetricsXOffset)
            newMetricsXOffset = EditorGUILayout.FloatField("XOffset (horizontalBearingX)", newMetricsXOffset);

        overrideMetricsYOffset = EditorGUILayout.Toggle("Override YOffset", overrideMetricsYOffset);
        if (overrideMetricsYOffset)
            newMetricsYOffset = EditorGUILayout.FloatField("YOffset (horizontalBearingY)", newMetricsYOffset);

        overrideMetricsXAdvance = EditorGUILayout.Toggle("Override XAdvance", overrideMetricsXAdvance);
        if (overrideMetricsXAdvance)
            newMetricsXAdvance = EditorGUILayout.FloatField("XAdvance (horizontalAdvance)", newMetricsXAdvance);
        overrideScale = EditorGUILayout.Toggle("Override Scale", overrideScale);
        if (overrideScale)
            newScale = EditorGUILayout.FloatField("Scale", newScale);

        EditorGUILayout.Space();

        // -------------------------
        // その他 (AtlasIndex, IDなど)
        // -------------------------
        GUILayout.Label("その他 (AtlasIndex, ID)", EditorStyles.boldLabel);

        overrideAtlasIndex = EditorGUILayout.Toggle("Override AtlasIndex", overrideAtlasIndex);
        if (overrideAtlasIndex)
            newAtlasIndex = EditorGUILayout.IntField("AtlasIndex", newAtlasIndex);

        overrideID = EditorGUILayout.Toggle("Override ID", overrideID);
        if (overrideID)
            newID = (uint)EditorGUILayout.IntField("ID (uint)", (int)newID);

        EditorGUILayout.Space();

        // -------------------------
        // 一括適用ボタン
        // -------------------------
        if (GUILayout.Button("一括適用 (選択項目のみ上書き)"))
        {
            ApplySelectiveChanges();
        }
    }

    private void ApplySelectiveChanges()
    {
        if (spriteAsset == null || spriteAsset.spriteGlyphTable == null)
        {
            Debug.LogWarning("Sprite AssetかspriteGlyphTableが存在しません。");
            return;
        }

        // Undo対応
        Undo.RecordObject(spriteAsset, "Selective Update TMP Sprite Asset");

        // 1. spriteGlyphTable を更新 (RectやMetrics等)
        foreach (var glyph in spriteAsset.spriteGlyphTable)
        {
            // 既存のRect, Metricsを取得
            var oldRect = glyph.glyphRect;
            var oldMetrics = glyph.metrics;

            // ------ GlyphRect ------
            int x = overrideRectX ? newRectX : oldRect.x;
            int y = overrideRectY ? newRectY : oldRect.y;
            int w = overrideRectW ? newRectW : oldRect.width;
            int h = overrideRectH ? newRectH : oldRect.height;
            glyph.glyphRect = new GlyphRect(x, y, w, h);

            // ------ GlyphMetrics ------
            float width  = overrideMetricsW        ? newMetricsW       : oldMetrics.width;
            float height = overrideMetricsH        ? newMetricsH       : oldMetrics.height;
            float bx     = overrideMetricsXOffset  ? newMetricsXOffset : oldMetrics.horizontalBearingX;
            float by     = overrideMetricsYOffset  ? newMetricsYOffset : oldMetrics.horizontalBearingY;
            float adv    = overrideMetricsXAdvance ? newMetricsXAdvance: oldMetrics.horizontalAdvance;
            glyph.metrics = new GlyphMetrics(width, height, bx, by, adv);
            glyph.scale = overrideScale ? newScale : glyph.scale;

            // ------ AtlasIndex (必要に応じて) ------
            if (overrideAtlasIndex)
                glyph.atlasIndex = newAtlasIndex;
        }

        // 2. spriteCharacterTable を更新 (ScaleやID等)
        foreach (var character in spriteAsset.spriteCharacterTable)
        {
            // IDの上書き (注意: 既存の文字コードと重複すると不整合が起きる可能性がある)
            if (overrideID)
                character.unicode = newID;

            // SpriteCharacter の Scale を上書き
            // if (overrideScale)
            //     character.scale = newScale;
        }

        // Lookupテーブルを更新
        spriteAsset.UpdateLookupTables();

        // アセットに変更をマークして保存
        EditorUtility.SetDirty(spriteAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"TMP_SpriteAsset '{spriteAsset.name}' のグリフを選択的に上書きしました。");
    }
}
