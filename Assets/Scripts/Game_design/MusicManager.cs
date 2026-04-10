using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;          // Источник звука для музыки
    public AudioSource soundEffectsSource;   // Источник звука для звуковых эффектов
    public AudioClip menuMusic;              // Музыка для главного меню
    public AudioClip gameMusic;              // Музыка для игры
    public AudioClip gameOverMusic;          // Музыка для экрана Game Over

    public AudioMixer audioMixer;            // Ссылка на AudioMixer для управления громкостью

    public Slider musicSlider;               // Слайдер для громкости музыки
    public Slider soundEffectsSlider;        // Слайдер для громкости звуков

    private static MusicManager instance;

    void Awake()
    {
        // Убедимся, что MusicManager существует только в одном экземпляре
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожаем объект при загрузке новой сцены
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Устанавливаем начальные значения громкости для слайдеров
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("Sounds", 0.75f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.value = PlayerPrefs.GetFloat("Audios", 0.75f);
            soundEffectsSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Метод для проигрывания музыки главного меню
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    // Метод для проигрывания музыки игры
    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    // Метод для проигрывания музыки Game Over
    public void PlayGameOverMusic()
    {
        PlayMusic(gameOverMusic);
    }

    // Вспомогательный метод для проигрывания нужной музыки
    private void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip != clip)
        {
            musicSource.Stop();               // Останавливаем текущую музыку
            musicSource.clip = clip;          // Устанавливаем новую музыку
            musicSource.Play();               // Проигрываем музыку
        }
    }

    // Метод для установки громкости музыки
    public void SetMusicVolume(float volume)
    {
		if (volume <= 0.0001f)
		{
			audioMixer.SetFloat("Sounds", -80f); // Полное выключение звука
		}
		else
		{
			audioMixer.SetFloat("Sounds", Mathf.Log10(volume) * 20);  // Изменяем громкость в логарифмической шкале
		}
		PlayerPrefs.SetFloat("Sounds", volume);  // Сохраняем настройку
		}

    // Метод для установки громкости звуковых эффектов
    public void SetSFXVolume(float volume)
    {
        if (volume <= 0.0001f)
		{
			audioMixer.SetFloat("Audios", -80f); // Полное выключение звука
		}
		else
		{
			audioMixer.SetFloat("Audios", Mathf.Log10(volume) * 20);  // Изменяем громкость в логарифмической шкале
		}
		PlayerPrefs.SetFloat("Audios", volume);  // Сохраняем настройку
    }
	
	public void ResumeGameMusic()
	{
		if (!musicSource.isPlaying && musicSource.clip == gameMusic)
		{
			musicSource.Play(); // Возобновляем музыку, если клип — это музыка игры
		}
	}
}
