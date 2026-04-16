using UnityEngine;
using UnityEngine.UI;

public class FightSceneBackgroundController : MonoBehaviour
{
    public Image backgroundImage;

    public Sprite fireArenaBackground;
    public Sprite iceArenaBackground;
    public Sprite airArenaBackground;
    public Sprite earthArenaBackground;

    void Start()
    {
        switch (ArenaSelectController.selectedArena)
        {
            case "Fire":
                backgroundImage.sprite = fireArenaBackground;
                break;

            case "Ice":
                backgroundImage.sprite = iceArenaBackground;
                break;

            case "Air":
                backgroundImage.sprite = airArenaBackground;
                break;

            case "Earth":
                backgroundImage.sprite = earthArenaBackground;
                break;
        }
    }
}