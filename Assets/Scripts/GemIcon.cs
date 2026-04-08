using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loads the gem sprite from Resources once and provides a helper
/// to stamp a small gem Image next to any RectTransform.
/// </summary>
public static class GemIcon
{
    private static Sprite _sprite;
    private static bool _loaded;

    public static Sprite Get()
    {
        if (_loaded) return _sprite;
        _loaded = true;

        var tex = Resources.Load<Texture2D>("StoreImages/Gem");
        if (tex != null)
        {
            _sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);
            _sprite.name = "GemIcon";
        }
        return _sprite;
    }

    /// <summary>
    /// Creates a small gem Image as a child of the given parent, anchored
    /// to the left-center. Returns the Image (or null if sprite not found).
    /// </summary>
    public static Image CreateIcon(Transform parent, float size = 36f)
    {
        var sp = Get();
        if (sp == null) return null;

        var go = new GameObject("GemIcon", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        var img = go.AddComponent<Image>();
        img.sprite = sp;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }
}
