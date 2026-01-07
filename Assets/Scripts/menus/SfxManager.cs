using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager instance;

    public AudioSource audioSource;

    public AudioClip grapple;
    public AudioClip bash;
    public AudioClip slowdown;
    public AudioClip damage;
    public AudioClip walk;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        audioSource.PlayOneShot(clip, volume);
    }
}
