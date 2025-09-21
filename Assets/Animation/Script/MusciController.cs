using UnityEngine;

public class MusicController : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        // Get a reference to the Audio Source component on this GameObject
        audioSource = GetComponent<AudioSource>();

        // Start playing the music
        audioSource.Play();
    }
}