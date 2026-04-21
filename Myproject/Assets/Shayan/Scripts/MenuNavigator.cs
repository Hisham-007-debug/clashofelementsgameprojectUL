using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Auto-injected into every menu scene on load.
/// Manages two independent cursors:
///   P1 (silver border) — Gamepad 1 (Gamepad.all[0])
///   P2 (gold border)   — Keyboard by default; Gamepad 2 (Gamepad.all[1]) takes priority
/// </summary>
public class MenuNavigator : MonoBehaviour
{
    // ── visual config ─────────────────────────────────────────────────────────
    static readonly Color P1Color = new Color(0.85f, 0.85f, 0.85f, 1f); // silver
    static readonly Color P2Color = new Color(1f,    0.85f, 0.1f,  1f); // gold
    const float Thick = 4f;
    const float Bleed = 3f;

    // ── input timing ─────────────────────────────────────────────────────────
    const float InitialDelay = 0.35f;
    const float RepeatDelay  = 0.12f;

    // ── static: readable by CharacterSelectController ────────────────────────
    /// <summary>0 = P1 cursor fired the confirm, 1 = P2 cursor fired it.</summary>
    public static int ConfirmingPlayer { get; private set; }

    // ── per-cursor state ──────────────────────────────────────────────────────
    struct CursorState
    {
        public int   current;
        public float axisHeld;
        public bool  axisActive;
    }

    CursorState _p1, _p2;

    // ── scene state ───────────────────────────────────────────────────────────
    Button[]       _buttons;
    GameObject[][] _p1Borders; // [buttonIdx][barIdx 0-3]
    GameObject[][] _p2Borders;
    bool           _confirmedThisFrame;

    // ── bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "FightScene" || scene.name == "TitleScene") return;
        new GameObject("[MenuNavigator]").AddComponent<MenuNavigator>();
    }

    // ── lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        _buttons = FindObjectsByType<Button>(FindObjectsSortMode.None)
            .Where(b => b.isActiveAndEnabled && b.interactable)
            .OrderBy(b =>
            {
                var p = ScreenCenter(b);
                return -p.y * 100000f + p.x;
            })
            .ToArray();

        if (_buttons.Length == 0) { Destroy(gameObject); return; }

        _p1Borders = new GameObject[_buttons.Length][];
        _p2Borders = new GameObject[_buttons.Length][];
        for (int i = 0; i < _buttons.Length; i++)
        {
            _p1Borders[i] = BuildBorder(_buttons[i], P1Color);
            _p2Borders[i] = BuildBorder(_buttons[i], P2Color);
        }

        SelectP1(0);
        SelectP2(0);
    }

    void Update()
    {
        CheckMouseHover();
        HandleP1Input();
        HandleP2Input();
    }

    void LateUpdate() => _confirmedThisFrame = false;

    // ── P1 input: Gamepad 1 only ──────────────────────────────────────────────
    void HandleP1Input()
    {
        if (_buttons == null || _buttons.Length == 0) return;
        var gp = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;
        if (gp == null) return;

        if (gp.buttonSouth.wasPressedThisFrame) { Confirm(0); return; }

        Vector2 stick = gp.leftStick.ReadValue();
        float v = stick.y, h = stick.x;
        if (gp.dpad.up.isPressed)    v =  1f;
        if (gp.dpad.down.isPressed)  v = -1f;
        if (gp.dpad.left.isPressed)  h = -1f;
        if (gp.dpad.right.isPressed) h =  1f;

        float axis = Mathf.Abs(v) >= Mathf.Abs(h) ? -v : h;
        StepCursor(ref _p1, axis, dir => SelectP1(NextValid(_p1.current, dir)));
    }

    // ── P2 input: Keyboard default; Gamepad 2 has priority ───────────────────
    void HandleP2Input()
    {
        if (_buttons == null || _buttons.Length == 0) return;

        bool confirm       = false;
        float v = 0f, h   = 0f;
        bool gpHasInput    = false;

        // Second gamepad (priority over keyboard)
        if (Gamepad.all.Count >= 2)
        {
            var gp2     = Gamepad.all[1];
            Vector2 s   = gp2.leftStick.ReadValue();
            bool anyDir = s.magnitude > 0.1f
                       || gp2.dpad.up.isPressed || gp2.dpad.down.isPressed
                       || gp2.dpad.left.isPressed || gp2.dpad.right.isPressed;

            if (gp2.buttonSouth.wasPressedThisFrame) { confirm = true; gpHasInput = true; }

            if (anyDir)
            {
                gpHasInput = true;
                v = s.y; h = s.x;
                if (gp2.dpad.up.isPressed)    v =  1f;
                if (gp2.dpad.down.isPressed)  v = -1f;
                if (gp2.dpad.left.isPressed)  h = -1f;
                if (gp2.dpad.right.isPressed) h =  1f;
            }
        }

        // Keyboard / mouse fallback (only when Gamepad 2 has no input this frame)
        if (!gpHasInput && Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                confirm = true;
            if (Keyboard.current.upArrowKey.isPressed    || Keyboard.current.wKey.isPressed) v =  1f;
            if (Keyboard.current.downArrowKey.isPressed  || Keyboard.current.sKey.isPressed) v = -1f;
            if (Keyboard.current.leftArrowKey.isPressed  || Keyboard.current.aKey.isPressed) h = -1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) h =  1f;
        }
        if (!gpHasInput && Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
            confirm = true;

        if (confirm) { Confirm(1); return; }

        float axis = Mathf.Abs(v) >= Mathf.Abs(h) ? -v : h;
        StepCursor(ref _p2, axis, dir => SelectP2(NextValid(_p2.current, dir)));
    }

    // ── mouse hover: moves P2 gold cursor to the hovered button ─────────────────
    // Only fires when the mouse actually moves, so keyboard/gamepad can still
    // navigate freely when the mouse is sitting still.
    void CheckMouseHover()
    {
        if (_buttons == null || _buttons.Length == 0) return;
        if (Mouse.current == null) return;
        if (Mouse.current.delta.ReadValue().sqrMagnitude < 0.01f) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (!_buttons[i].isActiveAndEnabled || !_buttons[i].interactable) continue;
            var rt = _buttons[i].GetComponent<RectTransform>();
            if (rt == null) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null))
            {
                SelectP2(i);
                break;
            }
        }
    }

    // ── shared cursor step / repeat logic ────────────────────────────────────
    delegate void NavAction(int dir);

    void StepCursor(ref CursorState state, float axis, NavAction navigate)
    {
        bool stepped = false;
        if (Mathf.Abs(axis) > 0.5f)
        {
            if (!state.axisActive)
            {
                state.axisActive = true;
                state.axisHeld   = 0f;
                stepped          = true;
            }
            else
            {
                state.axisHeld += Time.unscaledDeltaTime;
                if (state.axisHeld >= InitialDelay)
                {
                    state.axisHeld = InitialDelay - RepeatDelay;
                    stepped        = true;
                }
            }
        }
        else
        {
            state.axisActive = false;
            state.axisHeld   = 0f;
        }

        if (stepped) navigate(axis > 0f ? 1 : -1);
    }

    // ── confirm ───────────────────────────────────────────────────────────────
    void Confirm(int cursorIdx)
    {
        if (_confirmedThisFrame) return; // prevent double scene-load same frame
        _confirmedThisFrame = true;

        int idx = cursorIdx == 0 ? _p1.current : _p2.current;
        var btn = _buttons[idx];
        if (btn == null || !btn.isActiveAndEnabled || !btn.interactable) return;

        ConfirmingPlayer = cursorIdx;
        btn.onClick.Invoke();
    }

    // ── navigation helpers ────────────────────────────────────────────────────
    int NextValid(int from, int dir)
    {
        int count = _buttons.Length;
        int next  = (from + dir + count) % count;
        for (int tries = 0; tries < count && (!_buttons[next].isActiveAndEnabled || !_buttons[next].interactable); tries++)
            next = (next + dir + count) % count;
        return next;
    }

    // ── cursor selection (show/hide borders) ──────────────────────────────────
    void SelectP1(int idx)
    {
        SetBordersActive(_p1Borders, _p1.current, false);
        _p1.current = Mathf.Clamp(idx, 0, _buttons.Length - 1);
        SetBordersActive(_p1Borders, _p1.current, true);
    }

    void SelectP2(int idx)
    {
        SetBordersActive(_p2Borders, _p2.current, false);
        _p2.current = Mathf.Clamp(idx, 0, _buttons.Length - 1);
        SetBordersActive(_p2Borders, _p2.current, true);
    }

    void SetBordersActive(GameObject[][] borders, int buttonIdx, bool active)
    {
        if (borders == null || buttonIdx < 0 || buttonIdx >= borders.Length) return;
        var row = borders[buttonIdx];
        if (row == null) return;
        foreach (var go in row)
            if (go != null) go.SetActive(active);
    }

    // ── pixel-art border builder ───────────────────────────────────────────────
    GameObject[] BuildBorder(Button btn, Color color)
    {
        var bars = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject("_Border" + i);
            go.transform.SetParent(btn.transform, false);
            go.transform.SetAsFirstSibling();

            var img = go.AddComponent<Image>();
            img.color         = color;
            img.raycastTarget = false;

            ApplyBarAnchors(go.GetComponent<RectTransform>(), i, Thick, Bleed);
            go.SetActive(false);
            bars[i] = go;
        }
        return bars;
    }

    static void ApplyBarAnchors(RectTransform rt, int side, float thick, float bleed)
    {
        switch (side)
        {
            case 0: // Top
                rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = new Vector2(-bleed, 0);
                rt.offsetMax = new Vector2(bleed, bleed + thick);
                break;
            case 1: // Bottom
                rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
                rt.offsetMin = new Vector2(-bleed, -(bleed + thick));
                rt.offsetMax = new Vector2(bleed, 0);
                break;
            case 2: // Left
                rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0, 1);
                rt.offsetMin = new Vector2(-(bleed + thick), -bleed);
                rt.offsetMax = new Vector2(0, bleed);
                break;
            case 3: // Right
                rt.anchorMin = new Vector2(1, 0); rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = new Vector2(0, -bleed);
                rt.offsetMax = new Vector2(bleed + thick, bleed);
                break;
        }
    }

    // ── util ──────────────────────────────────────────────────────────────────
    static Vector2 ScreenCenter(Button b)
    {
        var rt = b.GetComponent<RectTransform>();
        if (rt == null) return Vector2.zero;
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Vector2(
            (corners[0].x + corners[2].x) * 0.5f,
            (corners[0].y + corners[2].y) * 0.5f);
    }
}
