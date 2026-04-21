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

    // P1 and P2 select simultaneously; both must confirm before advancing
    private bool _p1Picked, _p2Picked;
    private bool _p1Confirmed, _p2Confirmed;

    public void SelectFire()  => SelectCharacter("FIRE",  fireFull,  0);
    public void SelectIce()   => SelectCharacter("ICE",   iceFull,   1);
    public void SelectAir()   => SelectCharacter("AIR",   airFull,   2);
    public void SelectEarth() => SelectCharacter("EARTH", earthFull, 3);

    private void SelectCharacter(string characterName, Sprite characterSprite, int index)
    {
        if (characterSprite == null)
        {
            Debug.LogError("Character sprite is missing.");
            return;
        }

        // Route to the player whose cursor fired the confirm
        if (MenuNavigator.ConfirmingPlayer == 0)
        {
            if (p1Preview == null || p1NameText == null)
            {
                Debug.LogError("P1 preview or P1 name text is not assigned.");
                return;
            }
            p1Preview.sprite = characterSprite;
            p1NameText.text  = characterName;
            CharacterSelectionData.P1Index = index;
            _p1Picked = true;
            Debug.Log("P1 selected: " + characterName);
        }
        else
        {
            if (p2Preview == null || p2NameText == null)
            {
                Debug.LogError("P2 preview or P2 name text is not assigned.");
                return;
            }
            p2Preview.sprite = characterSprite;
            p2NameText.text  = characterName;
            CharacterSelectionData.P2Index = index;
            _p2Picked = true;
            Debug.Log("P2 selected: " + characterName);
        }

        UpdateContinueText();
    }

    public void ConfirmSelection()
    {
        if (MenuNavigator.ConfirmingPlayer == 0)
        {
            if (!_p1Picked) { Debug.Log("P1 hasn't picked a character yet."); return; }
            _p1Confirmed = true;
            Debug.Log("P1 confirmed.");
        }
        else
        {
            if (!_p2Picked) { Debug.Log("P2 hasn't picked a character yet."); return; }
            _p2Confirmed = true;
            Debug.Log("P2 confirmed.");
        }

        if (_p1Confirmed && _p2Confirmed)
        {
            Debug.Log("Both players confirmed — loading ArenaSelect.");
            SceneManager.LoadScene("ArenaSelect");
        }
        else
        {
            UpdateContinueText();
        }
    }

    public void ResetSelections()
    {
        _p1Picked = _p2Picked = false;
        _p1Confirmed = _p2Confirmed = false;

        if (p1Preview != null) p1Preview.sprite = defaultPreview;
        if (p2Preview != null) p2Preview.sprite = defaultPreview;
        if (p1NameText != null) p1NameText.text = "NONE";
        if (p2NameText != null) p2NameText.text = "NONE";
        if (continueButtonText != null) continueButtonText.text = "CONTINUE";
    }

    private void UpdateContinueText()
    {
        if (continueButtonText == null) return;
        if (_p1Confirmed && !_p2Confirmed)
            continueButtonText.text = "WAITING FOR P2...";
        else if (_p2Confirmed && !_p1Confirmed)
            continueButtonText.text = "WAITING FOR P1...";
        else
            continueButtonText.text = "CONTINUE";
    }

    private void Start()
    {
        ResetSelections();
    }
}