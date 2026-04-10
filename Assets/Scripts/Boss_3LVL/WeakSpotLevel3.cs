using UnityEngine;

public class WeakSpotLevel3 : MonoBehaviour
{
    private BossController boss;

    // Активируем слабое место и связываем его с боссом
    public void Activate(BossController assignedBoss)
    {
        boss = assignedBoss;  // Назначаем босса для нанесения урона
    }

    void OnTriggerEnter2D(Collider2D other)
    {
		if (other.CompareTag("Player") && other.GetComponent<PlayerController>().isBoosting)
		{
			// Если игрок попал в слабое место с бустом, наносим урон боссу
			boss.TakeDamage(1);

			// Обновляем флаг в Mechanic5, уведомляем, что слабое место уничтожено
			Mechanic5 mechanic5 = boss.GetComponent<Mechanic5>();
			if (mechanic5 != null)
			{
				mechanic5.HandleWeakSpotHit();
			}

			Destroy(gameObject);  // Удаляем слабое место после попадания
		}
    }
}