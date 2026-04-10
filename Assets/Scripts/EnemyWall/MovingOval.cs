using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingOval : MonoBehaviour
{
    public float speed = 2f;                    // Скорость движения овала
    public float detectionRadius = 4f;         // Радиус, в котором проверяется приближение
    public GameObject trianglePrefab;          // Префаб треугольника
    public float triangleSpeed = 10f;          // Скорость треугольника
    public float triangleLifetime = 1f;        // Время, через которое треугольник удалится
    public GameObject radiusIndicatorPrefab;   // Префаб полупрозрачного круга
    public AudioSource ovalSound;              // Звук при пересечении X = 14
    public AudioSource launchTriangleSound;    // Звук при запуске треугольника

    private Transform player;                  // Ссылка на игрока
    private bool triangleCreated = false;      // Флаг, который контролирует создание треугольника
    private bool hasPlayedSound = false;       // Флаг, чтобы звук играл только один раз
    private GameObject radiusIndicator;        // Ссылка на круговой индикатор

    void Start()
    {
        // Находим игрока по тегу
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Создаём индикатор радиуса как дочерний объект овала
        radiusIndicator = Instantiate(radiusIndicatorPrefab, transform.position, Quaternion.identity, transform);

        // Масштабируем индикатор радиуса в соответствии с detectionRadius
        float scaleFactor = detectionRadius * 2f;  // Диаметр круга (умножаем на 2)
        radiusIndicator.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    void Update()
    {
		OnDrawGizmos();
        // Овал движется по оси X
        transform.position += Vector3.right * speed * Time.deltaTime;

        // Проверяем, если объект пересек X = 14
        if (!hasPlayedSound && transform.position.x <= 14f)
        {
            hasPlayedSound = true;

            // Воспроизводим звук
            if (ovalSound != null)
            {
                ovalSound.Play();
            }
        }

        // Проверяем расстояние между игроком и овалом
        if (!triangleCreated && Vector3.Distance(player.position, transform.position) < detectionRadius)
        {
            // Запускаем треугольник из случайного угла
            LaunchTriangleFromRandomAngle();
            triangleCreated = true;  // Устанавливаем флаг, чтобы треугольник создавался только один раз
        }
    }

    void LaunchTriangleFromRandomAngle()
    {
        // Генерируем случайный угол (в радианах)
        float randomAngle = Random.Range(0f, Mathf.PI * 2);

        // Вычисляем позицию объекта на основе случайного угла
        Vector3 triangleStartPosition = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0) * 10f;

        // Создаём объект
        GameObject triangle = Instantiate(trianglePrefab, triangleStartPosition, Quaternion.identity);

        // Вычисляем направление движения объекта к игроку
        Vector3 direction = (player.position - triangleStartPosition).normalized;

        // Устанавливаем начальный флип по оси X в зависимости от направления
        SpriteRenderer spriteRenderer = triangle.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0; // Флип только по оси X, если направление отрицательное
        }

        // Добавляем движение объекту
        Rigidbody2D rb = triangle.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * triangleSpeed;
        }

        // Воспроизводим звук запуска треугольника
        if (launchTriangleSound != null)
        {
            launchTriangleSound.Play();
        }

        // Удаляем объект через заданное время
        Destroy(triangle, triangleLifetime);
    }
	
	void OnDrawGizmos()
	{
		// Устанавливаем цвет для Gizmos
		Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f); // Полупрозрачный синий цвет

		// Рисуем окружность в позиции объекта с радиусом detectionRadius
		Gizmos.DrawWireSphere(transform.position, detectionRadius);
	}
}
