// using UnityEngine;
// using UnityEngine.UI;

// public class SwordPickup : MonoBehaviour
// {
//     public GameObject noticeUI;    // Drag "PickupNotice" here
//     public Transform player;       // Drag Player here
//     public float detectionRange = 3f;

//     private bool playerNear = false;

//     void Update()
//     {
//         // Distance check
//         float distance = Vector3.Distance(player.position, transform.position);

//         if (distance <= detectionRange)
//         {
//             if (!playerNear)
//             {
//                 noticeUI.SetActive(true);
//                 playerNear = true;
//             }

//             if (Input.GetKeyDown(KeyCode.E))
//             {
//                 // Pick up logic
//                 noticeUI.SetActive(false);
//                 Destroy(gameObject);
//             }
//         }
//         else if (playerNear)
//         {
//             noticeUI.SetActive(false);
//             playerNear = false;
//         }
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwordPickup : MonoBehaviour
{
    public GameObject noticeUI;
    public GameObject level2Notice;  // For "Go to Level 2!" text
    public Transform player;
    public float detectionRange = 3f;
    public float loadSceneDelay = 2f;

    private bool playerNear = false;

    void Update()
    {
        // ✅ Check if player still exists
        if (player == null)
        {
            // Hide the UI if the player is gone
            if (noticeUI.activeSelf) noticeUI.SetActive(false);
            if (level2Notice.activeSelf) level2Notice.SetActive(false);
            playerNear = false;
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= detectionRange)
        {
            if (!playerNear)
            {
                noticeUI.SetActive(true);
                playerNear = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                noticeUI.SetActive(false);

                if (level2Notice != null)
                {
                    level2Notice.SetActive(true);
                }

                Invoke(nameof(LoadNextLevel), loadSceneDelay);
            }
        }
        else if (playerNear)
        {
            noticeUI.SetActive(false);
            playerNear = false;
        }
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene("Level2Scene");
    }
}
