using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Music")]
    public AudioClip backgroundMusic;

    [Header("Treat Sounds")]
    public AudioClip[] treatSounds;

    [Header("Other Sounds")]
    public AudioClip trickSound;
    public AudioClip missSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic();
    }

    // 🎵 MUSIC
    public void PlayMusic()
    {
        if (backgroundMusic == null) return;

        if (!musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // 🔊 SFX
    public void PlayTreat()
    {
        if (treatSounds.Length == 0) return;
        AudioClip clip = treatSounds[Random.Range(0, treatSounds.Length)];
        sfxSource.PlayOneShot(clip);
    }

    public void PlayTrick()
    {
        if (trickSound != null)
            sfxSource.PlayOneShot(trickSound);
    }

    public void PlayMiss()
    {
        if (missSound != null)
            sfxSource.PlayOneShot(missSound);
    }
}
