using UnityEngine;

public class FightSceneBackgroundController : MonoBehaviour
{
    public SpriteRenderer backgroundRenderer;

    public Sprite fireArenaBackground;
    public Sprite iceArenaBackground;
    public Sprite airArenaBackground;
    public Sprite earthArenaBackground;

    void Start()
    {
        int selectedArenaIndex = PlayerPrefs.GetInt("SelectedArenaIndex", -1);

        switch (selectedArenaIndex)
        {
            case 0:
                backgroundRenderer.sprite = fireArenaBackground;
                break;

            case 1:
                backgroundRenderer.sprite = iceArenaBackground;
                break;

            case 2:
                backgroundRenderer.sprite = airArenaBackground;
                break;

            case 3:
                backgroundRenderer.sprite = earthArenaBackground;
                break;

            default:
                Debug.LogError("No valid arena selected.");
                break;
        }
    }
}