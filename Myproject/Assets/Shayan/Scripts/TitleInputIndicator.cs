using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Displays P1 (always controller) and P2 (keyboard or second controller) input icons
/// at the bottom of the title screen. Attach to TitleSceneManager.
/// </summary>
public class TitleInputIndicator : MonoBehaviour
{
    private Sprite _controllerSprite;
    private Sprite _keyboardSprite;

    private Image    _p2Icon;
    private TMP_Text _p1StatusText;

    void Start()
    {
        _controllerSprite = Resources.Load<Sprite>("pixel remote");
        _keyboardSprite   = Resources.Load<Sprite>("Pixel keyboard");

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        BuildUI(canvas.transform);
    }

    void Update()
    {
        if (_p1StatusText == null || _p2Icon == null) return;

        bool p1Connected = Gamepad.all.Count > 0;
        _p1StatusText.text  = p1Connected ? "Connected" : "Disconnected";
        _p1StatusText.color = p1Connected
            ? new Color(0.2f, 1f, 0.2f)
            : new Color(1f, 0.3f, 0.3f);

        bool p2UsesGamepad = Gamepad.all.Count >= 2;
        _p2Icon.sprite = p2UsesGamepad ? _controllerSprite : _keyboardSprite;
    }

    // ── UI construction ───────────────────────────────────────────────────────

    void BuildUI(Transform canvasRoot)
    {
        var panel = new GameObject("InputIndicators");
        panel.transform.SetParent(canvasRoot, false);

        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 20f);
        rt.sizeDelta        = new Vector2(500f, 130f);

        _p1StatusText = null;
        Image p2Icon  = null;

        BuildPlayerSection(panel.transform, "PLAYER 1", _controllerSprite, true,
            new Vector2(-125f, 0f), out _p1StatusText, out _);

        BuildPlayerSection(panel.transform, "PLAYER 2", _keyboardSprite, false,
            new Vector2(125f, 0f), out _, out p2Icon);

        _p2Icon = p2Icon;
    }

    void BuildPlayerSection(
        Transform parent,
        string label,
        Sprite  defaultSprite,
        bool    showStatus,
        Vector2 centerPos,
        out TMP_Text statusOut,
        out Image    iconOut)
    {
        // Container
        var container  = new GameObject(label.Replace(" ", "") + "_Section");
        container.transform.SetParent(parent, false);
        var crt        = container.AddComponent<RectTransform>();
        crt.anchorMin  = new Vector2(0.5f, 0f);
        crt.anchorMax  = new Vector2(0.5f, 0f);
        crt.pivot      = new Vector2(0.5f, 0f);
        crt.anchoredPosition = centerPos;
        crt.sizeDelta  = new Vector2(180f, 130f);

        // "PLAYER 1 / PLAYER 2" text at the top
        var labelGO  = MakeText(container.transform, label + "_Label", label, 20);
        var labelRT  = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin        = new Vector2(0f, 1f);
        labelRT.anchorMax        = new Vector2(1f, 1f);
        labelRT.pivot            = new Vector2(0.5f, 1f);
        labelRT.anchoredPosition = new Vector2(0f, 0f);
        labelRT.sizeDelta        = new Vector2(0f, 26f);

        // Icon in the middle
        var iconGO = new GameObject(label.Replace(" ", "") + "_Icon");
        iconGO.transform.SetParent(container.transform, false);
        var iconRT          = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin    = new Vector2(0.5f, 0.5f);
        iconRT.anchorMax    = new Vector2(0.5f, 0.5f);
        iconRT.pivot        = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = new Vector2(0f, -8f);
        iconRT.sizeDelta    = new Vector2(72f, 72f);
        var img             = iconGO.AddComponent<Image>();
        img.sprite          = defaultSprite;
        img.preserveAspect  = true;
        img.raycastTarget   = false;
        iconOut             = img;

        // Status text at the bottom (P1 only)
        statusOut = null;
        if (showStatus)
        {
            var statusGO = MakeText(container.transform, "P1_Status", "Disconnected", 17);
            var statusRT = statusGO.GetComponent<RectTransform>();
            statusRT.anchorMin        = new Vector2(0f, 0f);
            statusRT.anchorMax        = new Vector2(1f, 0f);
            statusRT.pivot            = new Vector2(0.5f, 0f);
            statusRT.anchoredPosition = new Vector2(0f, 2f);
            statusRT.sizeDelta        = new Vector2(0f, 22f);
            var tmp                   = statusGO.GetComponent<TMP_Text>();
            tmp.color                 = new Color(1f, 0.3f, 0.3f); // default disconnected
            statusOut                 = tmp;
        }
    }

    // Creates a centred TMP_Text child and returns its GameObject.
    static GameObject MakeText(Transform parent, string name, string text, float fontSize)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp               = go.AddComponent<TextMeshProUGUI>();
        tmp.text              = text;
        tmp.fontSize          = fontSize;
        tmp.alignment         = TextAlignmentOptions.Center;
        tmp.color             = Color.white;
        tmp.raycastTarget     = false;
        return go;
    }
}
