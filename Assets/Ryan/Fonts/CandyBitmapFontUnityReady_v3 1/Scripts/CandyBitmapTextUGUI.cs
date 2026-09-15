using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[AddComponentMenu("UI/Candy Bitmap Text")]
public class CandyBitmapTextUGUI : MonoBehaviour
{
    [Serializable]
    public class GlyphInfo
    {
        public string @char;
        public string name;
        public int x;
        public int y;
        public int width;
        public int height;
        public int xOffset;
        public int yOffset;
        public int xAdvance;
    }

    [Serializable]
    public class FontMap
    {
        public string fontName;
        public int atlasWidth;
        public int atlasHeight;
        public int fontSize;
        public int lineHeight;
        public int @base;
        public int spaceAdvance;
        public List<GlyphInfo> glyphs;
    }

    [TextArea]
    [SerializeField] private string text = "SCORE 123";
    [SerializeField] private Texture2D atlasTexture;
    [SerializeField] private TextAsset fontMapJson;
    [SerializeField] private float fontSize = 128f;
    [SerializeField] private float extraLetterSpacing = 0f;
    [SerializeField] private float extraLineSpacing = 0f;
    [SerializeField] private bool forceUppercase = true;

    private readonly Dictionary<char, GlyphInfo> glyphLookup = new Dictionary<char, GlyphInfo>();
    private readonly Dictionary<char, Sprite> spriteCache = new Dictionary<char, Sprite>();
    private FontMap fontMap;
    private RectTransform rectTransform;

    public string Text
    {
        get => text;
        set { text = value; Refresh(); }
    }

    public void Refresh()
    {
        // [MODIFICATION]: Check if the internal Unity C++ object has been destroyed.
        // If yes, abort execution immediately to prevent MissingReferenceException.
        if (this == null) return;

        EnsureLoaded();
        Rebuild();
    }

    private void OnEnable() => Refresh();

    // [MODIFICATION]: Unsubscribe the function from the delayCall queue when the component 
    // is disabled or destroyed. This prevents zombie executions in the Editor.
    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall -= Refresh;
#endif
    }

    // [MODIFICATION]: Modified OnValidate to prevent "SendMessage cannot be called..." warnings.
    // Previously executed directly: private void OnValidate() => Refresh();
    // Changed to use delayCall so Rebuild() executes after Unity's internal OnValidate phase finishes,
    // making RectTransform modifications (SetNativeSize & sizeDelta) safe and warning-free.
    private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall -= Refresh;
        UnityEditor.EditorApplication.delayCall += Refresh;
#else
        Refresh();
#endif
    }

#if UNITY_EDITOR
    // [MODIFICATION]: Update method commented out to prevent Editor CPU performance waste.
    // Previously: if (!Application.isPlaying) Refresh();
    // Executing Refresh() (which loops through UI creation/layout) every frame makes the 
    // Editor extremely heavy. Real-time updates when text is changed in the Inspector 
    // are now safely and fully handled by OnValidate.
    /*
    private void Update()
    {
        if (!Application.isPlaying) Refresh();
    }
    */
