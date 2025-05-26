using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource sfxSource;
    public AudioSource musicSource;

    public Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        LoadSFXClips();
        LoadMusicClips();
    }

    private void LoadSFXClips()
    {
        sfxClips["explosion"] = Resources.Load<AudioClip>("SFX/explosion");
        sfxClips["incorrect"] = Resources.Load<AudioClip>("SFX/incorrect");
        sfxClips["correct"] = Resources.Load<AudioClip>("SFX/correct");
        sfxClips["sword"] = Resources.Load<AudioClip>("SFX/sword-sound-260274");
    }

    private void LoadMusicClips()
    {
        musicClips["Menu"] = Resources.Load<AudioClip>("Music/Menu");
        musicClips["ThemeGameplay"] = Resources.Load<AudioClip>("Music/ThemeGameplay");
    }

    public void PlaySFX(string clipName)
    {
        if (sfxClips.ContainsKey(clipName))
        {
            sfxSource.clip = sfxClips[clipName];
            sfxSource.Play();
        }
        else
        {
            Debug.LogWarning("El AudioClip " + clipName + " no se encontró en el diccionario de sfxClips.");
        }
    }

    public void PlayMusic(string clipName)
    {
        if (musicClips.ContainsKey(clipName))
        {
            musicSource.clip = musicClips[clipName];
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("El AudioClip " + clipName + " no se encontró en el diccionario de musicClips.");
        }
    }

    private void Start()
    {
        AudioManager.instance.PlayMusic("Menu");
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void ToggleMusic(bool isOn)
    {
        if (musicSource != null)
        {
            if (isOn)
            {
                if (!musicSource.isPlaying)
                    musicSource.Play();
            }
            else
            {
                musicSource.Pause();
            }
        }
    }

    // 🔊 Método para hacer transición suave entre canciones (fade)
    public void FadeToMusic(string newTrackName, float fadeDuration)
    {
        StartCoroutine(FadeOutInMusic(newTrackName, fadeDuration));
    }

    private IEnumerator FadeOutInMusic(string newTrackName, float duration)
    {
        if (musicSource == null || !musicClips.ContainsKey(newTrackName)) yield break;

        float startVolume = musicSource.volume;

        // Fade Out
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Cambiar clip
        musicSource.clip = musicClips[newTrackName];
        musicSource.Play();

        // Fade In
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / duration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }
}
