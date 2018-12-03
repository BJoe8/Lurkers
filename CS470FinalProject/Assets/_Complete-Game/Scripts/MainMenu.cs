using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Play game if the button is clicked
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    //Exit the game if the button is clicked
    public void QuitGame()
    {
        Debug.Log("Quit Pressed");
        Application.Quit();
    }
}
