using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController_3 : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float targetX = 0f;
    public int bossHP = 5;

    // Список механик
    private List<IBossMechanic> mechanics;

    void Start()
    {
        // Инициализируем механики, прикрепленные к объекту
        mechanics = new List<IBossMechanic>
        {
            GetComponent<Mechanic5_1>(),
            GetComponent<Mechanic5_2>(),
            GetComponent<Mechanic5_3>(),
            GetComponent<Mechanic5_4>(),
            GetComponent<Mechanic5_5>()
        };

        StartCoroutine(MoveToPositionX());
    }

    private IEnumerator MoveToPositionX()
    {
        while (transform.position.x > targetX)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetX, transform.position.y, transform.position.z), moveSpeed * Time.deltaTime);
            yield return null;
        }

        StartCoroutine(BossBehaviorCycle());
    }

    private IEnumerator BossBehaviorCycle()
    {
        while (bossHP > 0)
        {
            // Механика 1 (первая и между механиками)
            yield return StartCoroutine(mechanics[0].Execute());

            // Генерация случайного порядка для механик 2–4
            List<int> mechanicsOrder = GenerateRandomOrder();
            foreach (int mechanicIndex in mechanicsOrder)
            {
                yield return StartCoroutine(mechanics[mechanicIndex].Execute());
                yield return StartCoroutine(mechanics[0].Execute());
                yield return new WaitForSeconds(Random.Range(0.01f, 0.02f));
            }

            // Механика 5 завершает круг механик
            yield return StartCoroutine(mechanics[4].Execute());
			// Проверка, если механика 5 вызвала уменьшение HP
			if (bossHP > 0)  // Добавляем проверку на здоровье после выполнения механики 5
			{
				yield return new WaitForSeconds(1f); // Пауза перед началом следующего цикла
			}
        }

        Debug.Log("Босс побежден!");
        Destroy(gameObject);
    }
	
	public void TakeDamage(int damage)
	{
		bossHP -= damage;
		Debug.Log("Босс получил урон! Оставшееся здоровье: " + bossHP);

		// Проверка: если здоровье босса меньше или равно нулю, уничтожаем его
		if (bossHP <= 0)
		{
			Debug.Log("Босс побежден!");
			Destroy(gameObject); // Уничтожаем объект босса
		}
	}
	
    private List<int> GenerateRandomOrder()
    {
        List<int> mechanicsOrder = new List<int> { 1, 2, 3 };
        for (int i = 0; i < mechanicsOrder.Count; i++)
        {
            int temp = mechanicsOrder[i];
            int randomIndex = Random.Range(i, mechanicsOrder.Count);
            mechanicsOrder[i] = mechanicsOrder[randomIndex];
            mechanicsOrder[randomIndex] = temp;
        }
        return mechanicsOrder;
    }
}
