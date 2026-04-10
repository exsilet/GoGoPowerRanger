using UnityEngine;

public class CollectibleProjectile : MonoBehaviour
{
    // Событие, которое вызывается при сборе объекта
    public delegate void CollectedAction();
    public event CollectedAction OnCollected;

    [Header("Audio Settings")]
    public AudioSource collectSound; // Звук при сборе

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, если объект, столкнувшийся с "берущимся" объектом, имеет тег "Player"
        if (other.CompareTag("Player"))
        {
            // Воспроизводим звук, если он указан
            if (collectSound != null)
            {
                collectSound.Play();
            }

            // Вызываем событие OnCollected, чтобы уведомить механику
            OnCollected?.Invoke();

            // Скрываем объект и его дочерние элементы
            HideObjectAndChildren(gameObject);

            // Уничтожаем объект после короткой задержки, чтобы звук успел проиграться
            Destroy(gameObject, collectSound != null ? collectSound.clip.length : 0f);
        }
    }

    // Метод для скрытия объекта и его дочерних элементов
    private void HideObjectAndChildren(GameObject obj)
	{
		// Отключаем рендеринг объекта
		SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
		if (sr != null)
		{
			sr.enabled = false;
		}

		// Останавливаем ParticleSystem объекта
		ParticleSystem ps = obj.GetComponent<ParticleSystem>();
		if (ps != null)
		{
			ps.Stop();
			ps.Clear(); // Убираем текущие частицы
		}

		// Отключаем рендеринг и ParticleSystem для всех дочерних объектов
		foreach (Transform child in obj.transform)
		{
			SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
			if (childRenderer != null)
			{
				childRenderer.enabled = false;
			}

			ParticleSystem childPs = child.GetComponent<ParticleSystem>();
			if (childPs != null)
			{
				childPs.Stop();
				childPs.Clear(); // Убираем текущие частицы
			}
		}
	}
}