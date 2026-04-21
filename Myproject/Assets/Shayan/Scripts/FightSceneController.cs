using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class FightSceneController : MonoBehaviour
{
    public static FightSceneController Instance { get; private set; }

    [Header("Background")]
    public SpriteRenderer fightBackground;
    public Sprite fireArena;
    public Sprite iceArena;
    public Sprite airArena;
    public Sprite earthArena;

    [Header("Character Prefabs")]
    public GameObject firePrefab;
    public GameObject icePrefab;
    public GameObject airPrefab;
    public GameObject earthPrefab;

    [Header("Spawn Points")]
    public Transform p1SpawnPoint;
    public Transform p2SpawnPoint;

    [Header("P1 Health Bar UI")]
    public Image p1HealthFill;
    public Image p1GhostFill;
    public Text  p1NameLabel;

    [Header("P2 Health Bar UI")]
    public Image p2HealthFill;
    public Image p2GhostFill;
    public Text  p2NameLabel;

    [Header("Winner Display")]
    [Tooltip("Optional pixel font. Falls back to PressStart2P from Resources if null.")]
    public Font pixelFont;

    // ── Runtime State ─────────────────────────────────────────────────────────
    private GameObject p1GO;
    private GameObject p2GO;
    private int  p1Wins;
    private int  p2Wins;
    private bool roundLocked;   // blocks double-KO processing within one round
    private bool matchOver;

    // Win-dot rows (3 Image components each, one per possible round win)
    private Image[] p1WinDots;
    private Image[] p2WinDots;

    // Cached pixel-art circle sprites (created once)
    private Sprite _hollowCircle;
    private Sprite _filledCircle;

    // Round timer
    private const float RoundDuration = 60f;
    private float _timeRemaining;
    private Text  _timerText;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyBackground();
        StartCoroutine(InitAfterLayout());
    }

    // Wait one frame so the existing UI canvas has finished its layout pass
    // before we sample GetWorldCorners() for dot placement.
    private IEnumerator InitAfterLayout()
    {
        yield return null;
        _hollowCircle = CreateCircleSprite(16, false);
        _filledCircle = CreateCircleSprite(16, true);
        CreateRoundDotsUI();
        CreateTimerUI();
        StartRound();
    }

    // ── Public API (called by FighterHealth.OnKO) ─────────────────────────────

    private void Update()
    {
        if (roundLocked || matchOver || _timerText == null) return;

        _timeRemaining = Mathf.Max(0f, _timeRemaining - Time.deltaTime);
        UpdateTimerDisplay();
        if (_timeRemaining <= 0f)
            OnTimeUp();
    }

    public void OnFighterKO(GameObject loser)
    {
        if (roundLocked || matchOver) return;
        roundLocked = true;

        bool   p1Lost     = (loser == p1GO);
        string winnerName = p1Lost ? "PLAYER 2" : "PLAYER 1";
        int    winnerIdx  = p1Lost ? 1 : 0;   // 0 = P1 won, 1 = P2 won

        if (p1Lost) p2Wins++; else p1Wins++;

        // Freeze the winning fighter as well for the intermission
        DisableMovement(p1Lost ? p2GO : p1GO);

        UpdateWinDot(winnerIdx);

        if (p1Wins >= 2 || p2Wins >= 2)
        {
            matchOver = true;
            StartCoroutine(MatchEndCoroutine(winnerName + "\nWINS!"));
        }
        else
        {
            StartCoroutine(RoundEndCoroutine(winnerName + " wins the round"));
        }
    }

    // ── Round Management ──────────────────────────────────────────────────────

    private void StartRound()
    {
        roundLocked    = false;
        _timeRemaining = RoundDuration;
        UpdateTimerDisplay();

        if (p1GO != null) Destroy(p1GO);
        if (p2GO != null) Destroy(p2GO);

        p1GO = SpawnCharacter(CharacterSelectionData.P1Index, p1SpawnPoint, false);
        p2GO = SpawnCharacter(CharacterSelectionData.P2Index, p2SpawnPoint, true);

        // P1 uses gamepad (set in prefab), P2 always uses keyboard
        AssignToKeyboard(p2GO);

        WireHealthBar(p1GO, p1HealthFill, p1GhostFill, p1NameLabel, "P1_Fill", "P1_Ghost", "P1_Label");
        WireHealthBar(p2GO, p2HealthFill, p2GhostFill, p2NameLabel, "P2_Fill", "P2_Ghost", "P2_Label");
    }

    // Show round-win message for 5 s then reset
    private IEnumerator RoundEndCoroutine(string message)
    {
        yield return new WaitForSeconds(2.5f);
        GameObject overlay = BuildAnnouncementUI(message, false);
        yield return new WaitForSeconds(5f);
        Destroy(overlay);
        StartRound();
    }

    // Show final match-winner message for 5 s then return to character select
    private IEnumerator MatchEndCoroutine(string message)
    {
        yield return new WaitForSeconds(2.5f);
        BuildAnnouncementUI(message, true);
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("CharacterSelect");
    }

    // ── Win-dot UI ────────────────────────────────────────────────────────────

    private void CreateRoundDotsUI()
    {
        GameObject canvasGO = new GameObject("RoundDotsCanvas");
        Canvas c = canvasGO.AddComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 20;
        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        p1WinDots = CreateDotRow(canvasRect, true);
        p2WinDots = CreateDotRow(canvasRect, false);
    }

    private Image[] CreateDotRow(RectTransform canvasRect, bool isP1)
    {
        Image[] dots = new Image[3];

        Vector2 center = GetDotRowScreenPos(isP1);
        const float spacing = 28f;  // center-to-center px
        float startX = center.x - spacing; // 3 dots: -spacing, 0, +spacing from center

        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject($"{(isP1 ? "P1" : "P2")}_WinDot_{i}");
            go.transform.SetParent(canvasRect, false);

            Image img = go.AddComponent<Image>();
            img.sprite        = _hollowCircle;
            img.color         = Color.white;
            img.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(20f, 20f);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            Vector2 screenPos = new Vector2(startX + i * spacing, center.y);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, null, out Vector2 local);
            rt.anchoredPosition = local;

            dots[i] = img;
        }

        return dots;
    }

    // Returns the screen-space pixel position at which to centre the dot row.
    private Vector2 GetDotRowScreenPos(bool isP1)
    {
        Image fillRef = isP1 ? p1HealthFill : p2HealthFill;
        if (fillRef != null && fillRef.canvas != null)
        {
            Vector3[] corners = new Vector3[4];
            fillRef.rectTransform.GetWorldCorners(corners);
            // corners[0]=bottom-left  corners[3]=bottom-right
            Vector3 bottomCenter = (corners[0] + corners[3]) * 0.5f;

            Vector2 screenPt;
            if (fillRef.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // World space IS screen pixel space for overlay canvases
                screenPt = new Vector2(bottomCenter.x, bottomCenter.y);
            }
            else
            {
                Camera uiCam = fillRef.canvas.worldCamera != null
                    ? fillRef.canvas.worldCamera : Camera.main;
                screenPt = RectTransformUtility.WorldToScreenPoint(uiCam, bottomCenter);
            }
            return new Vector2(screenPt.x, screenPt.y - 22f);
        }

        // Fallback: guess a sensible position
        return isP1
            ? new Vector2(Screen.width * 0.13f, Screen.height * 0.87f)
            : new Vector2(Screen.width * 0.87f, Screen.height * 0.87f);
    }

    private void UpdateWinDot(int winnerIdx)
    {
        Image[] dots = winnerIdx == 0 ? p1WinDots : p2WinDots;
        int     wins = winnerIdx == 0 ? p1Wins    : p2Wins;
        if (dots == null) return;

        int idx = wins - 1;
        if (idx >= 0 && idx < dots.Length && dots[idx] != null)
            dots[idx].sprite = _filledCircle;
    }

    // ── Time-up Draw ──────────────────────────────────────────────────────────

    private void OnTimeUp()
    {
        if (roundLocked || matchOver) return;
        roundLocked = true;

        DisableMovement(p1GO);
        DisableMovement(p2GO);

        p1Wins++;
        p2Wins++;
        UpdateWinDot(0);
        UpdateWinDot(1);

        bool p1MatchWon  = p1Wins >= 2 && p2Wins < 2;
        bool p2MatchWon  = p2Wins >= 2 && p1Wins < 2;
        bool bothMaxWins = p1Wins >= 2 && p2Wins >= 2;

        if (bothMaxWins)
        {
            matchOver = true;
            StartCoroutine(MatchEndCoroutine("IT'S A\nDRAW!"));
        }
        else if (p1MatchWon)
        {
            matchOver = true;
            StartCoroutine(MatchEndCoroutine("PLAYER 1\nWINS!"));
        }
        else if (p2MatchWon)
        {
            matchOver = true;
            StartCoroutine(MatchEndCoroutine("PLAYER 2\nWINS!"));
        }
        else
        {
            StartCoroutine(RoundEndCoroutine("TIME'S UP!\nDRAW"));
        }
    }

    // ── Timer UI ──────────────────────────────────────────────────────────────

    private void CreateTimerUI()
    {
        Font font = LoadFont();

        GameObject canvasGO = new GameObject("TimerCanvas");
        Canvas c = canvasGO.AddComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 15;
        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject textGO = new GameObject("TimerText");
        textGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rt    = textGO.AddComponent<RectTransform>();
        rt.anchorMin         = new Vector2(0.5f, 1f);
        rt.anchorMax         = new Vector2(0.5f, 1f);
        rt.pivot             = new Vector2(0.5f, 1f);
        rt.anchoredPosition  = new Vector2(0f, -20f);
        rt.sizeDelta         = new Vector2(200f, 90f);

        _timerText               = textGO.AddComponent<Text>();
        _timerText.font          = font;
        _timerText.fontSize      = 64;
        _timerText.alignment     = TextAnchor.UpperCenter;
        _timerText.color         = Color.white;
        _timerText.raycastTarget = false;

        Outline outline        = textGO.AddComponent<Outline>();
        outline.effectColor    = Color.black;
        outline.effectDistance = new Vector2(3, -3);
    }

    private void UpdateTimerDisplay()
    {
        if (_timerText == null) return;
        int seconds      = Mathf.CeilToInt(_timeRemaining);
        _timerText.text  = seconds.ToString();
        _timerText.color = seconds <= 10
            ? new Color(1f, 0.25f, 0.25f)
            : Color.white;
    }

    // ── Announcement UI ───────────────────────────────────────────────────────

    private GameObject BuildAnnouncementUI(string message, bool isMatchEnd)
    {
        Font font = LoadFont();

        GameObject canvasGO = new GameObject("AnnouncementCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Dark overlay
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panel = panelGO.AddComponent<Image>();
        panel.color = new Color(0f, 0f, 0f, 0.65f);
        StretchToParent(panelGO.GetComponent<RectTransform>());

        // Text
        GameObject textGO = new GameObject("AnnounceText");
        textGO.transform.SetParent(panelGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text      = message;
        text.fontSize  = isMatchEnd ? 72 : 56;
        text.alignment = TextAnchor.MiddleCenter;
        text.color     = Color.white;
        if (font != null) text.font = font;

        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor    = new Color(1f, 0.4f, 0f, 1f);
        outline.effectDistance = new Vector2(4, -4);

        StretchToParent(textGO.GetComponent<RectTransform>());

        return canvasGO;
    }

    private static void StretchToParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ── Pixel-circle Sprite ───────────────────────────────────────────────────

    private static Sprite CreateCircleSprite(int size, bool filled)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        float center = (size - 1) * 0.5f;
        float outerR = center - 0.5f;
        float innerR = filled ? -1f : outerR * 0.55f;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
            pixels[y * size + x] = (dist <= outerR && dist >= innerR) ? Color.white : Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // ── Background ────────────────────────────────────────────────────────────

    private void ApplyBackground()
    {
        if (fightBackground == null) { Debug.LogError("FightBackground not assigned."); return; }

        int index = PlayerPrefs.GetInt("SelectedArenaIndex", -1);
        Sprite chosen = index switch
        {
            0 => fireArena, 1 => iceArena, 2 => airArena, 3 => earthArena, _ => null
        };

        if (chosen == null) { Debug.LogError("No arena sprite for index: " + index); return; }

        fightBackground.sprite = chosen;
        FitBackgroundToCamera();
    }

    private void FitBackgroundToCamera()
    {
        Camera cam = Camera.main;
        if (cam == null || fightBackground.sprite == null) return;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth  = worldHeight * cam.aspect;
        Vector2 spriteSize = fightBackground.sprite.bounds.size;

        fightBackground.transform.position   = new Vector3(cam.transform.position.x, cam.transform.position.y, 1f);
        fightBackground.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
        fightBackground.sortingOrder = -100;
    }

    // ── Spawn / Wire ──────────────────────────────────────────────────────────

    private void WireHealthBar(
        GameObject character,
        Image healthFill, Image ghostFill, Text nameLabel,
        string fillFallback, string ghostFallback, string labelFallback)
    {
        if (character == null) return;

        FighterHealth health = character.GetComponent<FighterHealth>();
        if (health == null) { Debug.LogWarning($"No FighterHealth on {character.name}."); return; }

        health.healthFill = healthFill != null ? healthFill
            : GameObject.Find(fillFallback)?.GetComponent<Image>();
        health.ghostFill  = ghostFill  != null ? ghostFill
            : GameObject.Find(ghostFallback)?.GetComponent<Image>();
        health.nameLabel  = nameLabel  != null ? nameLabel
            : GameObject.Find(labelFallback)?.GetComponent<Text>();

        if (health.healthFill == null)
            Debug.LogError($"Could not find '{fillFallback}' for {character.name}.");

        health.ForceRefresh();
    }

    private GameObject SpawnCharacter(int index, Transform spawnPoint, bool flipX)
    {
        if (spawnPoint == null) { Debug.LogError("Spawn point not assigned."); return null; }

        GameObject prefab = index switch
        {
            0 => firePrefab, 1 => icePrefab, 2 => airPrefab, 3 => earthPrefab, _ => null
        };

        if (prefab == null) { Debug.LogWarning($"No prefab for index {index}."); return null; }

        GameObject character = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        if (flipX)
        {
            Vector3 s = character.transform.localScale;
            s.x = -Mathf.Abs(s.x);
            character.transform.localScale = s;
        }

        return character;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void AssignToKeyboard(GameObject go)
    {
        if (go == null || Keyboard.current == null) return;
        var pi = go.GetComponent<PlayerInput>();
        if (pi == null) return;
        pi.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
    }

    private void DisableMovement(GameObject go)
    {
        if (go == null) return;
        var air   = go.GetComponent<PlayerMovementAir>();
        var earth = go.GetComponent<PlayerMovementEarth>();
        var fire  = go.GetComponent<PlayerMovementFire>();
        if (air   != null) air.enabled   = false;
        if (earth != null) earth.enabled = false;
        if (fire  != null) fire.enabled  = false;
    }

    private Font LoadFont()
    {
        if (pixelFont != null) return pixelFont;
        return Resources.Load<Font>("Fonts/PressStart2P-Regular");
    }
}