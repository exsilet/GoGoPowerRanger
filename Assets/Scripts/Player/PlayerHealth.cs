using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    // Настраиваемое начальное количество здоровья
    public int maxHealth = 3;

    // Текущее количество здоровья
    public int currentHealth;

    // Ссылка на панель Game Over
    public GameObject gameOverPanel;
	
	// Максимальное здоровье, которое может быть (чтобы не выходить за пределы)
    public int maxPlayerHealth = 5;
	
	// Параметры для шейка камеры
    public bool useShakeEffect = true;  // Включить ли шейк при получении урона
    public float shakeDuration = 0.2f;  // Длительность шейка
    public float shakeStrength = 0.2f;  // Сила шейка
    public float shakeDelay = 0.0f;    // Задержка перед шейком

    private Vector3 originalCameraPosition;  // Исходная позиция камеры
    private bool isShaking = false;  // Флаг, указывающий, идет ли шейк

    private Camera mainCamera;  // Камера
	
	public HeartManager heartManager;  // Ссылка на HeartManager
	
	public ParticleSystem deathEffect; // Ссылка на эффект смерти (система частиц)
	
	public AudioSource audioSource;  // Ссылка на компонент AudioSource
    public AudioClip damageSound;    // Звук получения урона

    void Start()
    {
        // Устанавливаем начальное здоровье при запуске игры
        ResetHealth();  // Используем новый метод для установки здоровья
		currentHealth = maxHealth;  // Изначально здоровье равно максимальному
		
		mainCamera = Camera.main;  // Получаем ссылку на основную камеру
        originalCameraPosition = mainCamera.transform.position;  // Сохраняем начальную позицию камеры
		
		
        if (heartManager != null)
        {
            heartManager.UpdateHeartDisplay();  // Обновляем отображение сердечек в начале игры
        }
    }

    // Метод получения урона
    public void TakeDamage(int damage)
    {
		
		if (GetComponent<PlayerController>().isInvincible) // Если игрок неуязвим, игнорируем урон
		{
			Debug.Log("Урон заблокирован щитом!");
			return;
		}
		
        currentHealth -= damage;
        Debug.Log("Получен урон! Текущее здоровье: " + currentHealth);
		
		// Запуск shake камеры
        if (useShakeEffect && !isShaking)
        {
            StartCoroutine(ShakeCamera());  // Запуск шейка камеры
        }
		
		// Воспроизводим звук получения урона
        PlayDamageSound();

        // Проверяем, не закончилось ли здоровье
        if (currentHealth <= 0)
        {
            Die();
        }
		
		// Уведомляем HeartManager, чтобы обновить сердечки
        if (heartManager != null)
        {
            heartManager.UpdateHeartDisplay();  // Обновляем отображение
        }
    }
	
	void PlayDamageSound()
    {
        if (damageSound != null)
        {
            audioSource.PlayOneShot(damageSound); // Воспроизводим звук
            Debug.Log("Damage sound played");
        }
        else
        {
            Debug.LogWarning("Damage sound not assigned");
        }
    }

    // Метод для обработки смерти игрока
    void Die()
    {
	    Debug.Log("Игрок умер!");

	    PlayerController playerController = GetComponent<PlayerController>();
	    PlayerAbilities playerAbilities = GetComponent<PlayerAbilities>();

	    if (playerController != null)
		    playerController.StopBlinkEffect();

	    if (playerAbilities != null)
		    playerAbilities.ResetAbilities();

	    Time.timeScale = 0f;
		
		// Поставить музыку на паузу
		GameObject levelObject = GameObject.FindGameObjectWithTag("LVL_1");
		if (levelObject != null)
		{
			AudioSource audio = levelObject.GetComponent<AudioSource>();
			if (audio != null)
			{
				audio.Pause();
				Debug.Log("Музыка поставлена на паузу для объекта с тегом LVL_1");
			}
		}

        // Запуск эффекта смерти перед панелью Game Over
        if (deathEffect != null)
        {
            // Получаем текущее положение игрока из PlayerController
            Vector3 playerPosition = transform.position;

            // Запускаем эффект смерти на позиции игрока
            Instantiate(deathEffect, playerPosition, Quaternion.identity);
        }

        // Показываем панель Game Over только после завершения эффекта
        if (gameOverPanel != null)
        {
            StartCoroutine(ShowGameOverPanelAfterEffect());
        }
    }
	
	
    // Корутина для того, чтобы подождать завершения эффекта и показать панель Game Over
    private IEnumerator ShowGameOverPanelAfterEffect()
    {
        // Ждем, пока частицы завершат свою анимацию
        if (deathEffect != null)
        {
            float effectDuration = deathEffect.main.duration;
            yield return new WaitForSecondsRealtime(effectDuration); // Используем RealTime для игнорирования паузы
        }

        // Показ панель Game Over после завершения эффекта
        gameOverPanel.SetActive(true);
    }
	
    // Метод для восстановления здоровья при рестарте
    public void ResetHealth()
    {
        currentHealth = maxHealth;  // Сбрасываем здоровье на максимальное
        Debug.Log("Здоровье восстановлено. Текущее здоровье: " + currentHealth);
    }

    // Метод для восстановления игры после паузы
    public void RestartGame()
    {
		
		currentHealth = maxHealth;  // Восстанавливаем здоровье
		
		// Сброс мигания и неуязвимости
		PlayerController playerController = GetComponent<PlayerController>();
		if (playerController != null)
		{
			playerController.StopBlinkEffect();
		}
		
        // Включаем время обратно
        Time.timeScale = 1f;

        // Восстанавливаем здоровье
        ResetHealth();

    }
	
	public void IncreaseHealth(int healthAmount)
	{
		currentHealth += healthAmount;

		// Ограничиваем здоровье максимальным значением
		if (currentHealth > maxHealth)
		{
			currentHealth = maxHealth;
		}

		Debug.Log("Здоровье увеличено! Текущее здоровье: " + currentHealth);
	}
	
	    
	// Корутину для камеры shake
    private IEnumerator ShakeCamera()
	{
		isShaking = true;  // Включаем флаг шейка
		float shakeTime = 0f;  // Время, которое прошло с начала шейка

		// Процесс шейка
		while (shakeTime < shakeDuration)
		{
			// Используем unscaledDeltaTime для правильной работы времени при паузе
			shakeTime += Time.unscaledDeltaTime;

			// Если игра на паузе, завершить шейк
			if (Time.timeScale == 0)
			{
				yield return null;  // Прерываем выполнение на кадр, но продолжаем в будущем
				continue;
			}

			// Генерация случайных смещений для шейка камеры
			float xShake = Random.Range(-shakeStrength, shakeStrength);
			float yShake = Random.Range(-shakeStrength, shakeStrength);
			Vector3 shakeOffset = new Vector3(xShake, yShake, 0);

			// Смещаем камеру
			mainCamera.transform.position = originalCameraPosition + shakeOffset;

			yield return null;  // Ждем один кадр
		}

		// Восстанавливаем камеру на исходную позицию
		mainCamera.transform.position = originalCameraPosition;

		isShaking = false;  // Выключаем флаг шейка
	}
	
	// Публичные методы для доступа к текущему здоровью и максимальному здоровью
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}