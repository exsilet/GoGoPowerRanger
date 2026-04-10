using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic5_1 : MonoBehaviour, IBossMechanic
{
    public GameObject turretPrefab;               // Префаб турели
    public GameObject projectilePrefab;           // Префаб снаряда
    public float shootingInterval = 2f;           // Интервал между выстрелами
    public float projectileSpeed = 5f;            // Скорость снаряда
    public int shotsPerTurret = 5;                // Количество выстрелов каждой турели
    public float bossMoveDuration = 2f;           // Длительность перемещения босса в центр
    public float pauseDuration = 0.5f;            // Длительность паузы между миганиями

    private List<Transform> turrets = new List<Transform>(); // Список турелей
    private List<GameObject> activeProjectiles = new List<GameObject>(); // Список активных снарядов
    private Transform boss;                       // Ссылка на босса
    private SpriteRenderer bossSpriteRenderer;    // Компонент SpriteRenderer босса
    private Transform player;                     // Ссылка на игрока
	public AudioSource shootingSound; // Звук стрельбы

    public IEnumerator Execute()
    {
        // Получаем ссылку на босса и его SpriteRenderer
        boss = transform;
        bossSpriteRenderer = boss.GetComponent<SpriteRenderer>();

        // Находим игрока по тегу "Player"
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Перемещаем босса в центр, мигаем и исчезаем
        yield return StartCoroutine(MoveBossToPosition(Vector2.zero));
        yield return StartCoroutine(BlinkBoss(2));
        SetVisibility(false);

        // Создаём турели
        CreateTurrets();

        // Начинаем стрельбу
        yield return StartCoroutine(ShootingRoutine());

        // Ждём перед удалением турелей и снарядов
        yield return new WaitForSeconds(3f);

        // Уничтожаем турели и снаряды
        DestroyTurrets();
        DestroyProjectiles();

        // Босс снова появляется и мигает дважды
        SetVisibility(true);
        yield return StartCoroutine(BlinkBoss(2));

        // Завершение механики
    }

    private IEnumerator MoveBossToPosition(Vector2 targetPosition)
    {
        Vector2 startPosition = boss.position;
        float elapsedTime = 0f;

        while (elapsedTime < bossMoveDuration)
        {
            boss.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / bossMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        boss.position = targetPosition;
    }

    private IEnumerator BlinkBoss(int times)
    {
        for (int i = 0; i < times; i++)
        {
            bossSpriteRenderer.enabled = false;
            yield return new WaitForSeconds(pauseDuration / 2);
            bossSpriteRenderer.enabled = true;
            yield return new WaitForSeconds(pauseDuration / 2);
        }
    }

    private void CreateTurrets()
    {
        Camera cam = Camera.main;
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector2(1, 1));
        Vector2 topLeft = cam.ViewportToWorldPoint(new Vector2(0, 1));
        Vector2 bottomRight = cam.ViewportToWorldPoint(new Vector2(1, 0));
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));

        // Располагаем турели по углам экрана
        turrets.Add(Instantiate(turretPrefab, topRight, Quaternion.identity).transform);
        turrets.Add(Instantiate(turretPrefab, topLeft, Quaternion.identity).transform);
        turrets.Add(Instantiate(turretPrefab, bottomRight, Quaternion.identity).transform);
        turrets.Add(Instantiate(turretPrefab, bottomLeft, Quaternion.identity).transform);
    }

    private IEnumerator ShootingRoutine()
    {
        int shotsFired = 0;

        while (shotsFired < shotsPerTurret)
        {
            foreach (Transform turret in turrets)
            {
                if (turret != null)
                {
                    ShootAtPlayer(turret);
                }
            }
            shotsFired++;
            yield return new WaitForSeconds(shootingInterval);
        }

        // Ждём, чтобы убедиться, что все снаряды созданы
        yield return new WaitForSeconds(0.1f);
    }

	private void ShootAtPlayer(Transform turret)
	{
		if (player != null)
		{
			// Вычисляем направление к игроку
			Vector2 direction = (player.position - turret.position).normalized;
			GameObject projectile = Instantiate(projectilePrefab, turret.position, Quaternion.identity);
			Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
			rb.velocity = direction * projectileSpeed;

			// Добавляем снаряд в список для последующего уничтожения
			activeProjectiles.Add(projectile);

			// Если турель совпадает с первой турелью, проигрываем звук
			if (turret == turrets[0] && shootingSound != null)
			{
				shootingSound.Play();
			}
		}
	}

    private void DestroyTurrets()
    {
        foreach (Transform turret in turrets)
        {
            if (turret != null)
            {
                Destroy(turret.gameObject);
            }
        }
        turrets.Clear();
    }

    private void DestroyProjectiles()
    {
        foreach (GameObject projectile in activeProjectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }
        activeProjectiles.Clear();
    }
	
	private void SetVisibility(bool isVisible)
	{
		// Управляем видимостью босса
		if (bossSpriteRenderer != null)
		{
			bossSpriteRenderer.enabled = isVisible;
		}

		// Управляем видимостью всех дочерних объектов
		foreach (Transform child in boss)
		{
			SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
			if (childRenderer != null)
			{
				childRenderer.enabled = isVisible;
			}
		}
	}
}