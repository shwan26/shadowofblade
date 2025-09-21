using UnityEngine;
using UnityEngine.SceneManagement; // 👈 You need this line to manage scenes

public class MainMenuManager : MonoBehaviour
{
    // Call this method when the "Start Game" button is clicked
    public void StartGame()
    {
        // Load the game scene. Make sure the scene name matches what you have in your Build Settings.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Call this method when the "Quit" button is clicked
    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        // Quit the application. This will only work in a build, not in the Unity editor.
        Application.Quit();
    }
}