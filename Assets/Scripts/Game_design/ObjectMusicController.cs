using UnityEngine;

public class ObjectMusicController : MonoBehaviour
{
    [Header("Trigger Settings")]
    public float targetX = 10f; // Координата X, при пересечении которой выключается музыка

    [Header("New Music")]
    public AudioSource newMusicSource; // Источник новой музыки, который нужно включить

    [Header("Tags for Levels")]
    public string[] levelTags = { "LVL_1", "LVL_2", "LVL_3", "LVL_4", "LVL_5" }; // Теги для уровней

    private AudioSource audioSource; // Кэшируем AudioSource для музыки
    private bool hasTriggered = false; // Флаг, чтобы музыка переключалась только один раз

    private void Start()
    {
        // Проверяем все уровни по указанным тегам
        foreach (string tag in levelTags)
        {
            GameObject musicObject = GameObject.FindGameObjectWithTag(tag);
            if (musicObject != null)
            {
                audioSource = musicObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    Debug.LogError($"AudioSource не найден на объекте с тегом {tag}.");
                }
                else
                {
                    Debug.Log($"Музыка найдена на объекте с тегом {tag}.");
                    break; // Используем первый найденный тег
                }
            }
        }

        if (audioSource == null)
        {
            Debug.LogError("Не удалось найти объект с аудио по указанным тегам.");
        }

        // Загружаем данные новой музыки в память, если они есть
        if (newMusicSource != null && newMusicSource.clip != null)
        {
            newMusicSource.clip.LoadAudioData();
        }
    }

    private void Update()
    {
        // Проверяем, пересекает ли объект целевую координату X
        if (!hasTriggered && transform.position.x <= targetX)
        {
            // Останавливаем музыку, если AudioSource существует
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Музыка отключена.");
            }
            else
            {
                Debug.LogWarning("Музыка уже отключена или AudioSource отсутствует.");
            }

            // Включаем новую музыку, если указано
            if (newMusicSource != null)
            {
                newMusicSource.Play();
                Debug.Log("Новая музыка включена.");
            }
            else
            {
                Debug.LogWarning("Источник новой музыки не указан.");
            }

            // Устанавливаем флаг, чтобы выключение произошло только один раз
            hasTriggered = true;
        }
    }
}
