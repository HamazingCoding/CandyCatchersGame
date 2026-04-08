using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Score & Gems HUD")]
    public TMP_Text scoreText;
    public TMP_Text gemsText;

    [Header("Lives / Hearts")]
    public Image[] heartImages;
    public TMP_Text livesCountText;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject revivePanel;
    public GameObject levelCompletePanel;

    [Header("Game Over Elements")]
    public TMP_Text finalScoreText;

    [Header("Revive Panel Elements")]
    public Button freeReviveButton;
    public Button paidReviveButton;
    public TMP_Text paidReviveCostText;
    public TMP_Text reviveGemsBalanceText;

    [Header("Level Complete Panel Elements")]
    public Image[] levelCompleteStars;
    public TMP_Text gemsAwardedText;

    // Runtime gem icon placed next to gemsText
    private Image _hudGemIcon;

    // Endless stats HUD (created at runtime)
    private GameObject _endlessStatsPanel;
    private TMP_Text _speedText;
    private TMP_Text _trickText;
    private TMP_Text _timeText;
    private TMP_Text _caughtText;

    void Start()
    {
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        revivePanel.SetActive(false);
        levelCompletePanel.SetActive(false);

        CreateHUDGemIcon();
    }

    // -------------------------------------------------------
    // HUD gem icon -- placed as a child of gemsText, left side
    // -------------------------------------------------------
    void CreateHUDGemIcon()
    {
        if (gemsText == null) return;

        var icon = GemIcon.CreateIcon(gemsText.transform, 32f);
        if (icon == null) return;

        _hudGemIcon = icon;
        var rt = icon.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(2, 0);

        // Shift text right to make room for the icon
        gemsText.margin = new Vector4(38, 0, 0, 0);
    }

    // -------------------------------------------------------
    // SCORE
    // -------------------------------------------------------
    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }

    // -------------------------------------------------------
    // GEMS
    // -------------------------------------------------------
    public void UpdateGems(int gems)
    {
        if (gemsText != null)
            gemsText.text = gems.ToString();
    }

    // -------------------------------------------------------
    // LIVES
    // -------------------------------------------------------
    public void UpdateLives(int lives)
    {
        for (int i = 0; i < heartImages.Length; i++)
            heartImages[i].enabled = i < lives;

        if (livesCountText != null)
            livesCountText.text = lives > 0 ? "x" + lives : "";
    }

    // -------------------------------------------------------
    // PAUSE
    // -------------------------------------------------------
    public void ShowPause(bool show)
    {
        pausePanel.SetActive(show);
    }

    // -------------------------------------------------------
    // GAME OVER
    // -------------------------------------------------------
    public void ShowGameOver(int finalScore, int gemsEarned = 0)
    {
        gameOverPanel.SetActive(true);
        if (finalScoreText != null)
        {
            string text = "Score: " + finalScore.ToString();
            if (gemsEarned > 0)
                text += "\n+" + gemsEarned + " Gems Earned";
            finalScoreText.text = text;
        }
    }

    // -------------------------------------------------------
    // REVIVE PANEL
    // -------------------------------------------------------
    public void ShowRevivePanel(bool hasFreeRevive, int playerGems)
    {
        revivePanel.SetActive(true);

        if (reviveGemsBalanceText != null)
            reviveGemsBalanceText.text = playerGems + " Gems";

        if (freeReviveButton != null)
            freeReviveButton.gameObject.SetActive(hasFreeRevive);

        if (paidReviveButton != null)
        {
            paidReviveButton.gameObject.SetActive(!hasFreeRevive);
            if (paidReviveCostText != null)
                paidReviveCostText.text = "Revive (100 Gems)";
        }
    }

    public void HideRevivePanel()
    {
        revivePanel.SetActive(false);
    }

    // -------------------------------------------------------
    // LEVEL COMPLETE PANEL
    // -------------------------------------------------------
    public void ShowLevelComplete(int stars, int gemsAwarded)
    {
        levelCompletePanel.SetActive(true);

        if (levelCompleteStars != null)
        {
            for (int i = 0; i < levelCompleteStars.Length; i++)
            {
                if (levelCompleteStars[i] != null)
                    levelCompleteStars[i].enabled = i < stars;
            }
        }

        if (gemsAwardedText != null)
            gemsAwardedText.text = "+" + gemsAwarded + " Gems";
    }

    // -------------------------------------------------------
    // ENDLESS MODE STATS HUD
    // -------------------------------------------------------

    public void ShowEndlessStats()
    {
        if (_endlessStatsPanel != null) return;

        Canvas canvas = null;
        if (heartImages != null && heartImages.Length > 0 && heartImages[0] != null)
            canvas = heartImages[0].GetComponentInParent<Canvas>();
        if (canvas == null && scoreText != null)
            canvas = scoreText.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _endlessStatsPanel = new GameObject("EndlessStatsPanel");
        _endlessStatsPanel.transform.SetParent(canvas.transform, false);

        var rt = _endlessStatsPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -160);
        rt.sizeDelta = new Vector2(320, 120);

        var bg = _endlessStatsPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.35f);

        var vlg = _endlessStatsPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(12, 12, 6, 6);
        vlg.spacing = 1;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var statColor = new Color(0.9f, 0.85f, 1f);
        _speedText  = MakeStatLabel(_endlessStatsPanel.transform, "Speed: 3.0", statColor);
        _trickText  = MakeStatLabel(_endlessStatsPanel.transform, "Trick: 10%", statColor);
        _timeText   = MakeStatLabel(_endlessStatsPanel.transform, "Time: 0:00", statColor);
        _caughtText = MakeStatLabel(_endlessStatsPanel.transform, "Caught: 0", statColor);
    }

    public void UpdateEndlessStats(float speed, float trickChance, float timeAlive, int candiesCaught)
    {
        if (_endlessStatsPanel == null) return;

        int mins = Mathf.FloorToInt(timeAlive / 60f);
        int secs = Mathf.FloorToInt(timeAlive % 60f);

        if (_speedText != null)  _speedText.text  = "Speed: " + speed.ToString("F1");
        if (_trickText != null)  _trickText.text  = "Trick: " + Mathf.RoundToInt(trickChance * 100f) + "%";
        if (_timeText != null)   _timeText.text   = "Time: " + mins + ":" + secs.ToString("D2");
        if (_caughtText != null) _caughtText.text = "Caught: " + candiesCaught;
    }

    TMP_Text MakeStatLabel(Transform parent, string text, Color color)
    {
        var go = new GameObject("Stat", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 26;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return tmp;
    }
}
