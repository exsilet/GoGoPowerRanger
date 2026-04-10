using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

//обязательно нужно изменить название (справа верхний угол в разделе миксера Exposed PArameters)
public class SoundManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat("Music", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("Audio", 0.75f);

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer.SetFloat("Music", volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f))
        {
            PlayerPrefs.SetFloat("Music", volume);
        }
        else
        {
            Debug.LogError($"Параметр 'Music' не найден!");
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer.SetFloat("Audio", volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f))
        {
            PlayerPrefs.SetFloat("Audio", volume);
        }
        else
        {
            Debug.LogError($"Параметр 'Audio' не найден!");
        }
    }
}
