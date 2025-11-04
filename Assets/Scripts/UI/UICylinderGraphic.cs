using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UICylinderGraphic : MaskableGraphic
{
    public enum CylinderLayerMode { Full, BackOnly, TopRimOnly, BottomShadowOnly }

    [Header("Layer Mode")]
    [Tooltip("Use BackOnly for the rear body, TopRimOnly for the front rim. Combine with a RectMask2D between them to place content 'inside'.")]
    public CylinderLayerMode layerMode = CylinderLayerMode.Full;

    [Header("Custom Rendering")]
    [Tooltip("Optional. If set, this material will be used (with stencil/mask modifications applied by UI).")]
    [SerializeField] private Material overrideMaterial;

    [Tooltip("Optional texture passed as mainTexture to the material/shader (_MainTex).")]
    [SerializeField] private Texture _mainTexture;

    public Texture MainTexture
    {
        get => _mainTexture;
        set { _mainTexture = value; SetVerticesDirty(); SetMaterialDirty(); }
    }

    public override Texture mainTexture => _mainTexture != null ? _mainTexture : s_WhiteTexture;

    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        var mat = overrideMaterial != null ? overrideMaterial : baseMaterial;
        return base.GetModifiedMaterial(mat);
    }

    [Header("Geometry")]
    [Tooltip("Approx cylinder height in px (uses RectTransform height by default).")]
    public float cylinderHeight = 220f;

    [Tooltip("Ellipse x-radius in px (RectTransform width should be ~2×radius).")]
    public float radius = 90f;

    [Range(-35f, 35f)]
    [Tooltip("Visual squash of the top/bottom ellipse to fake tilt.")]
    public float tiltDegrees = 20f;

    [Header("Material Look")]
    [Range(0f, 360f)] public float hue = 210f;
    [Range(0f, 100f)] public float saturation = 70f;
    [Range(0f, 100f)] public float lightness = 50f;
    [Range(0f, 1f)] public float gloss = 0.35f;
    [Range(0f, 1f)] public float shadow = 0.35f;

    [Header("Tank Fill (0..1)")]
    [Range(0f, 1f)]
    public float fill = 1f;

    [Header("Quality")]
    [Range(16, 128)]
    public int ellipseSegments = 64;

    // ---------- HSL utility ----------
    Color Hsl(float h, float s, float l, float a = 0.5f)
    {
        h = Mathf.Repeat(h, 360f) / 360f;
        s = Mathf.Clamp01(s / 100f);
        l = Mathf.Clamp01(l / 100f);

        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;

        float r = Hue2Rgb(p, q, h + 1f / 3f);
        float g = Hue2Rgb(p, q, h);
        float b = Hue2Rgb(p, q, h - 1f / 3f);
        return new Color(r, g, b, a);
    }
    float Hue2Rgb(float p, float q, float t)
    {
        t = (t + 1f) % 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    // ---------- Mesh drawing ----------
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = rectTransform.rect;
        float w = r.width;
        float h = Mathf.Max(cylinderHeight, r.height);
        float cx = r.xMin + w * 0.5f;

        float topY = r.yMax;                   // top edge of rect
        float sideTop = topY - TopEllipseHeight();
        float sideBottom = topY - h * Mathf.Clamp01(fill);
        float sideFullBottom = topY - h;

        float ellipseYScale = 1f - Mathf.Min(0.8f, Mathf.Abs(tiltDegrees) / 100f);
        float rx = Mathf.Max(8f, radius);
        float ry = Mathf.Max(6f, radius * 0.6f) * ellipseYScale;

        float l = lightness;
        float s = saturation;
        float edgeShadow = Mathf.Clamp(l - 18f * (shadow + 0.3f), 0f, 100f);
        float centerLight = Mathf.Clamp(l + 12f * gloss, 0f, 100f);

        Color colEdge = Hsl(hue, s, edgeShadow);
        Color colMid = Hsl(hue, s, l);
        Color colHi = Hsl(hue, s, centerLight);

        bool drawSide = layerMode == CylinderLayerMode.Full || layerMode == CylinderLayerMode.BackOnly;
        bool drawTopRim = layerMode == CylinderLayerMode.Full || layerMode == CylinderLayerMode.TopRimOnly;
        bool drawBottomShadow = layerMode == CylinderLayerMode.Full || layerMode == CylinderLayerMode.BottomShadowOnly;

        // 1) Side / back body (draw only in Back or Full)
        if (drawSide)
        {
            int slices = 4;
            float left = cx - rx;
            float right = cx + rx;
            float top = sideTop;
            float bottom = sideBottom;

            for (int i = 0; i < slices; i++)
            {
                float t0 = (float)i / slices;
                float t1 = (float)(i + 1) / slices;
                float x0 = Mathf.Lerp(left, right, t0);
                float x1 = Mathf.Lerp(left, right, t1);

                Color c0 = LerpWrap(colEdge, colHi, colMid, t0);
                Color c1 = LerpWrap(colEdge, colHi, colMid, t1);

                AddQuad(vh,
                    new Vector2(x0, bottom), new Vector2(x1, bottom),
                    new Vector2(x1, top), new Vector2(x0, top),
                    c0, c1, c1, c0);
            }
        }

        // 2) Top ellipse rim (front piece)
        if (drawTopRim)
        {
            AddEllipse(vh, cx, sideTop, rx, ry,
                Hsl(hue, s, Mathf.Clamp(l + 6f * gloss, 0, 100)),
                Hsl(hue, s, l),
                Hsl(hue, s, Mathf.Clamp(l - 8f * shadow, 0, 100)),
                1f);
        }

        // 3) Bottom shadow ellipse
        if (drawBottomShadow)
        {
            float shadowY = Mathf.Lerp(sideFullBottom - ry * 0.6f, sideBottom - ry * 0.25f, Mathf.Clamp01(fill));
            AddShadowEllipse(vh, cx, shadowY, rx * 0.95f, ry, 0.25f);
        }
    }

    float TopEllipseHeight()
    {
        float ry = Mathf.Max(6f, radius * 0.6f);
        float ellipseYScale = 1f - Mathf.Min(0.8f, Mathf.Abs(tiltDegrees) / 100f);
        return Mathf.Max(14f, ry * ellipseYScale);
    }

    Color LerpWrap(Color edge, Color hi, Color mid, float t)
    {
        if (t < 0.45f) return Color.Lerp(edge, hi, t / 0.45f);
        if (t < 0.50f) return Color.Lerp(hi, mid, (t - 0.45f) / 0.05f);
        if (t < 0.55f) return Color.Lerp(mid, hi, (t - 0.50f) / 0.05f);
        return Color.Lerp(hi, edge, (t - 0.55f) / 0.45f);
    }

    void AddQuad(VertexHelper vh, Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl,
        Color cbl, Color cbr, Color ctr, Color ctl)
    {
        int start = vh.currentVertCount;
        vh.AddVert(bl, cbl, Vector2.zero);
        vh.AddVert(br, cbr, Vector2.zero);
        vh.AddVert(tr, ctr, Vector2.zero);
        vh.AddVert(tl, ctl, Vector2.zero);
        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }

    void AddEllipse(VertexHelper vh, float cx, float cy, float rx, float ry,
        Color centerCol, Color midCol, Color rimCol, float alpha = 1f)
    {
        int start = vh.currentVertCount;
        vh.AddVert(new Vector2(cx, cy), centerCol, Vector2.zero);

        for (int i = 0; i <= ellipseSegments; i++)
        {
            float t = (float)i / ellipseSegments;
            float ang = t * Mathf.PI * 2f;
            float x = cx + Mathf.Cos(ang) * rx;
            float y = cy + Mathf.Sin(ang) * ry;

            float ringT = Mathf.Abs(Mathf.Sin(ang)) * 0.4f + 0.6f;
            Color c = Color.Lerp(midCol, rimCol, ringT);
            c.a *= alpha;
            vh.AddVert(new Vector2(x, y), c, Vector2.zero);
        }

        for (int i = 0; i < ellipseSegments; i++)
        {
            vh.AddTriangle(start, start + i + 1, start + i + 2);
        }
    }

    void AddShadowEllipse(VertexHelper vh, float cx, float cy, float rx, float ry, float strength)
    {
        int start = vh.currentVertCount;

        vh.AddVert(new Vector2(cx, cy), new Color(0f, 0f, 0f, Mathf.Clamp01(strength * 0.15f)), Vector2.zero);

        for (int i = 0; i <= ellipseSegments; i++)
        {
            float t = (float)i / ellipseSegments;
            float ang = t * Mathf.PI * 2f;
            float x = cx + Mathf.Cos(ang) * rx;
            float y = cy + Mathf.Sin(ang) * ry;

            Color c = new Color(0f, 0f, 0f, Mathf.Clamp01(strength * 0.35f));
            vh.AddVert(new Vector2(x, y), c, Vector2.zero);
        }

        for (int i = 0; i < ellipseSegments; i++)
        {
            vh.AddTriangle(start, start + i + 1, start + i + 2);
        }
    }

    // ---------- Mask fitting helper ----------
    /// <summary>
    /// Fits a RectTransform (e.g., parent with RectMask2D) to the top opening (ellipse) in local space.
    /// Call from context menu or at runtime.
    /// </summary>
    public void FitMaskToOpening(RectTransform mask)
    {
        if (mask == null) return;

        Rect r = rectTransform.rect;
        float w = r.width;
        float cx = r.xMin + w * 0.5f;

        float rx = Mathf.Max(8f, radius);
        float ellipseYScale = 1f - Mathf.Min(0.8f, Mathf.Abs(tiltDegrees) / 100f);
        float ry = Mathf.Max(6f, radius * 0.6f) * ellipseYScale;

        float sideTop = r.yMax - TopEllipseHeight();

        // Set size to ellipse bounds (rectangular mask that matches ellipse box)
        Vector2 size = new Vector2(rx * 2f, ry * 2f);
        mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        // Position the mask so its center matches the ellipse center
        Vector2 centerLocal = new Vector2(cx, sideTop);
        // Because RectTransform.anchoredPosition is relative to pivot, convert to anchored space:
        Vector2 pivotOffset = new Vector2(
            Mathf.Lerp(r.xMin, r.xMax, mask.pivot.x),
            Mathf.Lerp(r.yMin, r.yMax, mask.pivot.y)
        );
        // But easier: make mask share anchors & pivot with this element, then assign localPosition:
        mask.anchorMin = rectTransform.anchorMin;
        mask.anchorMax = rectTransform.anchorMax;
        mask.pivot = rectTransform.pivot;
        mask.localRotation = Quaternion.identity;
        mask.localScale = Vector3.one;
        mask.anchoredPosition = centerLocal - new Vector2(
            Mathf.Lerp(r.xMin, r.xMax, rectTransform.pivot.x),
            Mathf.Lerp(r.yMin, r.yMax, rectTransform.pivot.y)
        );
    }

#if UNITY_EDITOR
    [ContextMenu("UICylinderGraphic/Fit Mask To Opening (select RectTransform)")]
    void FitMaskContext()
    {
#if UNITY_EDITOR
        var go = UnityEditor.Selection.activeTransform as RectTransform;
        if (go != null) FitMaskToOpening(go);
#endif
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
        SetMaterialDirty();
    }
#endif
}
