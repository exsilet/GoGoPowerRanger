using UnityEngine;
using System;

public class WeakSpot_1 : MonoBehaviour
{
    public event Action OnDestroyed;

	void OnTriggerEnter2D(Collider2D other)
	{
		// Проверяем, что столкновение произошло с игроком
		if (other.CompareTag("Player"))
		{
			// Проверяем, находится ли игрок в состоянии буста
			PlayerController player = other.GetComponent<PlayerController>();
			if (player != null && player.isBoosting)
			{
				// Вызываем событие уничтожения слабого места
				OnDestroyed?.Invoke();

				// Уничтожаем слабое место после столкновения
				Destroy(gameObject);
			}
		}
	}
}
