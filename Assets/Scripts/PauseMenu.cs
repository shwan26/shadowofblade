using UnityEngine;
using UnityEngine.SceneManagement; // For quitting or loading menus

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Drag your PauseMenuPanel here
    private bool isPaused = false;

    void Update()
    {
        // Toggle with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resume time
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze time
        isPaused = true;
    }

    public void QuitGame()
    {
        // If running in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // Works in a built build
#endif
    }
}
