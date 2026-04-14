using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArenaSelectController : MonoBehaviour
{
    public Image arenaPreview;
    public TMP_Text arenaName;

    public Sprite fireArena;
    public Sprite iceArena;
    public Sprite airArena;
    public Sprite earthArena;

    private string selectedArena = "";

    public void SelectFireArena()
    {
        arenaPreview.sprite = fireArena;
        arenaName.text = "FIRE ARENA";
        selectedArena = "Fire";
    }

    public void SelectIceArena()
    {
        arenaPreview.sprite = iceArena;
        arenaName.text = "ICE ARENA";
        selectedArena = "Ice";
    }

    public void SelectAirArena()
    {
        arenaPreview.sprite = airArena;
        arenaName.text = "AIR ARENA";
        selectedArena = "Air";
    }

    public void SelectEarthArena()
    {
        arenaPreview.sprite = earthArena;
        arenaName.text = "EARTH ARENA";
        selectedArena = "Earth";
    }
}
