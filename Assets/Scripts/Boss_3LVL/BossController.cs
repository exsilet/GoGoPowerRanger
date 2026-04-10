using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
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
            GetComponent<Mechanic1>(),
            GetComponent<Mechanic2>(),
            GetComponent<Mechanic3>(),
            GetComponent<Mechanic4>(),
            GetComponent<Mechanic5>()
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
				//bossHP--; // Уменьшение HP после выполнения полного цикла
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

		// Если здоровье босса меньше или равно нулю, уничтожаем его
		if (bossHP <= 0)
		{
			Debug.Log("Босс побежден!");
			
			// Удаляем объекты из механики 5 перед уничтожением босса
			Mechanic5 mechanic5 = GetComponent<Mechanic5>();
			if (mechanic5 != null)
			{
				mechanic5.Cleanup();
			}
			
			Destroy(gameObject);
		}
		else
		{
			// Завершаем текущую механику и возвращаем босса в центр
			Mechanic5 currentMechanic = GetComponent<Mechanic5>();
			if (currentMechanic != null)
			{
				currentMechanic.EndMechanic();
			}
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
