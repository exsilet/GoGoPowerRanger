using System.Collections;
using UnityEngine;

public class BossMechanics : MonoBehaviour
{
    public GameObject projectilePrefab;         // Префаб снаряда
    public GameObject laserPrefab;              // Префаб лазера
    public GameObject minionPrefab;             // Префаб миньона
    public GameObject weakSpotPrefab;           // Префаб слабого места

    public float shootInterval = 3f;            // Интервал между сериями выстрелов
    public float laserDuration = 2f;            // Длительность лазера
    public float minionSpawnInterval = 5f;      // Интервал между призывами миньонов
    public float weakSpotInterval = 10f;        // Интервал между появлениями слабого места
    public float weakSpotDuration = 3f;         // Время появления слабого места
    public float approachSpeed = 2f;            // Скорость движения к стоп-позиции
    public int health = 5;                      // Общее количество жизней босса

    public float stopPositionX = 7f;            // Позиция X, где босс остановится и начнет атаки

    private bool isActive = false;              // Флаг, который указывает, активны ли механики босса
    private bool isWeakSpotActive = false;      // Флаг активности слабого места
    private bool isDodging = false;             // Флаг уклонения

    private int activeMinions = 0;              // Счётчик активных миньонов
    private int maxMinions = 5;                 // Максимальное количество миньонов на экране

    void Start()
    {
        // Начинаем движение босса к стоп-позиции
        StartCoroutine(ApproachStopPosition());
    }

    // Корутина для движения к стоп-позиции
    IEnumerator ApproachStopPosition()
    {
        // Пока координата X босса больше, чем stopPositionX
        while (transform.position.x > stopPositionX)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(stopPositionX, transform.position.y, transform.position.z), approachSpeed * Time.deltaTime);
            yield return null;
        }

        // Когда достиг стоп-позиции, активируем механики
        ActivateMechanics();
    }

    // Активирует механики босса
    void ActivateMechanics()
    {
        isActive = true; // Устанавливаем флаг активности
        StartCoroutine(ShootProjectiles());
        StartCoroutine(LaserAttack());
        StartCoroutine(SpawnMinions());
        StartCoroutine(ActivateWeakSpot());
    }

    // Метод для уменьшения здоровья при ударе по слабому месту
    public void TakeDamage()
    {
        if (!isActive) return; // Игнорируем урон, если босс еще не активен

        health--;
        Debug.Log("Босс получил урон! Осталось здоровья: " + health);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DodgeAndReturn()); // Уклонение после попадания
        }
    }

    // Метод уничтожения босса
    private void Die()
	{
		Debug.Log("Босс побеждён!");
		
		// Уничтожаем миньонов
		MinionBehavior[] minions = FindObjectsOfType<MinionBehavior>();
		foreach (var minion in minions)
		{
			// Уничтожаем каждого миньона
			Destroy(minion.gameObject);
		}

		// Уничтожаем объект босса
		Destroy(gameObject); // Уничтожаем объект босса
	}

    // Корутина для стрельбы снарядами
    IEnumerator ShootProjectiles()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);

            // Стреляем 3 снарядами с промежутком 0.5 секунд
            for (int i = 0; i < 3; i++)
            {
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                Vector2 direction = (GameObject.FindGameObjectWithTag("Player").transform.position - transform.position).normalized;
                projectile.GetComponent<Rigidbody2D>().velocity = direction * 5f;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // Корутина для лазерной атаки
    IEnumerator LaserAttack()
    {
        while (true)
        {
            yield return new WaitForSeconds(7f);

            // Создаём лазер как дочерний объект босса и центрируем его перед ним
            GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            laser.transform.SetParent(transform);
            laser.transform.localPosition = new Vector3(-30f, 0, 0);

            yield return new WaitForSeconds(laserDuration);
            Destroy(laser);
        }
    }

    // Корутина для спавна миньонов
    IEnumerator SpawnMinions()
    {
        while (true)
        {
            yield return new WaitForSeconds(minionSpawnInterval);

            // Проверяем, не превышает ли количество миньонов максимальное значение
            if (activeMinions < maxMinions)
            {
                GameObject minion = Instantiate(minionPrefab, transform.position, Quaternion.identity);
                MinionBehavior minionBehavior = minion.GetComponent<MinionBehavior>();
                if (minionBehavior != null)
                {
                    minionBehavior.InitializeBouncing();
                }
                activeMinions++;

                minion.GetComponent<MinionBehavior>().OnMinionDestroyed += () => activeMinions--;
            }
        }
    }

    // Корутина для активации слабого места
    IEnumerator ActivateWeakSpot()
    {
        while (true)
        {
            yield return new WaitForSeconds(weakSpotInterval);
            if (!isWeakSpotActive)
            {
                isWeakSpotActive = true;

                // Создаём слабое место и привязываем его к боссу
                GameObject weakSpot = Instantiate(weakSpotPrefab, transform.position, Quaternion.identity);
                weakSpot.transform.SetParent(transform);
                weakSpot.transform.localPosition = new Vector3(0, -4f, 0);

                weakSpot.GetComponent<WeakSpot>().ActivateForDuration(weakSpotDuration, this);
                yield return new WaitForSeconds(weakSpotDuration);
                
                Destroy(weakSpot);
                isWeakSpotActive = false;
            }
        }
    }

    // Корутина для уклонения и возвращения на место
    IEnumerator DodgeAndReturn()
    {
        if (!isDodging)
        {
            isDodging = true;
            Vector3 randomPosition = new Vector3(Random.Range(-8f, 8f), Random.Range(-4f, 4f), transform.position.z);

            float dodgeDuration = 1f;
            float elapsedTime = 0f;
            Vector3 startPosition = transform.position;

            while (elapsedTime < dodgeDuration)
            {
                transform.position = Vector3.Lerp(startPosition, randomPosition, elapsedTime / dodgeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = startPosition;
            isDodging = false;
        }
    }
}
