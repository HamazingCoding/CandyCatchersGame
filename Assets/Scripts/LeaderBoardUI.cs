using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Attach to a root GameObject in the LeaderboardScene.
/// Builds the leaderboard display dynamically at runtime � no prefab drag-drop needed.
/// Creates a full-screen Canvas with a styled scrollable list.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    // These are auto-created at runtime; exposed for optional Inspector override
    [Header("Override (optional � leave null for auto-creation)")]
    public Transform contentParent;   // ScrollRect content
    public TMP_Text titleText;
    public Button backButton;

    // -------------------------------------------------------
    // Colour palette (matches candy-catcher Halloween theme)
    // -------------------------------------------------------
    private static readonly Color BgColor = new Color(0.12f, 0.07f, 0.20f, 0.95f); // deep purple
    private static readonly Color HeaderColor = new Color(1f, 0.6f, 0.1f, 1f);         // candy orange
    private static readonly Color RowColorEven = new Color(0.18f, 0.10f, 0.28f, 0.90f);
    private static readonly Color RowColorOdd = new Color(0.22f, 0.13f, 0.33f, 0.90f);
    private static readonly Color GoldColor = new Color(1f, 0.84f, 0f, 1f);
    private static readonly Color SilverColor = new Color(0.75f, 0.75f, 0.80f, 1f);
    private static readonly Color BronzeColor = new Color(0.80f, 0.50f, 0.20f, 1f);
    private static readonly Color NormalTextColor = Color.white;
    private static readonly Color ButtonColor = new Color(0.85f, 0.35f, 0.15f, 1f);    // warm orange-red

    void Start()
    {
        Debug.Log("[LeaderboardUI] Start() called.");

        if (contentParent == null)
        {
            Debug.Log("[LeaderboardUI] contentParent is null � building UI from scratch.");
            BuildUI();
        }

        if (contentParent == null)
        {
            Debug.LogError("[LeaderboardUI] contentParent is STILL null after BuildUI()! Aborting.");
            return;
        }

        Debug.Log("[LeaderboardUI] contentParent set: " + contentParent.name
            + " | active: " + contentParent.gameObject.activeInHierarchy);

        // Seed test data if leaderboard is empty so there's always something to display
        LeaderboardManager.SeedTestData();

        PopulateEntries();
    }

    // -------------------------------------------------------
    // UI Construction (fully procedural)
    // -------------------------------------------------------

    private Canvas _canvas;

    void BuildUI()
    {
        // --- Canvas ---
        var canvasGO = new GameObject("LeaderboardCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Full-screen dark background ---
        var bgGO = CreateUIObject("Background", canvasGO.transform);
        var bgRect = bgGO.GetComponent<RectTransform>();
        StretchFull(bgRect);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = BgColor;

        // --- Title ---
        var titleGO = CreateUIObject("Title", bgRect);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(0, 120);
        titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "\u2605 LEADERBOARD \u2605";
        titleText.fontSize = 64;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = HeaderColor;
        titleText.fontStyle = FontStyles.Bold;

        // --- Column Header Row ---
        float headerY = -170f;
        var headerGO = CreateUIObject("Header", bgRect);
        var headerRect = headerGO.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = new Vector2(0, headerY);
        headerRect.sizeDelta = new Vector2(-80, 60);
        var headerLayout = headerGO.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.spacing = 10;
        headerLayout.padding = new RectOffset(20, 20, 0, 0);
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = true;

        AddHeaderLabel(headerGO.transform, "RANK", 0.15f);
        AddHeaderLabel(headerGO.transform, "NAME", 0.50f);
        AddHeaderLabel(headerGO.transform, "SCORE", 0.35f);

        // --- Scroll View ---
        float scrollY = headerY - 70f;
        var scrollGO = CreateUIObject("ScrollView", bgRect);
        var scrollRect = scrollGO.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(40, 200);  // bottom padding for button
        scrollRect.offsetMax = new Vector2(-40, scrollY);

        var scrollView = scrollGO.AddComponent<ScrollRect>();
        scrollView.horizontal = false;
        scrollView.movementType = ScrollRect.MovementType.Clamped;

        // Viewport with mask
        var viewportGO = CreateUIObject("Viewport", scrollGO.transform);
        var viewportRect = viewportGO.GetComponent<RectTransform>();
        StretchFull(viewportRect);
        // Use RectMask2D instead of Mask+Image � RectMask2D clips by rect bounds
        // and doesn't rely on image alpha, avoiding the invisible-content bug.
        viewportGO.AddComponent<RectMask2D>();

        // Content
        var contentGO = CreateUIObject("Content", viewportGO.transform);
        contentParent = contentGO.transform;
        var contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6;
        vlg.padding = new RectOffset(0, 0, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollView.viewport = viewportRect;
        scrollView.content = contentRect;

        // --- Back Button ---
        var btnGO = CreateUIObject("BackButton", bgRect);
        var btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.pivot = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = new Vector2(0, 50);
        btnRect.sizeDelta = new Vector2(400, 100);

        var btnImage = btnGO.AddComponent<Image>();
        btnImage.color = ButtonColor;

        backButton = btnGO.AddComponent<Button>();
        backButton.targetGraphic = btnImage;
        var btnColors = backButton.colors;
        btnColors.highlightedColor = ButtonColor * 1.15f;
        btnColors.pressedColor = ButtonColor * 0.8f;
        backButton.colors = btnColors;
        backButton.onClick.AddListener(OnBackPressed);

        var btnTextGO = CreateUIObject("Text", btnGO.transform);
        StretchFull(btnTextGO.GetComponent<RectTransform>());
        var btnTmp = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "BACK";
        btnTmp.fontSize = 48;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = Color.white;
        btnTmp.fontStyle = FontStyles.Bold;
    }

    // -------------------------------------------------------
    // Populate Entries
    // -------------------------------------------------------

    void PopulateEntries()
    {
        Debug.Log("[LeaderboardUI] PopulateEntries() called.");

        // Clear existing rows
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var entries = LeaderboardManager.GetEntries();
        Debug.Log("[LeaderboardUI] Fetched " + entries.Count + " entries from LeaderboardManager.");

        if (entries.Count == 0)
        {
            Debug.Log("[LeaderboardUI] No entries found � showing empty message.");
            AddEmptyMessage();
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            Debug.Log("[LeaderboardUI] Creating row " + (i + 1) + ": "
                + entries[i].playerName + " � " + entries[i].score);
            CreateRow(i + 1, entries[i], i % 2 == 0);
        }

        Debug.Log("[LeaderboardUI] All " + entries.Count + " rows created. Content child count: "
            + contentParent.childCount);

        // Force layout rebuild to ensure rows are visible
        Canvas.ForceUpdateCanvases();
        var contentRect = contentParent.GetComponent<RectTransform>();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Debug.Log("[LeaderboardUI] Content sizeDelta after rebuild: " + contentRect.sizeDelta);
    }

    void CreateRow(int rank, LeaderboardManager.LeaderboardEntry entry, bool even)
    {
        var rowGO = CreateUIObject("Row_" + rank, contentParent);
        var rowRect = rowGO.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 80);

        // Ensure VerticalLayoutGroup respects the row height
        var rowLE = rowGO.AddComponent<LayoutElement>();
        rowLE.minHeight = 80;
        rowLE.preferredHeight = 80;

        var rowImage = rowGO.AddComponent<Image>();
        rowImage.color = even ? RowColorEven : RowColorOdd;

        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 10;
        hlg.padding = new RectOffset(20, 20, 5, 5);
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        // Rank color
        Color rankColor;
        string rankText;
        switch (rank)
        {
            case 1: rankColor = GoldColor; rankText = "\U0001f947"; break; // gold medal emoji
            case 2: rankColor = SilverColor; rankText = "\U0001f948"; break;
            case 3: rankColor = BronzeColor; rankText = "\U0001f949"; break;
            default: rankColor = NormalTextColor; rankText = "#" + rank; break;
        }

        AddRowCell(rowGO.transform, rankText, 0.15f, rankColor, 40);
        AddRowCell(rowGO.transform, entry.playerName, 0.50f, NormalTextColor, 36);
        AddRowCell(rowGO.transform, entry.score.ToString("N0"), 0.35f, HeaderColor, 40);
    }

    void AddEmptyMessage()
    {
        var msgGO = CreateUIObject("EmptyMsg", contentParent);
        var msgRect = msgGO.GetComponent<RectTransform>();
        msgRect.sizeDelta = new Vector2(0, 300);
        var msgLE = msgGO.AddComponent<LayoutElement>();
        msgLE.minHeight = 300;
        msgLE.preferredHeight = 300;
        var tmp = msgGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "No scores yet!\nPlay a level to get on the board!";
        tmp.fontSize = 40;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 1f, 1f, 0.7f);
    }

    // -------------------------------------------------------
    // Navigation
    // -------------------------------------------------------

    void OnBackPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    // -------------------------------------------------------
    // UI Helper Methods
    // -------------------------------------------------------

    static GameObject CreateUIObject(string name, Transform parent)
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

    void AddHeaderLabel(Transform parent, string text, float flexWeight)
    {
        var go = CreateUIObject("Header_" + text, parent);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = flexWeight;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 1f, 1f, 0.6f);
        tmp.fontStyle = FontStyles.Bold;
    }

    void AddRowCell(Transform parent, string text, float flexWeight, Color color, float fontSize)
    {
        var go = CreateUIObject("Cell", parent);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = flexWeight;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
    }
}

