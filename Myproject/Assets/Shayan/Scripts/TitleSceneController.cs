using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleSceneController : MonoBehaviour
{
    private bool hasStarted = false;

    void Update()
    {
        if (hasStarted) return;

        bool pressed =
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (pressed)
        {
            hasStarted = true;
            SceneManager.LoadScene("ModeSelect");
        }
    }
}