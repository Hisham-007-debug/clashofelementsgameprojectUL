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
    }

    public void SelectIceArena()
    {
        selectedArenaIndex = 1;
        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        if (arenaPreview != null) arenaPreview.sprite = iceArena;
        if (arenaName != null) arenaName.text = "ICE";
    }

    public void SelectAirArena()
    {
        selectedArenaIndex = 2;
        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        if (arenaPreview != null) arenaPreview.sprite = airArena;
        if (arenaName != null) arenaName.text = "AIR";
    }

    public void SelectEarthArena()
    {
        selectedArenaIndex = 3;
        PlayerPrefs.SetInt("SelectedArenaIndex", selectedArenaIndex);
        PlayerPrefs.Save();

        if (arenaPreview != null) arenaPreview.sprite = earthArena;
        if (arenaName != null) arenaName.text = "EARTH";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("CharacterSelect");
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

        SceneManager.LoadScene("FightScene");
    }
}