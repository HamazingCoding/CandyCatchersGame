using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class StoreUI : MonoBehaviour
{
    static readonly Color BgColor      = new Color(0.12f, 0.08f, 0.22f, 1f);
    static readonly Color CardBg       = new Color(0.20f, 0.15f, 0.35f, 1f);
    static readonly Color CardBorder   = new Color(0.45f, 0.35f, 0.70f, 0.7f);
    static readonly Color GemBarBg     = new Color(0.10f, 0.08f, 0.20f, 0.95f);
    static readonly Color TextWhite    = Color.white;
    static readonly Color TextGold     = new Color(1f, 0.85f, 0.30f, 1f);
    static readonly Color GemCyan      = new Color(0.55f, 0.85f, 1f, 1f);
    static readonly Color DescGray     = new Color(0.78f, 0.75f, 0.90f, 1f);
    static readonly Color BtnGreen     = new Color(0.15f, 0.72f, 0.35f, 1f);
    static readonly Color BtnGray      = new Color(0.40f, 0.40f, 0.40f, 0.85f);
    static readonly Color BtnOrange    = new Color(0.85f, 0.38f, 0.15f, 1f);
    static readonly Color AccentPink   = new Color(1f, 0.40f, 0.60f, 1f);

    TMP_Text _gemBalanceText;
    readonly Dictionary<PowerupManager.PowerupType, TMP_Text> _qtyTexts    = new();
    readonly Dictionary<PowerupManager.PowerupType, Button>   _buyButtons  = new();
    readonly Dictionary<PowerupManager.PowerupType, TMP_Text> _buyBtnTexts = new();
    readonly Dictionary<string, Sprite> _sprites = new();

    void Start()
    {
        LoadImages();
        Build();
    }

    // ── Image loading ─────────────────────────────────────────
    void LoadImages()
    {
        foreach (var t in Resources.LoadAll<Texture2D>("StoreImages"))
        {
            _sprites[t.name] = Sprite.Create(t,
                new Rect(0, 0, t.width, t.height),
                new Vector2(0.5f, 0.5f), 100f);
        }
        Debug.Log($"[StoreUI] Loaded {_sprites.Count} images: {string.Join(", ", _sprites.Keys)}");
    }

    Sprite GetSprite(string name)
    {
        _sprites.TryGetValue(name, out var sp);
        return sp;
    }

    Sprite GetGemSprite()
    {
        var sp = GetSprite("Gem");
        if (sp != null) return sp;
        return GetSprite("GemsStore");
    }

    // ── Build ─────────────────────────────────────────────────
    void Build()
    {
        var rootGO = new GameObject("StoreCanvas");
        var canvas = rootGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = rootGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        rootGO.AddComponent<GraphicRaycaster>();

        // Background
        var bg = AddImage(rootGO.transform, "BG", BgColor);
        StretchFull(bg.rectTransform);

        // Main column
        var col = AddRT(rootGO.transform, "Column");
        StretchFull(col);
        var vlg = col.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(32, 32, 40, 32);
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Header
        var header = AddTMP(col, "Header", "STORE", 64, TextGold);
        header.fontStyle = FontStyles.Bold | FontStyles.Italic;
        header.alignment = TextAlignmentOptions.Center;
        AddLE(header.rectTransform, prefH: 80);

        // Gem bar
        BuildGemBar(col);

        // Scroll area with cards
        var content = BuildScroll(col);
        foreach (PowerupManager.PowerupType t in System.Enum.GetValues(typeof(PowerupManager.PowerupType)))
            BuildCard(content, t);

        // Back button
        BuildBackBtn(col);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(col);
        RefreshAll();
    }

    // ── Gem bar ───────────────────────────────────────────────
    void BuildGemBar(RectTransform parent)
    {
        var bar = AddImage(parent, "GemBar", GemBarBg);
        AddLE(bar.rectTransform, prefH: 80);

        var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(24, 24, 8, 8);
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var gemSp = GetGemSprite();
        if (gemSp != null)
        {
            var icon = AddImage(bar.transform, "GemIcon", Color.white);
            icon.sprite = gemSp;
            icon.preserveAspect = true;
            AddLE(icon.rectTransform, prefW: 56, prefH: 56);
        }

        _gemBalanceText = AddTMP(bar.transform, "Num", "0", 48, GemCyan);
        _gemBalanceText.fontStyle = FontStyles.Bold;
        _gemBalanceText.alignment = TextAlignmentOptions.Center;
        AddLE(_gemBalanceText.rectTransform, prefW: 200, prefH: 56);

        var lbl = AddTMP(bar.transform, "Lbl", "Gems", 32, DescGray);
        AddLE(lbl.rectTransform, prefW: 100, prefH: 56);
    }

    // ── Scroll area ───────────────────────────────────────────
    RectTransform BuildScroll(RectTransform parent)
    {
        var scrollRT = AddRT(parent, "Scroll");
        AddLE(scrollRT, flexH: 1, minH: 200);

        var sr = scrollRT.gameObject.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 40f;

        scrollRT.gameObject.AddComponent<Image>().color = Color.clear;

        var vp = AddRT(scrollRT, "VP");
        StretchFull(vp);
        vp.gameObject.AddComponent<RectMask2D>();

        var content = AddRT(vp, "Content");
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        var csf = content.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(0, 0, 8, 8);
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        sr.viewport = vp;
        sr.content = content;
        return content;
    }

    // ── Item card ─────────────────────────────────────────────
    // Anchor-based layout inside a fixed-height card:
    //   [Icon 20%] [Info 50%] [BuyBtn 25%] with gaps
    void BuildCard(RectTransform parent, PowerupManager.PowerupType type)
    {
        var card = AddImage(parent, "Card_" + type, CardBg);
        AddLE(card.rectTransform, prefH: 200, minH: 180);

        var outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = CardBorder;
        outline.effectDistance = new Vector2(2, -2);

        float pad = 12f;
        float cardW = 1f; // normalized

        // Icon box: left 0% to 18%
        float iconR = 0.18f;
        var iconBox = AddImage(card.transform, "IconBox", new Color(0.14f, 0.11f, 0.26f, 0.9f));
        AnchorRect(iconBox.rectTransform, 0f, 0f, iconR, 1f, pad, pad, -pad / 2, -pad);

        string imgName = PowerupManager.GetImageName(type);
        var sprite = GetSprite(imgName);
        if (sprite != null)
        {
            var iconImg = AddImage(iconBox.transform, "Img", Color.white);
            iconImg.sprite = sprite;
            iconImg.preserveAspect = true;
            StretchWithInset(iconImg.rectTransform, 6);
        }

        // Info: 20% to 72%
        float infoL = 0.20f;
        float infoR = 0.72f;

        var nameRT = AddRT(card.transform, "Name");
        AnchorRect(nameRT, infoL, 0.62f, infoR, 1f, 0, 0, 0, -pad);
        var nameTmp = nameRT.gameObject.AddComponent<TextMeshProUGUI>();
        nameTmp.text = PowerupManager.GetDisplayName(type);
        nameTmp.fontSize = 34;
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.color = TextWhite;
        nameTmp.alignment = TextAlignmentOptions.Left;
        nameTmp.overflowMode = TextOverflowModes.Ellipsis;
        nameTmp.textWrappingMode = TextWrappingModes.NoWrap;

        var descRT = AddRT(card.transform, "Desc");
        AnchorRect(descRT, infoL, 0.28f, infoR, 0.62f, 0, 0, 0, 0);
        var descTmp = descRT.gameObject.AddComponent<TextMeshProUGUI>();
        descTmp.text = PowerupManager.GetDescription(type);
        descTmp.fontSize = 22;
        descTmp.color = DescGray;
        descTmp.alignment = TextAlignmentOptions.Left;
        descTmp.textWrappingMode = TextWrappingModes.Normal;
        descTmp.overflowMode = TextOverflowModes.Ellipsis;

        var qtyRT = AddRT(card.transform, "Qty");
        AnchorRect(qtyRT, infoL, 0f, infoR, 0.28f, 0, pad, 0, 0);
        var qtyTmp = qtyRT.gameObject.AddComponent<TextMeshProUGUI>();
        qtyTmp.text = "Owned: 0";
        qtyTmp.fontSize = 22;
        qtyTmp.color = GemCyan;
        qtyTmp.alignment = TextAlignmentOptions.Left;
        _qtyTexts[type] = qtyTmp;

        // Buy button: right 74% to 98%
        float btnL = 0.74f;
        float btnR = 0.98f;
        var btnBg = AddImage(card.transform, "BuyBtn", BtnGreen);
        AnchorRect(btnBg.rectTransform, btnL, 0.2f, btnR, 0.8f, 0, 0, 0, 0);

        var btn = btnBg.gameObject.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        _buyButtons[type] = btn;

        var btnTmp = AddTMP(btnBg.transform, "BtnTxt", PriceText(type), 24, TextWhite);
        btnTmp.fontStyle = FontStyles.Bold;
        btnTmp.alignment = TextAlignmentOptions.Center;
        StretchFull(btnTmp.rectTransform);
        _buyBtnTexts[type] = btnTmp;

        var captured = type;
        btn.onClick.AddListener(() => OnBuy(captured));
    }

    // ── Back button ───────────────────────────────────────────
    void BuildBackBtn(RectTransform parent)
    {
        var bar = AddImage(parent, "BackBtn", BtnOrange);
        AddLE(bar.rectTransform, prefH: 100, minH: 80);

        var btn = bar.gameObject.AddComponent<Button>();
        btn.targetGraphic = bar;

        var tmp = AddTMP(bar.transform, "Txt", "BACK", 44, TextWhite);
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        StretchFull(tmp.rectTransform);

        btn.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene("Main Menu"); });
    }

    // ── Buy / refresh ─────────────────────────────────────────
    void OnBuy(PowerupManager.PowerupType type)
    {
        if (PowerupManager.Purchase(type))
            StartCoroutine(Flash(type, "BOUGHT!", TextGold));
        else
            StartCoroutine(Flash(type, "NO GEMS!", AccentPink));
        RefreshAll();
    }

    System.Collections.IEnumerator Flash(PowerupManager.PowerupType type, string msg, Color c)
    {
        if (!_buyBtnTexts.TryGetValue(type, out var txt)) yield break;
        txt.text = msg;
        txt.color = c;
        yield return new WaitForSecondsRealtime(1f);
        txt.text = PriceText(type);
        txt.color = TextWhite;
    }

    static string PriceText(PowerupManager.PowerupType type)
    {
        return PowerupManager.GetPrice(type) + " Gems";
    }

    void RefreshAll()
    {
        int gems = SaveManager.GetGems();
        if (_gemBalanceText != null) _gemBalanceText.text = gems.ToString();

        foreach (PowerupManager.PowerupType t in System.Enum.GetValues(typeof(PowerupManager.PowerupType)))
        {
            if (_qtyTexts.ContainsKey(t))
                _qtyTexts[t].text = "Owned: " + PowerupManager.GetQuantity(t);

            if (_buyButtons.ContainsKey(t))
                _buyButtons[t].GetComponent<Image>().color =
                    SaveManager.CanAfford(PowerupManager.GetPrice(t)) ? BtnGreen : BtnGray;

            if (_buyBtnTexts.ContainsKey(t))
            {
                _buyBtnTexts[t].text = PriceText(t);
                _buyBtnTexts[t].color = TextWhite;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════

    static RectTransform AddRT(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static Image AddImage(Transform parent, string name, Color color)
    {
        var rt = AddRT(parent, name);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI AddTMP(Transform parent, string name, string text, float size, Color color)
    {
        var rt = AddRT(parent, name);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }

    static LayoutElement AddLE(RectTransform rt,
        float prefW = -1, float prefH = -1,
        float minW = -1, float minH = -1,
        float flexW = -1, float flexH = -1)
    {
        var le = rt.gameObject.AddComponent<LayoutElement>();
        if (prefW >= 0) le.preferredWidth  = prefW;
        if (prefH >= 0) le.preferredHeight = prefH;
        if (minW  >= 0) le.minWidth        = minW;
        if (minH  >= 0) le.minHeight       = minH;
        if (flexW >= 0) le.flexibleWidth   = flexW;
        if (flexH >= 0) le.flexibleHeight  = flexH;
        return le;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void StretchWithInset(RectTransform rt, float inset)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    static void AnchorRect(RectTransform rt,
        float aMinX, float aMinY, float aMaxX, float aMaxY,
        float offL, float offB, float offR, float offT)
    {
        rt.anchorMin = new Vector2(aMinX, aMinY);
        rt.anchorMax = new Vector2(aMaxX, aMaxY);
        rt.offsetMin = new Vector2(offL, offB);
        rt.offsetMax = new Vector2(offR, offT);
    }
}
