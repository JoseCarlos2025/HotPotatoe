using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public Toggle musicToggle;

    private void Start()
    {
        // Cargar valores guardados
        float savedVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        bool isMusicOn = PlayerPrefs.GetInt("musicEnabled", 1) == 1;

        // Aplicar valores iniciales
        volumeSlider.value = savedVolume;
        musicToggle.isOn = isMusicOn;

        ApplyVolume(savedVolume);
        ApplyMusicToggle(isMusicOn);

        // Suscribirse a eventos
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
    }

    public void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("musicVolume", value);
    }

    public void OnMusicToggleChanged(bool isOn)
    {
        ApplyMusicToggle(isOn);
        PlayerPrefs.SetInt("musicEnabled", isOn ? 1 : 0);
    }

    private void ApplyVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(volume);
        }
    }

    private void ApplyMusicToggle(bool isOn)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ToggleMusic(isOn);
        }
    }
}
