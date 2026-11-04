using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderSFX;

    private void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (sliderMusic != null)
        {
            sliderMusic.SetValueWithoutNotify(musicVolume);
            sliderMusic.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sliderSFX != null)
        {
            sliderSFX.SetValueWithoutNotify(sfxVolume);
            sliderSFX.onValueChanged.AddListener(SetSFXVolume);
        }

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        float decibels = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
        audioMixer.SetFloat("MusicVolume", decibels);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        float decibels = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
        audioMixer.SetFloat("SFXVolume", decibels);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}