#endif

    private void EnsureLoaded()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();

        if (fontMapJson == null || atlasTexture == null)
            return;

        if (fontMap == null || glyphLookup.Count == 0)
        {
            fontMap = JsonUtility.FromJson<FontMap>(fontMapJson.text);
            glyphLookup.Clear();
            spriteCache.Clear();
            if (fontMap != null && fontMap.glyphs != null)
            {
                foreach (var glyph in fontMap.glyphs)
                {
                    if (!string.IsNullOrEmpty(glyph.@char))
                    {
                        glyphLookup[glyph.@char[0]] = glyph;
                    }
                }
            }
        }
    }

    private Sprite GetSprite(char c)
    {
        if (spriteCache.TryGetValue(c, out var sprite))
            return sprite;
        if (!glyphLookup.TryGetValue(c, out var glyph) || atlasTexture == null || fontMap == null)
            return null;

        // JSON mapping uses the original atlas pixel size. Unity may downscale
        // imported textures (for example 1344x1152 -> 1024x1024). Scale the
        // glyph rectangle to the Texture2D's REAL imported size so the rect
        // always stays inside the texture.
        float sx = atlasTexture.width / (float)Mathf.Max(1, fontMap.atlasWidth);
        float sy = atlasTexture.height / (float)Mathf.Max(1, fontMap.atlasHeight);

        var rect = new Rect(
            glyph.x * sx,
            (fontMap.atlasHeight - glyph.y - glyph.height) * sy,
            glyph.width * sx,
            glyph.height * sy);

        // Clamp tiny floating point overshoots at the texture edges.
        rect.x = Mathf.Clamp(rect.x, 0f, atlasTexture.width);
        rect.y = Mathf.Clamp(rect.y, 0f, atlasTexture.height);
        rect.width = Mathf.Clamp(rect.width, 0f, atlasTexture.width - rect.x);
        rect.height = Mathf.Clamp(rect.height, 0f, atlasTexture.height - rect.y);

        if (rect.width <= 0f || rect.height <= 0f)
        {
            Debug.LogError($"Invalid glyph rect for '{c}': {rect} on {atlasTexture.width}x{atlasTexture.height} texture.", this);
            return null;
        }

        sprite = Sprite.Create(atlasTexture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        sprite.name = glyph.name;
        spriteCache[c] = sprite;
        return sprite;
    }

    private void Rebuild()
    {
        if (fontMapJson == null || atlasTexture == null)
            return;
        if (fontMap == null || glyphLookup.Count == 0)
            EnsureLoaded();
        if (fontMap == null)
            return;

        string finalText = forceUppercase ? (text ?? string.Empty).ToUpperInvariant() : (text ?? string.Empty);
        float scale = fontSize / Mathf.Max(1, fontMap.fontSize);
        int childIndex = 0;
        float penX = 0f;
        float penY = 0f;
        float maxWidth = 0f;
        float lineHeight = (fontMap.lineHeight + extraLineSpacing) * scale;

        var lines = finalText.Replace("\r", string.Empty).Split('\n');
        for (int li = 0; li < lines.Length; li++)
        {
            string line = lines[li];
            penX = 0f;
            float lineWidth = 0f;
            foreach (char c in line)
            {
                if (c == ' ')
                {
                    lineWidth += (fontMap.spaceAdvance + extraLetterSpacing) * scale;
                    continue;
                }
                if (!glyphLookup.TryGetValue(c, out var glyph))
                    continue;
                lineWidth += (glyph.xAdvance + extraLetterSpacing) * scale;
            }
            maxWidth = Mathf.Max(maxWidth, lineWidth);
        }

        penY = 0f;
        foreach (int i in System.Linq.Enumerable.Range(0, lines.Length))
        {
            string line = lines[i];
            penX = 0f;
            foreach (char c in line)
            {
                if (c == ' ')
                {
                    penX += (fontMap.spaceAdvance + extraLetterSpacing) * scale;
                    continue;
                }
                if (!glyphLookup.TryGetValue(c, out var glyph))
                    continue;
                var sprite = GetSprite(c);
                if (sprite == null)
                    continue;

                RectTransform child;
                Image image;
                if (childIndex < transform.childCount)
                {
                    child = transform.GetChild(childIndex) as RectTransform;
                    image = child.GetComponent<Image>();
                    if (image == null) image = child.gameObject.AddComponent<Image>();
                }
                else
                {
                    var go = new GameObject("Glyph_" + c, typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(transform, false);
                    child = go.GetComponent<RectTransform>();
                    image = go.GetComponent<Image>();
                    image.raycastTarget = false;
                }

                image.sprite = sprite;
                image.SetNativeSize();
                child.anchorMin = new Vector2(0f, 1f);
                child.anchorMax = new Vector2(0f, 1f);
                child.pivot = new Vector2(0f, 1f);
                child.sizeDelta = new Vector2(glyph.width * scale, glyph.height * scale);
                child.anchoredPosition = new Vector2(penX, -penY - glyph.yOffset * scale);
                child.gameObject.SetActive(true);

                penX += (glyph.xAdvance + extraLetterSpacing) * scale;
                childIndex++;
            }
            penY += lineHeight;
        }

        for (int i = childIndex; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        rectTransform.sizeDelta = new Vector2(maxWidth, Mathf.Max(lineHeight, penY));
    }
}