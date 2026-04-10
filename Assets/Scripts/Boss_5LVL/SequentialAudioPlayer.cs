using System.Collections;
using UnityEngine;

public class SequentialAudioPlayer : MonoBehaviour
{
    public AudioSource[] audioSources; // Массив AudioSource, которые нужно воспроизвести

    private void Start()
    {
        if (audioSources.Length > 0)
        {
            StartCoroutine(PlayAudioSequentially());
        }
    }

    private IEnumerator PlayAudioSequentially()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null)
            {
                audioSource.Play(); // Запуск текущего AudioSource
                
                // Ожидание завершения текущего аудио
                while (audioSource.isPlaying)
                {
                    yield return null;
                }
            }
        }

        Debug.Log("Все аудиотреки завершены.");
    }
}