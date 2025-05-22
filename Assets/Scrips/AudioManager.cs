using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Instancia única de la clase AudioManager
                                         // Dos AudioSources para que se puedan reproducir efectos y música al mismo tiempo
    public AudioSource sfxSource; // Componente AudioSource para efectos de sonido
    public AudioSource musicSource; // Componente AudioSource para la música de fondo
                                    // En vez de usar un vector de AudioClips vamos a utilizar un Diccionario
                                    // en el que cargaremos directamente los recursos desde la jerarquía del proyecto
    public Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();
    // Método Awake que se llama al inicio antes de que se active el objeto. Útil para
    // inicializarvariables u objetos que serán llamados por otros scripts
    // (game managers, clases singleton, etc).
    private void Awake()
    {
        // Garantizamos que solo exista una instancia del AudioManager
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject); // No destruimos el AudioManager aunque cambie de escena
        LoadSFXClips(); // Cargamos los AudioClips (SFX) en los diccionarios
        LoadMusicClips(); // Cargamos los AudioClips (Music) en los diccionarios
    }
    // Método para cargar los efectos de sonido directamente desde las carpetas
    private void LoadSFXClips()
    {
        // Los recursos deben estar dentro de una carpeta denominada /Assets/Resources/SFX
        sfxClips["Jump"] = Resources.Load<AudioClip>("SFX/Jump");
        sfxClips["CollectCoin"] = Resources.Load<AudioClip>("SFX/Collect_Coin");
    }
    // Método privado para cargar la música de fondo directamente desde las carpetas
    private void LoadMusicClips()
    {
        // Los recursos deben estar dentro de una carpeta denominada /Assets/Resources/Music
        musicClips["Menu"] = Resources.Load<AudioClip>("Music/Menu");
        musicClips["ThemeGameplay"] = Resources.Load<AudioClip>("Music/ThemeGameplay");
    }
    // Método de la clase singleton para reproducir efectos de sonido
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
    // Método de la clase singleton para reproducir música de fondo
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
    // Reproducimos la música de fondo (aunque lo normal es que vaya en el GameManager)
    private void Start()
    {
        AudioManager.instance.PlayMusic("Menu");
    }
}

