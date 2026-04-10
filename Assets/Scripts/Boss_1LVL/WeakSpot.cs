using System.Collections;
using UnityEngine;

public class WeakSpot : MonoBehaviour
{
    private BossMechanics bossMechanics;      // Ссылка на скрипт босса для нанесения урона

    void Start()
    {
        bossMechanics = GetComponentInParent<BossMechanics>();
    }

    // Метод для инициализации слабого места и связи с боссом
    public void ActivateForDuration(float duration, BossMechanics boss)
    {
        bossMechanics = boss;
        Destroy(gameObject, duration);  // Уничтожаем слабое место после истечения времени
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что столкновение произошло с игроком
        if (other.CompareTag("Player"))
        {
            // Проверяем, находится ли игрок в состоянии буста
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.isBoosting)
            {
                // Наносим урон боссу и уничтожаем слабое место
                if (bossMechanics != null)
                {
                    bossMechanics.TakeDamage();
                }
                Destroy(gameObject);  // Уничтожаем слабое место после столкновения
            }
        }
    }
}
