using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void GoToModeSelect()
    {
         SceneManager.LoadScene("ModeSelect");
    }
    public void GoToCharacterSelect()
    {
         SceneManager.LoadScene("CharacterSelect");
    }
    public void GoToArenaSelect()
    {
         SceneManager.LoadScene("ArenaSelect");
       
    }
    public void GoToFightScene()
    {
         SceneManager.LoadScene("FightScene");
    }
    public void GoToMainMenu()
    {
         SceneManager.LoadScene("MainMenu");
       
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    
}
