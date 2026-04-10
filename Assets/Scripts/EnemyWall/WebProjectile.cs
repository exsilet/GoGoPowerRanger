using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    public float speed = 5f;              // Постоянная скорость полёта паутины
    public float minShootDelay = 0.5f;    // Минимальная задержка перед выстрелом
    public float maxShootDelay = 1f;      // Максимальная задержка перед выстрелом
    public float destroyDelay = 5f;       // Время, через которое паутина исчезнет, если не попадёт в игрока
    public AudioSource webFlyingSound;   // Звук полёта паутины

    private Transform player;             // Ссылка на игрока
    private PlayerController playerController; // Ссылка на скрипт управления игроком
    private Rigidbody2D rb;               // Rigidbody2D паутины
    private Camera mainCamera;            // Главная камера
    private bool hasEnteredView = false;  // Флаг, который определяет, появилась ли паутина на экране
    private static bool playerCaught = false; // Статический флаг, который определяет, что игрок уже пойман
    private bool isCurrentWebActive = false;  // Флаг, что эта паутина активна на игроке
	public AudioClip keyPressSound; // Звук для нажатия клавиши
	private AudioSource audioSource; // Аудиоисточник для воспроизведения звука
	public AudioClip releaseSound;       // Звук освобождения от паутины

    void Start()
    {
        // Находим игрока и камеру
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();  // Получаем скрипт управления игроком
        mainCamera = Camera.main;

        rb = GetComponent<Rigidbody2D>();
		
		    // Инициализируем аудиоисточник
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.clip = keyPressSound;

        // Пока паутина не видима на экране, она не двигается
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        // Если игрок уже пойман другой паутиной, эта паутина пролетает мимо
        if (playerCaught && !isCurrentWebActive)
        {
            return;
        }

        // Паутина движется, если она в поле зрения
        if (IsInView() && !hasEnteredView)
        {
            hasEnteredView = true;  // Паутина попала на экран
            StartCoroutine(ShootAfterDelay());  // Запускаем корутину с задержкой перед выстрелом
        }

        // Проверяем, можно ли уничтожить паутину, если она покинула экран
        if (hasEnteredView && !IsInView())
        {
            Destroy(gameObject);  // Уничтожаем паутину
        }
    }

    // Корутина для выстрела с задержкой
    IEnumerator ShootAfterDelay()
    {
        // Случайная задержка перед выстрелом
        float delay = Random.Range(minShootDelay, maxShootDelay);
        Debug.Log($"Задержка перед выстрелом: {delay} секунд.");
        yield return new WaitForSeconds(delay);

        // Как только паутина стала видимой и задержка закончилась, она начинает двигаться в сторону игрока
        if (player != null && !playerCaught)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * speed;  // Задаём постоянную скорость в направлении игрока

            // Проигрываем звук полёта паутины
            if (webFlyingSound != null)
            {
                webFlyingSound.Play();
                Debug.Log("Звук полёта паутины воспроизведён.");
            }
        }
    }

    // Обработка столкновения с игроком
    void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player") && !playerCaught)
		{
			Debug.Log("Игрок пойман в паутину!");
			playerCaught = true;
			isCurrentWebActive = true;
			rb.velocity = Vector2.zero;

			transform.SetParent(player);

			if (playerController != null)
			{
				playerController.enabled = false;
				playerController.EnableKeys(); // Включаем отображение букв A и D
				StartCoroutine(HandleKeyPressSequence());
			}
		}
	}
	
	IEnumerator HandleKeyPressSequence()
	{
		KeyCode currentExpectedKey = KeyCode.A; // Начинаем с ожидания A

		for (int i = 0; i < 3; i++) // 3 цикла переключения между A и D
		{
			// Активируем и подсвечиваем букву A
			currentExpectedKey = KeyCode.A; // Устанавливаем текущую ожидаемую клавишу
			playerController.HighlightKey(playerController.keyA, Color.red);
			playerController.ResetKeyColor(playerController.keyD); // Сбрасываем цвет D, если он остался красным

			// Ждём нажатия A
			yield return new WaitUntil(() => Input.GetKeyDown(currentExpectedKey));
			Debug.Log("Нажата клавиша A");
			
			// Проигрываем звук нажатия
			if (keyPressSound != null && audioSource != null)
			{
				audioSource.Play();
			}
			
			playerController.ResetKeyColor(playerController.keyA);         // Сбрасываем цвет A
			playerController.PlayKeyParticle(playerController.keyAParticle); // Проигрываем партикл A

			// Активируем и подсвечиваем букву D
			currentExpectedKey = KeyCode.D; // Устанавливаем текущую ожидаемую клавишу
			playerController.HighlightKey(playerController.keyD, Color.red);
			playerController.ResetKeyColor(playerController.keyA); // Сбрасываем цвет A, если он остался красным

			// Ждём нажатия D
			yield return new WaitUntil(() => Input.GetKeyDown(currentExpectedKey));
			Debug.Log("Нажата клавиша D");
			
			// Проигрываем звук нажатия
			if (keyPressSound != null && audioSource != null)
			{
				audioSource.Play();
			}
			
			playerController.ResetKeyColor(playerController.keyD);         // Сбрасываем цвет D
			playerController.PlayKeyParticle(playerController.keyDParticle); // Проигрываем партикл D
		}

		Debug.Log("Игрок освободился из паутины!");
		playerCaught = false;
		isCurrentWebActive = false;

		if (playerController != null)
		{
			playerController.enabled = true;
			playerController.DisableKeys(); // Выключаем отображение букв A и D
		}

		StartCoroutine(HandleWebRelease());
	}
	
	IEnumerator HandleWebRelease()
    {
        HideWeb(); // Скрываем паутину

        if (audioSource != null && keyPressSound != null)
        {
            yield return new WaitWhile(() => audioSource.isPlaying); // Ждём завершения звука последнего нажатия
        }

        if (releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound); // Воспроизводим звук освобождения
            yield return new WaitWhile(() => audioSource.isPlaying); // Ждём завершения звука освобождения
        }

        Destroy(gameObject); // Удаляем паутину
    }

    void HideWeb()
    {
        // Скрываем объект и его дочерние элементы
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    // Проверка на нахождение паутины на экране
    bool IsInView()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
}
