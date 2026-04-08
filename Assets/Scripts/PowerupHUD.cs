using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// In-gameplay HUD for powerups. Shows buttons to activate owned powerups,
/// active timers, and visual feedback. Attaches to a Canvas in the game scene.
/// Works with GameManager for actual effects.
/// </summary>
public class PowerupHUD : MonoBehaviour
{
    // -------------------------------------------------------
    // Static access for GameManager
    // -------------------------------------------------------
    public static PowerupHUD Instance { get; private set; }

    // Active powerup state
    public bool IsBoosterActive { get; private set; }
    public bool IsShieldActive { get; private set; }
    public bool IsLightningActive { get; private set; }

    public float ScoreMultiplier => IsBoosterActive ? 2f : 1f;

    // Durations
    private const float BoosterDuration = 20f;
    private const float ShieldDuration = 10f;
    private const float LightningDuration = 15f;

    // UI refs
    private Canvas _canvas;
    private RectTransform _hudPanel;
    private Dictionary<PowerupManager.PowerupType, GameObject> _buttonGOs = new Dictionary<PowerupManager.PowerupType, GameObject>();
    private Dictionary<PowerupManager.PowerupType, TMP_Text> _btnQtyTexts = new Dictionary<PowerupManager.PowerupType, TMP_Text>();
    private Dictionary<PowerupManager.PowerupType, Image> _btnImages = new Dictionary<PowerupManager.PowerupType, Image>();

    // Active timer displays
    private GameObject _activeTimersPanel;
    private Dictionary<PowerupManager.PowerupType, GameObject> _timerDisplays = new Dictionary<PowerupManager.PowerupType, GameObject>();
    private Dictionary<PowerupManager.PowerupType, TMP_Text> _timerTexts = new Dictionary<PowerupManager.PowerupType, TMP_Text>();
    private Dictionary<PowerupManager.PowerupType, Image> _timerFills = new Dictionary<PowerupManager.PowerupType, Image>();

    // Loaded sprites for powerup icons
    private Dictionary<string, Sprite> _iconSprites = new Dictionary<string, Sprite>();

    // Colors
    private static readonly Color BoosterColor = new Color(1f, 0.82f, 0.15f, 0.9f);
    private static readonly Color LightningColor = new Color(0.3f, 0.75f, 1f, 0.9f);
    private static readonly Color ShieldColor = new Color(0.2f, 0.5f, 1f, 0.9f);
    private static readonly Color HeartColor = new Color(1f, 0.3f, 0.5f, 0.9f);
    private static readonly Color DisabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        LoadIconSprites();
        BuildHUD();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // -------------------------------------------------------
    // Load powerup icon sprites from Resources
    // -------------------------------------------------------
    void LoadIconSprites()
    {
        string[] names = { "Booster", "Lightning", "Shield", "Heart" };
        foreach (var n in names)
        {
            var tex = Resources.Load<Texture2D>("StoreImages/" + n);
            if (tex != null)
            {
                var sp = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                sp.name = n;
                _iconSprites[n] = sp;
            }
        }
    }

    Sprite GetIconSprite(PowerupManager.PowerupType type)
    {
        string name = PowerupManager.GetImageName(type);
        _iconSprites.TryGetValue(name, out var sp);
        return sp;
    }

    // -------------------------------------------------------
    // Build the HUD
    // -------------------------------------------------------
    void BuildHUD()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            var canvasGO = new GameObject("PowerupHUDCanvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 50;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            transform.SetParent(canvasGO.transform, false);
        }

        // --- Powerup buttons panel (bottom-right) ---
        _hudPanel = CreateRT("PowerupButtons", _canvas.transform);
        _hudPanel.anchorMin = new Vector2(1, 0);
        _hudPanel.anchorMax = new Vector2(1, 0);
        _hudPanel.pivot = new Vector2(1, 0);
        _hudPanel.anchoredPosition = new Vector2(-15, 180);
        _hudPanel.sizeDelta = new Vector2(80, 400);

        var vlg = _hudPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.LowerCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        CreatePowerupButton(PowerupManager.PowerupType.Booster, "2X", BoosterColor);
        CreatePowerupButton(PowerupManager.PowerupType.Lightning, "LTN", LightningColor);
        CreatePowerupButton(PowerupManager.PowerupType.Shield, "SHD", ShieldColor);
        CreatePowerupButton(PowerupManager.PowerupType.Heart, "HP", HeartColor);

        // --- Active timers panel (top-center) ---
        _activeTimersPanel = new GameObject("ActiveTimers", typeof(RectTransform));
        _activeTimersPanel.transform.SetParent(_canvas.transform, false);
        var timersRT = _activeTimersPanel.GetComponent<RectTransform>();
        timersRT.anchorMin = new Vector2(0.5f, 1);
        timersRT.anchorMax = new Vector2(0.5f, 1);
        timersRT.pivot = new Vector2(0.5f, 1);
        timersRT.anchoredPosition = new Vector2(0, -120);
        timersRT.sizeDelta = new Vector2(600, 50);

