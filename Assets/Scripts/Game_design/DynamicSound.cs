using UnityEngine;

public class DynamicSound : MonoBehaviour
{
    public AudioSource soundSource;  // Ссылка на AudioSource
    public float minPitch = 0.5f;    // Минимальный pitch (когда объект стоит)
    public float maxPitch = 1.5f;    // Максимальный pitch (при разгоне)
    public float stablePitch = 1.2f; // Pitch для стабильной скорости
    public float accelerationThreshold = 0.2f; // Порог скорости для начала увеличения pitch
    public float stableSpeedThreshold = 5f;    // Скорость, на которой pitch становится стабильным
    public float pitchChangeSpeed = 5f;        // Скорость изменения pitch
    private Vector3 lastPosition;              // Последняя позиция объекта
    private float speed;                       // Текущая скорость объекта
    private float targetPitch;                 // Целевой pitch для плавного изменения

    void Start()
    {
        lastPosition = transform.position;  // Инициализируем позицию
        targetPitch = minPitch;             // Начальный pitch
        if (soundSource != null)
        {
            soundSource.loop = true;        // Включаем повторение звука
            soundSource.pitch = minPitch;   // Устанавливаем начальный pitch
            soundSource.Play();             // Запускаем звук
        }
    }

    void Update()
    {
        // Рассчитываем скорость как пройденное расстояние за кадр
        float distance = Vector3.Distance(lastPosition, transform.position);
        speed = distance / Time.deltaTime;

        // Логика изменения pitch
        if (speed > accelerationThreshold && speed < stableSpeedThreshold)
        {
            // Объект ускоряется — увеличиваем pitch
            targetPitch = Mathf.Lerp(minPitch, maxPitch, (speed - accelerationThreshold) / stableSpeedThreshold);
        }
        else if (speed >= stableSpeedThreshold)
        {
            // Объект движется стабильно — фиксируем pitch на стабильном значении
            targetPitch = stablePitch;
        }
        else
        {
            // Объект медленно движется или стоит — возвращаемся к минимальному pitch
            targetPitch = minPitch;
        }

        // Плавно изменяем текущий pitch
        soundSource.pitch = Mathf.Lerp(soundSource.pitch, targetPitch, Time.deltaTime * pitchChangeSpeed);

        // Обновляем последнюю позицию
        lastPosition = transform.position;
    }
}
