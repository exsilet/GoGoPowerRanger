using System.Collections;
using UnityEngine;

public class ElectricObstacle : MonoBehaviour
{
    public GameObject electricField;  // Спрайт электричества (например, полоска)
    public float activeDuration = 2f; // Время, когда электричество активно
    public float inactiveDuration = 3f; // Время, когда электричество выключено
    public AudioSource electricitySound; // Звук для активации электричества

    private bool isActive = false; // Состояние электричества
    private bool isSoundPlaying = false; // Флаг для отслеживания состояния звука

    void Start()
    {
        StartCoroutine(ToggleElectricity());
    }

    IEnumerator ToggleElectricity()
    {
        while (true)
        {
            if (isActive)
            {
                // Отключаем электричество
                electricField.SetActive(false);
                isActive = false;

                // Останавливаем звук, если он воспроизводится
                if (electricitySound != null && isSoundPlaying)
                {
                    electricitySound.Stop();
                    isSoundPlaying = false;
                }

                yield return new WaitForSeconds(inactiveDuration);
            }
            else
            {
                // Включаем электричество
                electricField.SetActive(true);
                isActive = true;

                // Проверяем, можно ли воспроизводить звук (если объект находится между границами)
                if (electricitySound != null && transform.position.x <= 10 && transform.position.x >= -10 && !isSoundPlaying)
                {
                    electricitySound.Play();
                    isSoundPlaying = true;
                }

                yield return new WaitForSeconds(activeDuration);
            }
        }
    }

    void Update()
    {
        // Проверяем пересечение границ и включаем/выключаем звук
        if (transform.position.x > 10 || transform.position.x < -10)
        {
            // Останавливаем звук, если объект вне границ
            if (electricitySound != null && isSoundPlaying)
            {
                electricitySound.Stop();
                isSoundPlaying = false;
            }
        }
        else
        {
            // Включаем звук, если объект в пределах границ и электричество активно
            if (electricitySound != null && isActive && !isSoundPlaying)
            {
                electricitySound.Play();
                isSoundPlaying = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isActive)
        {
            Debug.Log("Игрок получил урон от электричества!");
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // Уменьшаем здоровье игрока
            }
        }
    }
}
