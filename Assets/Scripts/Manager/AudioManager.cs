using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Audio Sources")]
    private AudioSource soundSource;
    private AudioSource musicSource;

    [Header("Background Music Clips")]
    public AudioClip mainBGM;     // Untuk semua scene kecuali Intro
    public AudioClip introBGM;    // Khusus untuk IntroScene

    [Header("Sound Effects")]
    public AudioClip walkSFX;
    public AudioClip sleepSFX;
    public AudioClip cutSFX;
    public AudioClip talkfastSFX;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Get or Add AudioSource
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            soundSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            soundSource = sources[0];
            musicSource = sources[1];
        }

        musicSource.loop = true;

        // Load volume settings
        float musicVol = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        float soundVol = PlayerPrefs.GetFloat("soundVolume", 0.5f);
        ChangeMusicVolume(musicVol);
        ChangeSoundVolume(soundVol);
    }


    public void PlayWalk() => PlaySound(walkSFX);
    public void PlaySleep() => PlaySound(sleepSFX);
    public void PlayCut() => PlaySound(cutSFX);
    public void PlayTalkFast() => PlaySound(talkfastSFX);

    public void StopSound()
    {
        if (soundSource != null)
            soundSource.Stop();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cek apakah sudah memainkan musik yang sesuai, hindari tumpang tindih
        if ((scene.name == "IntroScene" || scene.name == "BadEnding" || scene.name == "GoodEnding") && musicSource.clip != introBGM)
        {
            PlayMusic(introBGM, true);
        }
        else if (scene.name != "IntroScene" && scene.name != "BadEnding" && scene.name != "GoodEnding" && musicSource.clip != mainBGM)
        {
            PlayMusic(mainBGM, true);
        }
    }

    // Main methods
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            soundSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (music == null) return;

        if (musicSource.clip == music && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = music;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void ChangeMusicVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat("musicVolume", value);
    }

    public void ChangeSoundVolume(float value)
    {
        soundSource.volume = value;
        PlayerPrefs.SetFloat("soundVolume", value);
    }

    public void SetMusicVolumeFromSlider(Slider slider)
    {
        ChangeMusicVolume(slider.value);
    }

    public void SetSoundVolumeFromSlider(Slider slider)
    {
        ChangeSoundVolume(slider.value);
    }

    public void SetAllVolumeFromSlider(Slider slider)
    {
        ChangeMusicVolume(slider.value);
        ChangeSoundVolume(slider.value);
    }

    public float GetMusicVolume() => musicSource.volume;
    public float GetSoundVolume() => soundSource.volume;
}
