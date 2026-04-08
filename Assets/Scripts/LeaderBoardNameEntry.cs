using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Self-contained name-entry popup that appears when the player achieves a high score.
/// Creates its own Canvas and UI elements at runtime � just call Show(score, callback).
/// Designed to overlay on top of the Game Over panel.
/// </summary>
public class LeaderboardNameEntry : MonoBehaviour
{
    // Singleton-like quick access
    private static LeaderboardNameEntry _instance;

    private Canvas _canvas;
    private GameObject _panel;
    private TMP_InputField _inputField;
    private TMP_Text _messageText;
    private TMP_Text _scoreText;
    private Button _submitButton;
    private Button _skipButton;

    private int _pendingScore;
    private System.Action _onComplete;

    // -------------------------------------------------------
    // Colour palette (consistent with LeaderboardUI)
    // -------------------------------------------------------
    private static readonly Color BgOverlay = new Color(0, 0, 0, 0.7f);
    private static readonly Color PanelColor = new Color(0.15f, 0.08f, 0.25f, 0.97f);
    private static readonly Color AccentColor = new Color(1f, 0.6f, 0.1f, 1f);
    private static readonly Color BtnSubmit = new Color(0.2f, 0.75f, 0.3f, 1f);
    private static readonly Color BtnSkip = new Color(0.5f, 0.5f, 0.5f, 1f);

    // -------------------------------------------------------
    // Public Static API
    // -------------------------------------------------------

    /// <summary>
    /// Shows the name-entry popup. Call from GameManager when game ends.
    /// onComplete fires after the player submits or skips.
    /// </summary>
    public static void Show(int score, System.Action onComplete = null)
    {
        if (_instance == null)
        {
            var go = new GameObject("LeaderboardNameEntry");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LeaderboardNameEntry>();
        }

        _instance._pendingScore = score;
        _instance._onComplete = onComplete;
        _instance.BuildAndShow();
    }

    // -------------------------------------------------------
    // Build UI
    // -------------------------------------------------------

