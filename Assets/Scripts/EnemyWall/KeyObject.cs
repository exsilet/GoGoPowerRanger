using UnityEngine;
using System.Collections;

public class KeyObject : MonoBehaviour
{
    public MovingGate gate;            // Ссылка на ворота
    public AudioSource pickupSound;    // Звук при поднятии ключа
    public AudioSource afterPickupSound; // Второй звук, воспроизводится после первого

    private bool isPickedUp = false;   // Флаг для предотвращения повторного срабатывания

    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что столкновение произошло с игроком
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true;

            // Делаем объект и его дочерние объекты невидимыми
            SetVisibility(false);

            // Воспроизводим звук при поднятии ключа
            if (pickupSound != null)
            {
                pickupSound.Play();
                // После завершения первого звука воспроизводим второй
                StartCoroutine(PlaySecondSoundAndDestroy());
            }
            else
            {
                // Если первого звука нет, сразу запускаем второй
                PlayAfterPickupSoundAndDestroy();
            }

            // Поднимаем ворота
            gate.LiftGate();
            Debug.Log("Игрок поднял ключ, ворота открываются!");
        }
    }

    private void SetVisibility(bool isVisible)
    {
        // Скрываем или показываем текущий объект
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = isVisible;
        }

        // Скрываем или показываем дочерние объекты
        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            childRenderer.enabled = isVisible;
        }
    }

    private IEnumerator PlaySecondSoundAndDestroy()
    {
        // Ждём завершения первого звука
        yield return new WaitForSeconds(pickupSound.clip.length);

        // Воспроизводим второй звук
        PlayAfterPickupSoundAndDestroy();
    }

    private void PlayAfterPickupSoundAndDestroy()
    {
        if (afterPickupSound != null)
        {
            afterPickupSound.Play();
            Destroy(gameObject, afterPickupSound.clip.length); // Удаляем объект после завершения второго звука
        }
        else
        {
            Destroy(gameObject); // Если второго звука нет, сразу удаляем объект
        }
    }
}
