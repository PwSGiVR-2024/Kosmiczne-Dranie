using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSystemController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("LightTesting");
    }

    public void MoveToMainMenu()
    {
        Debug.Log("aaa");
        SceneManager.LoadScene("WelcomeScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
