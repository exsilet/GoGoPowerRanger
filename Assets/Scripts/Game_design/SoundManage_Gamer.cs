using UnityEngine;
using UnityEngine.UI;  // Для работы с кнопками
using System.Collections;

public class SoundManager_Gamer : MonoBehaviour
{
    public AudioSource soundSource;  // Ссылка на компонент AudioSource
    public SoundButton[] soundButtons; // Массив кнопок и соответствующих звуков

    [System.Serializable]
    public class SoundButton
    {
        public Button button;       // Кнопка UI
        public AudioClip soundClip; // Звук для кнопки
    }

    void Start()
    {
        // Подписываем кнопки на события
        foreach (SoundButton soundButton in soundButtons)
        {
            soundButton.button.onClick.AddListener(() => PlaySound(soundButton));
        }
    }

    // Метод для воспроизведения звука
    void PlaySound(SoundButton soundButton)
    {
        if (soundButton.soundClip != null)
        {
            soundSource.PlayOneShot(soundButton.soundClip);
        }
    }
}
