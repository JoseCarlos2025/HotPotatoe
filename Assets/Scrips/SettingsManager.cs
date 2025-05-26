using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public Toggle musicToggle;
    public Toggle Punter;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        bool isMusicOn = PlayerPrefs.GetInt("musicEnabled", 1) == 1;
        bool isPunter = PlayerPrefs.GetInt("PunterEnabled", 1) == 1;

        volumeSlider.value = savedVolume;
        musicToggle.isOn = isMusicOn;
        Punter.isOn = isPunter;

        ApplyVolume(savedVolume);
        ApplyMusicToggle(isMusicOn);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        Punter.onValueChanged.AddListener(OnPunterToggleChanged);
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

    public void OnPunterToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("PunterEnabled", isOn ? 1 : 0);
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
