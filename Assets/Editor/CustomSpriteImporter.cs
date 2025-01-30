using UnityEditor;
using UnityEngine;

public class CustomSpriteImporter : AssetPostprocessor
{
    private const string TARGET_FOLDER = "Assets/Sprites/";
    
    // このメソッドは画像がインポートされる前に呼び出される
    private void OnPreprocessTexture()
    {
        // 初回インポートを判定する
        var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        if (EditorPrefs.HasKey($"CustomSpriteImporter_{assetGuid}"))
            return; // すでに処理済みのアセットはスキップ

        if (!assetPath.StartsWith(TARGET_FOLDER) || !assetPath.EndsWith(".png"))
            return;
        
        // インポーターを取得
        var importer = (TextureImporter)assetImporter;
        // 圧縮設定
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        // スプライトモード設定
        importer.textureType = TextureImporterType.Sprite;
        // スプライトモードを単数に
        importer.spriteImportMode = SpriteImportMode.Single;
        // フィルターモード設定
        importer.filterMode = FilterMode.Point;
        // ピクセル単位設定（例: 16ピクセル = 1ユニット）
        // importer.spritePixelsPerUnit = 16;

        // 処理済みとして記録
        EditorPrefs.SetBool($"CustomSpriteImporter_{assetGuid}", true);
    }
}