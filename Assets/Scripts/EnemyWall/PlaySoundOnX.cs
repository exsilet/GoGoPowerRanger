using UnityEngine;

public class SoundOnXCross : MonoBehaviour
{
    public AudioSource sound; // Аудиоисточник для звука
    public float triggerX = 10f; // Координата X, при пересечении которой включается звук
    private bool hasPlayedSound = false; // Флаг для предотвращения повторного воспроизведения

    void Update()
    {
        // Проверяем, пересёк ли объект указанную координату X и звук ещё не играл
        if (transform.position.x <= triggerX && !hasPlayedSound)
        {
            if (sound != null)
            {
                sound.Play(); // Воспроизводим звук
                hasPlayedSound = true; // Устанавливаем флаг, чтобы звук не играл повторно
                Debug.Log($"Звук включился при пересечении X = {triggerX}.");
            }
        }
    }
}
