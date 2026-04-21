using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelectController : MonoBehaviour
{
    public Image p1Preview;
    public Image p2Preview;

    public TMP_Text p1NameText;
    public TMP_Text p2NameText;
    public TMP_Text continueButtonText;

    public Sprite defaultPreview;
    public Sprite fireFull;
    public Sprite iceFull;
    public Sprite airFull;
    public Sprite earthFull;

    private bool selectingP1 = true;

    public void SelectFire()
    {
        SelectCharacter("FIRE", fireFull, 0);
    }

    public void SelectIce()
    {
        SelectCharacter("ICE", iceFull, 1);
    }

    public void SelectAir()
    {
        SelectCharacter("AIR", airFull, 2);
    }

    public void SelectEarth()
    {
        SelectCharacter("EARTH", earthFull, 3);
    }

    private void SelectCharacter(string characterName, Sprite characterSprite, int index)
    {
        if (characterSprite == null)
        {
            Debug.LogError("Character sprite is missing.");
            return;
        }

        if (selectingP1)
        {
            if (p1Preview == null || p1NameText == null)
            {
                Debug.LogError("P1 preview or P1 name text is not assigned.");
                return;
            }

            p1Preview.sprite = characterSprite;
            p1NameText.text = characterName;
            CharacterSelectionData.P1Index = index;
            Debug.Log("Selected for P1: " + characterName);
        }
        else
        {
            if (p2Preview == null || p2NameText == null)
            {
                Debug.LogError("P2 preview or P2 name text is not assigned.");
                return;
            }

            p2Preview.sprite = characterSprite;
            p2NameText.text = characterName;
            CharacterSelectionData.P2Index = index;
            Debug.Log("Selected for P2: " + characterName);
        }
    }

    public void ConfirmSelection()
    {
        if (selectingP1)
        {
            selectingP1 = false;
            Debug.Log("Now selecting Player 2");

            if (continueButtonText != null)
            {
                continueButtonText.text = "START";
            }
        }
        else
        {
            Debug.Log("Both players selected.");
            SceneManager.LoadScene("ArenaSelect");
        }
    }

    public void ResetSelections()
    {
        selectingP1 = true;

        if (p1Preview != null)
            p1Preview.sprite = defaultPreview;

        if (p2Preview != null)
            p2Preview.sprite = defaultPreview;

        if (p1NameText != null)
            p1NameText.text = "NONE";

        if (p2NameText != null)
            p2NameText.text = "NONE";

        if (continueButtonText != null)
            continueButtonText.text = "CONTINUE";
    }

    private void Start()
    {
        ResetSelections();
    }
}