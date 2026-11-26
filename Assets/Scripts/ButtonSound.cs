using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickClip;

    public void PlayClickSound()
    {
        if (audioSource != null && clickClip != null)
            audioSource.PlayOneShot(clickClip);
    }
}