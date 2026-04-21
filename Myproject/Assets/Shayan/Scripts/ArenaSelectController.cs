using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ArenaSelectController : MonoBehaviour
{
    public Image arenaPreview;
    public TMP_Text arenaName;

    public Sprite defaultPreview;
    public Sprite fireArena;
    public Sprite iceArena;
    public Sprite airArena;
    public Sprite earthArena;

    private int selectedArenaIndex = -1;

    private void Start()
    {
        selectedArenaIndex = -1;
        PlayerPrefs.DeleteKey("SelectedArenaIndex");

        if (arenaPreview != null)
            arenaPreview.sprite = defaultPreview;

        if (arenaName != null)
            arenaName.text = "NONE";
    }

    public void SelectFireArena()
    {
        selectedArenaIndex = 0;
        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        if (arenaPreview != null) arenaPreview.sprite = fireArena;
        if (arenaName != null) arenaName.text = "FIRE";

        Debug.Log("Selected Fire arena");
    }

    public void SelectIceArena()
    {
        selectedArenaIndex = 1;
        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        if (arenaPreview != null) arenaPreview.sprite = iceArena;
        if (arenaName != null) arenaName.text = "ICE";

        Debug.Log("Selected Ice arena");
    }

    public void SelectAirArena()
    {
        selectedArenaIndex = 2;
        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        if (arenaPreview != null) arenaPreview.sprite = airArena;
        if (arenaName != null) arenaName.text = "AIR";

        Debug.Log("Selected Air arena");
    }

    public void SelectEarthArena()
    {
        selectedArenaIndex = 3;
        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        if (arenaPreview != null) arenaPreview.sprite = earthArena;
        if (arenaName != null) arenaName.text = "EARTH";

        Debug.Log("Selected Earth arena");
    }

    public void StartFight()
    {
        if (selectedArenaIndex == -1)
        {
            Debug.LogError("No arena selected.");
            return;
        }

        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        Debug.Log("Loading FightScene with arena index: " + selectedArenaIndex);
        SceneManager.LoadScene("FightScene");
    }
}