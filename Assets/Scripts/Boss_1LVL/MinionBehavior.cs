using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MinionBehavior : MonoBehaviour
{
    public float speed = 3f;               // Скорость движения миньона
    private Vector2 direction;             // Направление движения
    public event Action OnMinionDestroyed; // Событие для уведомления об уничтожении миньона

    [Header("Effects")]
    public AudioSource bounceSound;       // Звук при ударе
    public ParticleSystem bounceParticles; // Частицы при ударе

    private Rigidbody2D rb;               // Rigidbody для физики

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeBouncing();              // Инициализируем направление движения
    }

    public void InitializeBouncing()
    {
        // Задаём случайное начальное направление движения
        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        if (rb != null)
        {
            rb.velocity = direction * speed; // Устанавливаем начальную скорость
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
	{
		// Если столкновение с границей
		if (collision.gameObject.CompareTag("Boundary"))
		{
			// Получаем нормаль столкновения
			Vector2 normal = collision.contacts[0].normal;
			direction = Vector2.Reflect(direction, normal).normalized; // Отражаем направление

			// Обновляем скорость движения
			if (rb != null)
			{
				rb.velocity = direction * speed;
			}

			// Воспроизводим эффекты
			PlayBounceEffect();
		}

		// Если столкновение с объектом "Obstacle"
		if (collision.gameObject.CompareTag("Obstacle"))
		{
			// Получаем нормаль столкновения
			Vector2 normal = collision.contacts[0].normal;
			direction = Vector2.Reflect(direction, normal).normalized; // Отражаем направление

			// Устанавливаем фиксированную скорость 3
			if (rb != null)
			{
				rb.velocity = direction * 3f;
			}

			// Воспроизводим эффекты
			PlayBounceEffect();
		}
	}

    void PlayBounceEffect()
    {
        // Воспроизведение звука
        if (bounceSound != null)
        {
            bounceSound.Play();
        }

        // Активация частиц
        if (bounceParticles != null)
        {
            bounceParticles.transform.position = transform.position; // Перемещаем частицы к позиции объекта
            bounceParticles.Play();
        }
    }

    void OnDestroy()
    {
        // Вызов события уничтожения
        OnMinionDestroyed?.Invoke();
    }
}