    void BuildAndShow()
    {
        // Clean up old UI if exists
        if (_canvas != null)
            Destroy(_canvas.gameObject);

        // --- Canvas ---
        var canvasGO = new GameObject("NameEntryCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200; // above everything
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Overlay bg ---
        var overlayGO = CreateUI("Overlay", canvasGO.transform);
        StretchFull(overlayGO.GetComponent<RectTransform>());
        overlayGO.AddComponent<Image>().color = BgOverlay;

        // --- Central Panel ---
        _panel = CreateUI("Panel", overlayGO.transform);
        var panelRect = _panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(900, 750);
        _panel.AddComponent<Image>().color = PanelColor;

        var vlg = _panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 20;
        vlg.padding = new RectOffset(40, 40, 40, 40);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // --- "NEW HIGH SCORE!" ---
        _messageText = AddLabel(_panel.transform, "\u2605 NEW HIGH SCORE! \u2605", 52, AccentColor, 80);

        // --- Score display ---
        _scoreText = AddLabel(_panel.transform, "Score: " + _pendingScore, 44, Color.white, 70);

        // --- Spacer ---
        AddSpacer(_panel.transform, 10);

        // --- "Enter Your Name:" ---
        AddLabel(_panel.transform, "Enter Your Name:", 36, new Color(1, 1, 1, 0.7f), 50);

        // --- Input Field ---
        CreateInputField(_panel.transform);

        // --- Spacer ---
        AddSpacer(_panel.transform, 15);

        // --- Buttons row ---
        var btnRow = CreateUI("ButtonRow", _panel.transform);
        var btnRowRect = btnRow.GetComponent<RectTransform>();
        btnRowRect.sizeDelta = new Vector2(0, 90);
        var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 30;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        _submitButton = CreateButton(btnRow.transform, "SUBMIT", BtnSubmit, OnSubmit);
        _skipButton = CreateButton(btnRow.transform, "SKIP", BtnSkip, OnSkip);
    }

    // -------------------------------------------------------
    // Input Field creation
    // -------------------------------------------------------

    void CreateInputField(Transform parent)
    {
        var fieldGO = CreateUI("InputField", parent);
        var fieldRect = fieldGO.GetComponent<RectTransform>();
        fieldRect.sizeDelta = new Vector2(0, 90);

        var fieldBg = fieldGO.AddComponent<Image>();
        fieldBg.color = new Color(0.25f, 0.15f, 0.35f, 1f);

        // Text Area (child container for the text)
        var textAreaGO = CreateUI("TextArea", fieldGO.transform);
        var textAreaRect = textAreaGO.GetComponent<RectTransform>();
        StretchFull(textAreaRect);
        textAreaRect.offsetMin = new Vector2(15, 5);
        textAreaRect.offsetMax = new Vector2(-15, -5);
        textAreaGO.AddComponent<RectMask2D>();

        // Placeholder
        var placeholderGO = CreateUI("Placeholder", textAreaGO.transform);
        StretchFull(placeholderGO.GetComponent<RectTransform>());
        var phTmp = placeholderGO.AddComponent<TextMeshProUGUI>();
        phTmp.text = "Your Name...";
        phTmp.fontSize = 36;
        phTmp.alignment = TextAlignmentOptions.MidlineLeft;
        phTmp.color = new Color(1, 1, 1, 0.35f);
        phTmp.fontStyle = FontStyles.Italic;

        // Input text
        var inputTextGO = CreateUI("Text", textAreaGO.transform);
        StretchFull(inputTextGO.GetComponent<RectTransform>());
        var inputTmp = inputTextGO.AddComponent<TextMeshProUGUI>();
        inputTmp.fontSize = 38;
        inputTmp.alignment = TextAlignmentOptions.MidlineLeft;
        inputTmp.color = Color.white;

        // TMP_InputField component
        _inputField = fieldGO.AddComponent<TMP_InputField>();
        _inputField.textViewport = textAreaRect;
        _inputField.textComponent = inputTmp;
        _inputField.placeholder = phTmp;
        _inputField.characterLimit = 15;
        _inputField.contentType = TMP_InputField.ContentType.Alphanumeric;
        _inputField.onFocusSelectAll = true;

        // Set the caret color
        _inputField.caretColor = AccentColor;
        _inputField.customCaretColor = true;
        _inputField.selectionColor = new Color(1f, 0.6f, 0.1f, 0.3f);
    }

    // -------------------------------------------------------
    // Button Callbacks
    // -------------------------------------------------------

    void OnSubmit()
    {
        string name = _inputField != null ? _inputField.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) name = "Player";

        LeaderboardManager.AddEntry(name, _pendingScore);
        Close();
    }

    void OnSkip()
    {
        // Still save with a default name
        LeaderboardManager.AddEntry("Player", _pendingScore);
        Close();
    }

    void Close()
    {
        if (_canvas != null)
            Destroy(_canvas.gameObject);

        var callback = _onComplete;
        _onComplete = null;
        callback?.Invoke();
    }

    // -------------------------------------------------------
    // UI Helpers
    // -------------------------------------------------------

    static GameObject CreateUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    TMP_Text AddLabel(Transform parent, string text, float fontSize, Color color, float height)
    {
        var go = CreateUI("Label", parent);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, height);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    void AddSpacer(Transform parent, float height)
    {
        var go = CreateUI("Spacer", parent);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }

    Button CreateButton(Transform parent, string label, Color bgColor, UnityEngine.Events.UnityAction action)
    {
        var go = CreateUI("Btn_" + label, parent);
        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.15f;
        colors.pressedColor = bgColor * 0.8f;
        btn.colors = colors;
        btn.onClick.AddListener(action);

        var txtGO = CreateUI("Text", go.transform);
        StretchFull(txtGO.GetComponent<RectTransform>());
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 38;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }
}