        var timersHLG = _activeTimersPanel.AddComponent<HorizontalLayoutGroup>();
        timersHLG.spacing = 15;
        timersHLG.childAlignment = TextAnchor.MiddleCenter;
        timersHLG.childControlWidth = false;
        timersHLG.childControlHeight = true;
        timersHLG.childForceExpandWidth = false;

        // Heart button is not clickable -- it's auto-use only
        if (_buttonGOs.ContainsKey(PowerupManager.PowerupType.Heart))
        {
            var heartBtn = _buttonGOs[PowerupManager.PowerupType.Heart].GetComponent<Button>();
            if (heartBtn != null) heartBtn.interactable = false;
        }

        RefreshQuantities();
    }

    void CreatePowerupButton(PowerupManager.PowerupType type, string fallbackLabel, Color color)
    {
        var btnGO = new GameObject("Btn_" + type.ToString(), typeof(RectTransform));
        btnGO.transform.SetParent(_hudPanel, false);
        var le = btnGO.AddComponent<LayoutElement>();
        le.preferredHeight = 80;
        le.preferredWidth = 72;

        var img = btnGO.AddComponent<Image>();
        img.color = color;
        _btnImages[type] = img;

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;

        // Icon area (top 65%)
        var iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(btnGO.transform, false);
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.1f, 0.35f);
        iconRT.anchorMax = new Vector2(0.9f, 0.95f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        Sprite iconSprite = GetIconSprite(type);
        if (iconSprite != null)
        {
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
        }
        else
        {
            var iconText = iconGO.AddComponent<TextMeshProUGUI>();
            iconText.text = fallbackLabel;
            iconText.fontSize = 24;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.color = Color.white;
            iconText.fontStyle = FontStyles.Bold;
        }

        // Quantity text (bottom 35%)
        var qtyGO = new GameObject("Qty", typeof(RectTransform));
        qtyGO.transform.SetParent(btnGO.transform, false);
        var qtyRT = qtyGO.GetComponent<RectTransform>();
        qtyRT.anchorMin = new Vector2(0, 0);
        qtyRT.anchorMax = new Vector2(1, 0.35f);
        qtyRT.offsetMin = Vector2.zero;
        qtyRT.offsetMax = Vector2.zero;
        var qtyText = qtyGO.AddComponent<TextMeshProUGUI>();
        qtyText.text = "x0";
        qtyText.fontSize = 18;
        qtyText.alignment = TextAlignmentOptions.Center;
        qtyText.color = Color.white;
        _btnQtyTexts[type] = qtyText;

        _buttonGOs[type] = btnGO;

        PowerupManager.PowerupType capturedType = type;
        btn.onClick.AddListener(() => OnActivate(capturedType));
    }

    // -------------------------------------------------------
    // Activate Powerup
    // -------------------------------------------------------
    void OnActivate(PowerupManager.PowerupType type)
    {
        if (type == PowerupManager.PowerupType.Heart) return;

        if (type == PowerupManager.PowerupType.Booster && IsBoosterActive) return;
        if (type == PowerupManager.PowerupType.Lightning && IsLightningActive) return;
        if (type == PowerupManager.PowerupType.Shield && IsShieldActive) return;

        if (!PowerupManager.HasAny(type))
        {
            Debug.Log("[PowerupHUD] No " + type + " in inventory");
            return;
        }

        if (!PowerupManager.UseOne(type))
            return;

        RefreshQuantities();

        switch (type)
        {
            case PowerupManager.PowerupType.Booster:
                StartCoroutine(BoosterRoutine());
                break;
            case PowerupManager.PowerupType.Lightning:
                StartCoroutine(LightningRoutine());
                break;
            case PowerupManager.PowerupType.Shield:
                StartCoroutine(ShieldRoutine());
                break;
        }
    }

    // -------------------------------------------------------
    // Powerup Coroutines
    // -------------------------------------------------------
    IEnumerator BoosterRoutine()
    {
        IsBoosterActive = true;
        ShowTimer(PowerupManager.PowerupType.Booster, "2X SCORE", BoosterColor, BoosterDuration);
        Debug.Log("[PowerupHUD] Booster ACTIVATED — 2x score for " + BoosterDuration + "s");

        float remaining = BoosterDuration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            UpdateTimer(PowerupManager.PowerupType.Booster, remaining, BoosterDuration);
            yield return null;
        }

        IsBoosterActive = false;
        HideTimer(PowerupManager.PowerupType.Booster);
        Debug.Log("[PowerupHUD] Booster EXPIRED");
    }

    IEnumerator LightningRoutine()
    {
        IsLightningActive = true;
        ShowTimer(PowerupManager.PowerupType.Lightning, "SPEED", LightningColor, LightningDuration);
        Debug.Log("[PowerupHUD] Lightning ACTIVATED — speed boost for " + LightningDuration + "s");

        var player = FindFirstObjectByType<PlayerController>();
        float originalSpeed = 6f;
        float originalDrag = 0f;
        if (player != null)
        {
            originalSpeed = player.moveSpeed;
            originalDrag = player.dragSpeed;
            player.moveSpeed = originalSpeed * 2f;
            player.dragSpeed = originalDrag * 1.5f;
        }

        float remaining = LightningDuration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            UpdateTimer(PowerupManager.PowerupType.Lightning, remaining, LightningDuration);
            yield return null;
        }

        if (player != null)
        {
            player.moveSpeed = originalSpeed;
            player.dragSpeed = originalDrag;
        }

        IsLightningActive = false;
        HideTimer(PowerupManager.PowerupType.Lightning);
        Debug.Log("[PowerupHUD] Lightning EXPIRED");
    }

    IEnumerator ShieldRoutine()
    {
        IsShieldActive = true;
        ShowTimer(PowerupManager.PowerupType.Shield, "SHIELD", ShieldColor, ShieldDuration);
        Debug.Log("[PowerupHUD] Shield ACTIVATED — trick immunity for " + ShieldDuration + "s");

        float remaining = ShieldDuration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            UpdateTimer(PowerupManager.PowerupType.Shield, remaining, ShieldDuration);
            yield return null;
        }

        IsShieldActive = false;
        HideTimer(PowerupManager.PowerupType.Shield);
        Debug.Log("[PowerupHUD] Shield EXPIRED");
    }

    // -------------------------------------------------------
    // Heart -- called by GameManager on death
    // -------------------------------------------------------
    public bool TryAutoReviveWithHeart()
    {
        if (!PowerupManager.HasAny(PowerupManager.PowerupType.Heart))
            return false;

        PowerupManager.UseOne(PowerupManager.PowerupType.Heart);
        RefreshQuantities();
        Debug.Log("[PowerupHUD] Heart AUTO-REVIVE used!");
        StartCoroutine(FlashHeartEffect());
        return true;
    }

    IEnumerator FlashHeartEffect()
    {
        if (_btnImages.ContainsKey(PowerupManager.PowerupType.Heart))
        {
            var img = _btnImages[PowerupManager.PowerupType.Heart];
            Color orig = img.color;
            for (int i = 0; i < 4; i++)
            {
                img.color = Color.white;
                yield return new WaitForSeconds(0.15f);
                img.color = orig;
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    // -------------------------------------------------------
    // Timer Display
    // -------------------------------------------------------
    void ShowTimer(PowerupManager.PowerupType type, string label, Color color, float duration)
    {
        if (_timerDisplays.ContainsKey(type) && _timerDisplays[type] != null)
        {
            _timerDisplays[type].SetActive(true);
            return;
        }

        var timerGO = new GameObject("Timer_" + type, typeof(RectTransform));
        timerGO.transform.SetParent(_activeTimersPanel.transform, false);

        var le = timerGO.AddComponent<LayoutElement>();
        le.preferredWidth = 180;
        le.preferredHeight = 45;

        var bg = timerGO.AddComponent<Image>();
        bg.color = new Color(color.r, color.g, color.b, 0.4f);

        // Fill bar
        var fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(timerGO.transform, false);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(color.r, color.g, color.b, 0.7f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        _timerFills[type] = fillImg;

        // Label text
        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(timerGO.transform, false);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(5, 0);
        textRT.offsetMax = new Vector2(-5, 0);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        _timerTexts[type] = tmp;

        _timerDisplays[type] = timerGO;
    }

    void UpdateTimer(PowerupManager.PowerupType type, float remaining, float total)
    {
        if (_timerFills.ContainsKey(type) && _timerFills[type] != null)
            _timerFills[type].fillAmount = Mathf.Clamp01(remaining / total);

        if (_timerTexts.ContainsKey(type) && _timerTexts[type] != null)
        {
            string label = type == PowerupManager.PowerupType.Booster ? "2X" :
                           type == PowerupManager.PowerupType.Lightning ? "SPEED" : "SHIELD";
            _timerTexts[type].text = label + " " + Mathf.CeilToInt(remaining) + "s";
        }
    }

    void HideTimer(PowerupManager.PowerupType type)
    {
        if (_timerDisplays.ContainsKey(type) && _timerDisplays[type] != null)
            Destroy(_timerDisplays[type]);
    }

    // -------------------------------------------------------
    // Refresh quantities
    // -------------------------------------------------------
    public void RefreshQuantities()
    {
        foreach (PowerupManager.PowerupType type in System.Enum.GetValues(typeof(PowerupManager.PowerupType)))
        {
            if (_btnQtyTexts.ContainsKey(type))
            {
                int qty = PowerupManager.GetQuantity(type);
                _btnQtyTexts[type].text = "x" + qty;
            }

            if (_btnImages.ContainsKey(type))
            {
                int qty = PowerupManager.GetQuantity(type);
                bool isActive = (type == PowerupManager.PowerupType.Booster && IsBoosterActive) ||
                                (type == PowerupManager.PowerupType.Lightning && IsLightningActive) ||
                                (type == PowerupManager.PowerupType.Shield && IsShieldActive);

                if (qty <= 0 && !isActive)
                    _btnImages[type].color = DisabledColor;
            }
        }
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------
    RectTransform CreateRT(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
