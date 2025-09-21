

// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using System.Collections;
// using System.Collections.Generic;

// public class GameManager : MonoBehaviour
// {
//     [Header("UI")]
//     public GameObject keyCounterUI;
//     public Text keyCounterText;

//     [Header("Level Progression")]
//     public string nextLevelSceneName = "Level3Scene";

//     [Header("Notices")]
//     public GameObject level2Notice;
//     public float noticeDuration = 2f;

//     [Header("Boss Settings")]
//     public GameObject boss;               // Assign the boss in Inspector
//     public GameObject successUI;          // UI to display when boss dies

//     private Coroutine noticeCoroutine;
//     private List<KeyPickup> keysInScene = new List<KeyPickup>();
//     private bool bossDefeated = false;

//     void Start()
//     {
//         keysInScene.AddRange(FindObjectsOfType<KeyPickup>());
//         UpdateKeyUI();

//         if (level2Notice != null)
//             level2Notice.SetActive(false);

//         if (successUI != null)
//             successUI.SetActive(false); // Hide success screen initially
//     }

//     // Called by each key when collected
//     public void KeyCollected(KeyPickup key)
//     {
//         if (keysInScene.Contains(key))
//         {
//             keysInScene.Remove(key);
//             UpdateKeyUI();

//             if (level2Notice != null)
//             {
//                 if (noticeCoroutine != null)
//                     StopCoroutine(noticeCoroutine);

//                 noticeCoroutine = StartCoroutine(ShowLevel2Notice());
//             }

//             if (keysInScene.Count == 0 && boss == null)
//             {
//                 // If no boss exists in this level, load next level directly
//                 LoadNextLevel();
//             }
//         }
//     }

//     public void BossDefeated()
//     {
//         bossDefeated = true;

//         if (successUI != null)
//             successUI.SetActive(true);

//         // Optional: Delay before loading next level
//         StartCoroutine(LoadNextLevelAfterDelay(2f));
//     }

//     IEnumerator LoadNextLevelAfterDelay(float delay)
//     {
//         yield return new WaitForSecondsRealtime(delay);
//         LoadNextLevel();
//     }

//     void UpdateKeyUI()
//     {
//         if (keyCounterUI != null)
//             keyCounterUI.SetActive(true);

//         if (keyCounterText != null)
//             keyCounterText.text = $"Keys Remaining: {keysInScene.Count}";
//     }

//     private IEnumerator ShowLevel2Notice()
//     {
//         level2Notice.SetActive(true);
//         yield return new WaitForSecondsRealtime(noticeDuration);
//         level2Notice.SetActive(false);
//         noticeCoroutine = null;
//     }

//     void LoadNextLevel()
//     {
//         SceneManager.LoadScene(nextLevelSceneName);
//     }

//     public void ReplayGame()
//     {
//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//     }
// }


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject keyCounterUI;
    public Text keyCounterText;

    [Header("Level Progression")]
    public string nextLevelSceneName = "Level3Scene";

    [Header("Notices")]
    public GameObject level2Notice;
    public float noticeDuration = 2f;

    [Header("Boss Settings")]
    public GameObject boss;               // Assign the boss in Inspector
    public GameObject successUI;          // UI to display when boss dies

    private Coroutine noticeCoroutine;
    private List<KeyPickup> keysInScene = new List<KeyPickup>();
    private bool bossDefeated = false;

    void Start()
    {
        // Find all keys in the scene and add them to the list
        keysInScene.AddRange(FindObjectsOfType<KeyPickup>());
        UpdateKeyUI();

        // Hide notice and success UIs at the start
        if (level2Notice != null)
            level2Notice.SetActive(false);

        if (successUI != null)
            successUI.SetActive(false);

        // CRUCIAL: Get the BossEnemyHealth component and subscribe to its death event
        if (boss != null)
        {
            BossEnemyHealth bossHealth = boss.GetComponent<BossEnemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.onDied += BossDefeated;
            }
        }
    }

    // Called by each key when collected
    public void KeyCollected(KeyPickup key)
    {
        if (keysInScene.Contains(key))
        {
            keysInScene.Remove(key);
            UpdateKeyUI();

            if (level2Notice != null)
            {
                if (noticeCoroutine != null)
                    StopCoroutine(noticeCoroutine);

                noticeCoroutine = StartCoroutine(ShowLevel2Notice());
            }

            // Check for level completion if no boss is present
            if (keysInScene.Count == 0 && boss == null)
            {
                LoadNextLevel();
            }
        }
    }

    // This method is now called by the BossEnemyHealth's onDied event
    public void BossDefeated()
    {
        bossDefeated = true;

        if (successUI != null)
            successUI.SetActive(true);

        // Optional: Delay before loading next level
        StartCoroutine(LoadNextLevelAfterDelay(2f));
    }

    IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        LoadNextLevel();
    }

    void UpdateKeyUI()
    {
        if (keyCounterUI != null)
            keyCounterUI.SetActive(true);

        if (keyCounterText != null)
            keyCounterText.text = $"Keys Remaining: {keysInScene.Count}";
    }

    private IEnumerator ShowLevel2Notice()
    {
        level2Notice.SetActive(true);
        yield return new WaitForSecondsRealtime(noticeDuration);
        level2Notice.SetActive(false);
        noticeCoroutine = null;
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}