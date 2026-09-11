#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class CandyBitmapFontImporter
{
    [Serializable]
    private class CandyGlyph
    {
        public string charValue;
        public string name;
        public int x;
        public int y;
        public int width;
        public int height;
        public int xOffset;
        public int yOffset;
        public int xAdvance;

        public string GetChar()
        {
            if (!string.IsNullOrEmpty(charValue)) return charValue;
            return name;
        }
    }

    [Serializable]
    private class CandyFontMap
    {
        public string fontName;
        public int atlasWidth;
        public int atlasHeight;
        public int fontSize;
        public int lineHeight;
        public int baseLine;
        public int spaceAdvance;
        public List<CandyGlyph> glyphs;
    }

    [MenuItem("Tools/Candy Bitmap Font/Configure Atlas Slices")]
    public static void ConfigureAtlasSlices()
    {
        string atlasPath = FindAssetByName("CandyBitmapFontAtlas.png");
        string jsonPath = FindAssetByName("CandyBitmapFontMap.json");

        if (string.IsNullOrEmpty(atlasPath) || string.IsNullOrEmpty(jsonPath))
        {
            Debug.LogError("CandyBitmapFontAtlas.png atau CandyBitmapFontMap.json belum ada di project.");
            return;
        }

        string json = File.ReadAllText(jsonPath)
            .Replace("\"char\"", "\"charValue\"")
            .Replace("\"base\"", "\"baseLine\"");

        CandyFontMap map = JsonUtility.FromJson<CandyFontMap>(json);
        if (map == null || map.glyphs == null || map.glyphs.Count == 0)
        {
            Debug.LogError("Gagal membaca CandyBitmapFontMap.json.");
            return;
        }

        // First configure the texture as a Multiple Sprite atlas.
        TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("TextureImporter untuk atlas tidak ditemukan.");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // IMPORTANT: atlas source is 1344x1152. If Unity limits it to 1024,
        // runtime pixel coordinates no longer match the JSON mapping.
        importer.maxTextureSize = 2048;
        importer.npotScale = TextureImporterNPOTScale.None;

        importer.SaveAndReimport();

        // Unity 2021.2+ / Unity 6 compatible Sprite Editor Data Provider API.
        importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;

        var factory = new SpriteDataProviderFactories();
        factory.Init();

        ISpriteEditorDataProvider dataProvider =
            factory.GetSpriteEditorDataProviderFromObject(importer);

        if (dataProvider == null)
        {
            Debug.LogError(
                "Sprite Editor Data Provider tidak tersedia. Pastikan package '2D Sprite' terpasang di Package Manager.");
            return;
        }

        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = new List<SpriteRect>();
        var nameFileIdPairs = new List<SpriteNameFileIdPair>();

        foreach (CandyGlyph glyph in map.glyphs)
        {
            string character = glyph.GetChar();
            string spriteName = ToSpriteName(character);

            // JSON coordinates are top-left based, while SpriteRect uses bottom-left.
            var rect = new Rect(
                glyph.x,
                map.atlasHeight - glyph.y - glyph.height,
                glyph.width,
                glyph.height);

            var spriteRect = new SpriteRect
            {
                name = spriteName,
                spriteID = GUID.Generate(),
                rect = rect,
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = Vector4.zero
            };

            spriteRects.Add(spriteRect);
            nameFileIdPairs.Add(new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID));
        }

        dataProvider.SetSpriteRects(spriteRects.ToArray());

        // Required on modern Unity so sprite names keep stable file IDs.
        ISpriteNameFileIdDataProvider nameProvider =
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();

        if (nameProvider != null)
            nameProvider.SetNameFileIdPairs(nameFileIdPairs);

        dataProvider.Apply();
        importer.SaveAndReimport();
        AssetDatabase.Refresh();

        Debug.Log($"Candy bitmap font berhasil dislice: {spriteRects.Count} karakter.");
    }

    private static string ToSpriteName(string character)
    {
        switch (character)
        {
            case "+": return "plus";
            case "-": return "minus";
            case "=": return "equals";
            default: return character;
        }
    }

    private static string FindAssetByName(string fileName)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        string[] guids = AssetDatabase.FindAssets(fileNameWithoutExtension);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                return path;
        }

        return null;
    }
}
#endif
