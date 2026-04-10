using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BossLevel2 : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject fallingObjectPrefab;
    public GameObject ringPrefab;
    public GameObject weakSpotPrefab;
    public GameObject collectiblePrefab;
    public float approachSpeed = 2f;
    public float movementSpeed = 3f;
    public Vector3 stopPosition;
    public float attackCooldown = 2f;
    public float weakSpotDuration = 5f;
    public float projectileSpeed = 5f;
    public float ringMaxScale = 3f;
    public float ringExpandDuration = 1.5f;
    public int health = 5;
    public int collectibleCount = 3;

    private bool isActive = false;
    private bool isWeakSpotActive = false;
    private bool isMechanicsPaused = false;
    private Camera mainCamera;
    private Vector2 movementDirection;
    private int collectedItems;
	public AudioSource shootSound;
	[Header("Mechanic 5 sound")]
	public AudioSource audioSource;         // Источник звука
	public AudioClip collectSound;          // Звук сбора объекта

    void Start()
    {
        mainCamera = Camera.main;
        movementDirection = new Vector2(1, 1).normalized;
        StartCoroutine(ApproachStopPosition());
    }

    IEnumerator ApproachStopPosition()
    {
        while (transform.position.x > stopPosition.x)
        {
            transform.position = Vector3.MoveTowards(transform.position, stopPosition, approachSpeed * Time.deltaTime);
            yield return null;
        }

        ActivateMechanics();
    }

    void ActivateMechanics()
    {
        isActive = true;
        StartCoroutine(ExecuteAttackCycle());
    }

    void Update()
	{
		// Блокируем движение босса, если активна механика слабого места
		if (isActive && !isWeakSpotActive && !isMechanicsPaused)
		{
			// Двигаем босса
			transform.Translate(movementDirection * movementSpeed * Time.deltaTime);

			// Получаем границы видимой зоны камеры
			Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
			Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

			// Учитываем размеры босса
			float halfWidth = GetComponent<SpriteRenderer>().bounds.extents.x; // Половина ширины
			float halfHeight = GetComponent<SpriteRenderer>().bounds.extents.y; // Половина высоты

			// Проверяем границы по X
			if (transform.position.x - halfWidth < min.x || transform.position.x + halfWidth > max.x)
			{
				movementDirection.x = -movementDirection.x; // Меняем направление
				transform.position = new Vector3(
					Mathf.Clamp(transform.position.x, min.x + halfWidth, max.x - halfWidth),
					transform.position.y,
					transform.position.z
				);
			}

			// Проверяем границы по Y
			if (transform.position.y - halfHeight < min.y || transform.position.y + halfHeight > max.y)
			{
				movementDirection.y = -movementDirection.y; // Меняем направление
				transform.position = new Vector3(
					transform.position.x,
					Mathf.Clamp(transform.position.y, min.y + halfHeight, max.y - halfHeight),
					transform.position.z
				);
			}
		}
	}

	IEnumerator ExecuteAttackCycle()
	{
		while (isActive)  // Основной цикл, пока босс активен
		{
			// Создаем список механик, кроме слабого места, и перемешиваем порядок выполнения
			List<IEnumerator> mechanics = new List<IEnumerator> { Attack1(), Attack2(), Attack3(), Attack4() };
			ShuffleList(mechanics);

			// Выполняем перемешанные механики по очереди
			foreach (var mechanic in mechanics)
			{
				yield return new WaitForSeconds(attackCooldown);
				if (isMechanicsPaused) yield break;  // Проверяем, что механики не приостановлены
				yield return StartCoroutine(mechanic);
			}

			// После выполнения всех механик активируем слабое место и ждем его завершения
			yield return new WaitForSeconds(attackCooldown);
			yield return ActivateWeakSpotMechanic();

			// После завершения слабого места ждем cooldown и начинаем цикл заново
			yield return new WaitForSeconds(attackCooldown);
		}
	}

	// Метод для перемешивания списка механик
	void ShuffleList<T>(List<T> list)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int randomIndex = Random.Range(0, i + 1);
			T temp = list[i];
			list[i] = list[randomIndex];
			list[randomIndex] = temp;
		}
	}

    IEnumerator Attack1()
    {
        GameObject ring = Instantiate(ringPrefab, transform.position, Quaternion.identity);
        ring.transform.SetParent(transform);

        Vector3 initialScale = ring.transform.localScale;
        Vector3 targetScale = initialScale * ringMaxScale;

        float elapsedTime = 0f;

        while (elapsedTime < ringExpandDuration)
        {
            ring.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / ringExpandDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < ringExpandDuration)
        {
            ring.transform.localScale = Vector3.Lerp(targetScale, initialScale, elapsedTime / ringExpandDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(ring);
    }

    IEnumerator Attack2()
	{
		Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
		yield return new WaitForSeconds(2f);

		// Определяем границы видимой области камеры
		Vector3 min = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
		Vector3 max = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

		float halfWidth = GetComponent<SpriteRenderer>().bounds.extents.x; // Половина ширины
		float halfHeight = GetComponent<SpriteRenderer>().bounds.extents.y; // Половина высоты

		while (Vector3.Distance(transform.position, playerPosition) > 0.5f)
		{
			// Двигаем босса к игроку
			transform.position = Vector3.MoveTowards(transform.position, playerPosition, movementSpeed * 10 * Time.deltaTime);

			// Проверяем границы карты и корректируем позицию
			if (transform.position.x - halfWidth < min.x || transform.position.x + halfWidth > max.x)
			{
				// Меняем направление по X
				playerPosition.x = Mathf.Clamp(playerPosition.x, min.x + halfWidth, max.x - halfWidth);
			}

			if (transform.position.y - halfHeight < min.y || transform.position.y + halfHeight > max.y)
			{
				// Меняем направление по Y
				playerPosition.y = Mathf.Clamp(playerPosition.y, min.y + halfHeight, max.y - halfHeight);
			}

			yield return null; // Ждем следующего кадра
		}

		// Пауза перед следующим действием
		yield return new WaitForSeconds(1f);
	}

	IEnumerator Attack3()
	{
		int shots = Random.Range(2, 5); // Случайное количество серий выстрелов от 2 до 4

		for (int j = 0; j < shots; j++)  // Выполнение случайного количества серий выстрелов
		{
			// Воспроизводим звук выстрела
			PlayShootSound();
			
			int projectilesPerShot = 12; // Количество снарядов в одной серии
			for (int i = 0; i < projectilesPerShot; i++)
			{
				// Центр выстрела - текущая позиция босса
				Vector3 startPosition = transform.position;

				// Вычисляем угол для каждого снаряда с небольшим случайным отклонением
				float angle = i * Mathf.PI * 2 / projectilesPerShot + Random.Range(-0.1f, 0.1f);
				Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

				// Создаем снаряд и задаем его направление с небольшой рандомизацией
				GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
				projectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
			}
			yield return new WaitForSeconds(0.5f); // Задержка между сериями выстрелов
		}
	}

	void PlayShootSound()
	{
		if (shootSound != null)
		{
			shootSound.Play();
		}
	}



    IEnumerator Attack4()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-8.3f, 8.3f), mainCamera.ViewportToWorldPoint(new Vector3(0, 1)).y + 1, 0);
            Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }
    }

	IEnumerator ActivateWeakSpotMechanic()
	{
		isMechanicsPaused = true;  // Приостановка всех других механик
		Vector3 targetPosition = new Vector3(0, 0, transform.position.z);

		// Плавное движение к центру
		while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, approachSpeed * Time.deltaTime);
			yield return null;
		}

		// Появление объектов для сбора
		collectedItems = 0;
		float exclusionRadius = 4f;
		float collectionTimeLimit = 5f;
		GameObject[] collectibles = new GameObject[collectibleCount];

		for (int i = 0; i < collectibleCount; i++)
		{
			Vector3 spawnPosition;
			do
			{
				spawnPosition = new Vector3(
					Random.Range(-7f, 7f),
					Random.Range(-4f, 4f),
					0
				);
			} while (Vector3.Distance(spawnPosition, targetPosition) < exclusionRadius);

			GameObject collectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);
			collectibles[i] = collectible;

			collectible.AddComponent<Collectible>().OnCollected += () =>
			{
				collectedItems++;
				
				// Воспроизведение звука
				if (audioSource != null && collectSound != null)
				{
					audioSource.PlayOneShot(collectSound);
				}
				
				Destroy(collectible);
			};

			yield return new WaitForSeconds(1f);
		}

		// Даем игроку 5 секунд на сбор всех объектов
		float timer = collectionTimeLimit;
		while (collectedItems < collectibleCount && timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}

		if (collectedItems < collectibleCount)
		{
			foreach (var collectible in collectibles)
			{
				if (collectible != null)
				{
					Destroy(collectible);
				}
			}
		}
		else
		{
			GameObject weakSpot = Instantiate(weakSpotPrefab, transform.position + Vector3.up * -2.5f, Quaternion.identity);
			WeakSpotLevel2 weakSpotScript = weakSpot.GetComponent<WeakSpotLevel2>();
			if (weakSpotScript != null)
			{
				isWeakSpotActive = true;
				weakSpotScript.ActivateForDuration(weakSpotDuration, this);
			}
		}

		yield return new WaitForSeconds(weakSpotDuration);
		isWeakSpotActive = false;
		isMechanicsPaused = false;  // Снятие паузы после завершения слабого места
	}

    public void TakeDamage()
    {
        if (!isActive) return;

        health--;
        Debug.Log("Босс получил урон! Осталось здоровья: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Босс побеждён!");
        Destroy(gameObject);
    }
}
