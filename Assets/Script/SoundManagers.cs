using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManagers : MonoBehaviour
{
    public static SoundManagers instance;

    private AudioSource sfxSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayFootstep(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clip, 0.7f);
    }
}
