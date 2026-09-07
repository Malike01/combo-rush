using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource;
    public AudioSource loopingSfxSource;
    public AudioClip ShuffleExplosion;       // Sound effect for the shuffle explosion
    public AudioSource musicSource;

    [Header("Music")]
    public AudioClip OST;   // Background music clip

    [Header("SFX Clips")]
    public AudioClip jellyPopSound;      // Normal Jelly Pop Sound
    public AudioClip normalComboSound;   // 5-6 Normal Combo
    public AudioClip greatComboSound;    // 7-9 Great Combo
    public AudioClip perfectComboSound;  // 10-14 Perfect Combo
    public AudioClip legendaryComboSound;// 15-19 Legendary Combo
    public AudioClip immortalComboSound; // 20+ Immortal Combo


    [Header("Time Pressure Audio Clips")]
    public AudioClip fastTickClip;    // Fast ticking sound for warning
    public AudioClip timeOutClip;

    [Header("Purchase Audio Clips")]
    public AudioClip purchaseSuccessSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic()
    {
        if (OST == null || musicSource == null) return;

        musicSource.clip = OST;
        musicSource.loop = true;
        musicSource.volume = 0.25f;
        musicSource.Play();
    }

    //Play a sound effect with optional pitch and volume adjustments
    public void PlaySFX(AudioClip clip, float pitch = 1.0f, float volume = 1.0f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
    }

    // Play combo quality sound based on the number of jellies matched
    public void PlayComboQualitySound(int jellyCount)
    {
        if (jellyCount < 5);
            PlaySFX(jellyPopSound, 1.0f);
        if (jellyCount >= 5 && jellyCount <= 6)
            PlaySFX(normalComboSound, 1.0f);
        else if (jellyCount >= 7 && jellyCount <= 9)
            PlaySFX(greatComboSound, 1.05f);
        else if (jellyCount >= 10 && jellyCount <= 14)
            PlaySFX(perfectComboSound, 1.1f);
        else if (jellyCount >= 15 && jellyCount <= 19)
            PlaySFX(legendaryComboSound, 1.15f);
        else if (jellyCount >= 20)
            PlaySFX(immortalComboSound, 1.2f, 1.2f);
    }

    public void PlayTimeOutSound()
    {
        if (timeOutClip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(timeOutClip);
        }
    }
    /// <summary>
    /// Starts the panic sound for time pressure.
    /// </summary>
    public void StartPanicSound()
    {
        if (fastTickClip != null && loopingSfxSource != null)
        {
            if (!loopingSfxSource.isPlaying)
            {
                loopingSfxSource.clip = fastTickClip;
                loopingSfxSource.loop = true; 
                loopingSfxSource.Play();
            }
        }
    }

    public void StopPanicSound()
    {
        if (loopingSfxSource != null && loopingSfxSource.isPlaying)
        {
            loopingSfxSource.Stop();
        }
    }

    public void PlaySound(string soundName)
    {
        AudioClip clipToPlay = null;

        switch (soundName)
        {
            case "ShuffleExplosion":
                clipToPlay = ShuffleExplosion;
                break;

            case "PurchaseSuccess":
                clipToPlay = purchaseSuccessSound;
                break;
        }

        if (clipToPlay != null)
        {
            PlaySFX(clipToPlay);
        }
    }

}