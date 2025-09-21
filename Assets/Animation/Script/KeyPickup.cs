// // KeyPickup.cs
// using UnityEngine;
// using UnityEngine.UI;

// public class KeyPickup : MonoBehaviour
// {
//     [Header("UI References")]
//     public GameObject noticeUI;
//     public GameObject level2Notice;

//     [Header("Player & Door")]
//     public Transform player;
//     public DoorController door; // 🟢 Use the new DoorController script type
//     public float detectionRange = 3f;
//     private GameManager gameManager;

//     private bool playerNear = false;

//     void Start()
//     {
//         // 🎯 Find and store the GameManager instance at the start
//         gameManager = FindObjectOfType<GameManager>();
//         if (gameManager == null)
//         {
//             Debug.LogError("GameManager not found in the scene! Please add it.");
//         }
//     }

//     void Update()
//     {
//         if (player == null)
//         {
//             // Omitted for brevity
//             return;
//         }

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
//                 noticeUI.SetActive(false);

//                 if (level2Notice != null)
//                 {
//                     level2Notice.SetActive(true);
//                 }

//                 // 🟢 Call the OpenDoor method on the DoorController script
//                 if (door != null)
//                 {
//                     door.OpenDoor();
//                 }

//                 gameManager.CollectKey();
//                 // Destroy the key after it has been used
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

public class KeyPickup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject noticeUI;

    [Header("Player & Door")]
    public Transform player;
    public DoorController door; // Optional door to open
    public float detectionRange = 3f;

    private GameManager gameManager;
    private bool playerNear = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene! Please add it.");
        }
    }

    void Update()
    {
        if (player == null) return;

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

                // Open the door if assigned
                if (door != null)
                {
                    door.OpenDoor();
                }

                // Notify GameManager
                gameManager.KeyCollected(this);

                // Destroy the key after collection
                Destroy(gameObject);
            }
        }
        else if (playerNear)
        {
            noticeUI.SetActive(false);
            playerNear = false;
        }
    }
}


