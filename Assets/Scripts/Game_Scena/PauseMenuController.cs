using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    // Ссылка на панель паузы
    public GameObject pauseMenuPanel;
    public Button pauseButton; // Кнопка паузы
    public Button resumeButton; // Кнопка для продолжения игры
    public Button mainMenuButton; // Кнопка для возвращения в главное меню
    public Button settingsButton; // Кнопка для открытия настроек
	public Button restartLevelButton; // Кнопка для перезапуска уровня

    // Ссылки на объекты из LevelManager
    public GameObject gamePanel; // Игровая панель
    public GameObject mainMenu; // Главное меню
    public GameObject gameOverPanel; // Панель Game Over
	public GameObject finishPanel;  // Ссылка на панель Finish

    // Ссылки на объекты из Game_LVL
    public GameObject[] levelObjects; // Объекты, представляющие уровни

    // Переменная для хранения клона текущего уровня
    private GameObject currentLevelClone;
	private int currentLevelIndex;  // Индекс текущего уровня
	
	private LevelManager levelManager;

    private bool isGamePaused = false; // Флаг для отслеживания состояния игры
	
	//Анимация перехода на уровень
	public GameObject blackPanel;  // Панель для затемнения экрана
    public TextMeshProUGUI levelNameText;  // TMP текст для отображения названия уровня
	
	//Отображени ХП на экране
	public HeartManager heartManager;  // Ссылка на HeartManager
	[SerializeField] private Start_Design startDesignObject; 

    void Start()
    {
		// Находим LevelManager на сцене
        levelManager = FindObjectOfType<LevelManager>();
		
        // Подключаем кнопки к методам
        pauseButton.onClick.AddListener(TogglePause);  // При нажатии на кнопку паузы запускаем метод TogglePause
        resumeButton.onClick.AddListener(ResumeGame);  // При нажатии на кнопку продолжения возобновляем игру
        mainMenuButton.onClick.AddListener(GoToMainMenu);  // Возврат в главное меню
        settingsButton.onClick.AddListener(OpenSettings);  // Открытие настроек
		restartLevelButton.onClick.AddListener(RestartLevel); // Подключаем кнопку перезапуска

        // Панель паузы изначально скрыта
        pauseMenuPanel.SetActive(false);
    }

    // Метод для переключения паузы
    void TogglePause()
	{
		if (isGamePaused)
		{
			return;  // Игнорируем нажатие, если игра на паузе
		}
		else
		{
			PauseGame();  // Если игра не на паузе, ставим на паузу
		}
	}

    // Останавливаем игру и показываем панель паузы
    void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;  // Останавливаем игру

        // Показываем панель паузы
        pauseMenuPanel.SetActive(true);
		
		PlayerController playerController = FindObjectOfType<PlayerController>();
		if (playerController != null)
		{
			playerController.DisableControlForDuration(float.MaxValue, false); // Отключаем управление игрока
		}
		
		Rigidbody2D playerRigidbody = FindObjectOfType<PlayerController>()?.GetComponent<Rigidbody2D>();
		if (playerRigidbody != null)
		{
			playerRigidbody.velocity = Vector2.zero; // Останавливаем движение
			playerRigidbody.isKinematic = true;     // Отключаем физику
		}
		
		Animator playerAnimator = FindObjectOfType<PlayerController>()?.GetComponent<Animator>();
		if (playerAnimator != null)
		{
			playerAnimator.speed = 0; // Останавливаем анимацию
		}
    }

    // Возобновляем игру и скрываем панель паузы
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;  // Возвращаем нормальную скорость времени

        // Скрываем панель паузы
        pauseMenuPanel.SetActive(false);
		
		PlayerController playerController = FindObjectOfType<PlayerController>();
		if (playerController != null)
		{
			playerController.DisableControlForDuration(0, false); // Сбрасываем блокировку управления
		}
		
		Rigidbody2D playerRigidbody = FindObjectOfType<PlayerController>()?.GetComponent<Rigidbody2D>();
		if (playerRigidbody != null)
		{
			playerRigidbody.isKinematic = false; // Возвращаем физику
		}
		
		Animator playerAnimator = FindObjectOfType<PlayerController>()?.GetComponent<Animator>();
		if (playerAnimator != null)
		{
			playerAnimator.speed = 1; // Включаем анимацию
		}
    }

	public void GoToMainMenu()
	{
		// Удаляем клон текущего уровня через LevelManager
		LevelManager levelManager = FindObjectOfType<LevelManager>(); // Находим LevelManager на сцене
		if (levelManager != null)
		{
			levelManager.RemoveCurrentLevel(); // Удаляем текущий уровень
			levelManager.ResetObjectsToInitialPositions(); // Восстановим начальные позиции
		}
	
		// Восстанавливаем здоровье игрока
		PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
		if (playerHealth != null)
		{
			playerHealth.RestartGame();  // Восстанавливаем здоровье
		}
		
		if (heartManager != null)
		{
			heartManager.ResetHearts();  // Сбрасываем сердечки в начальное состояние
		}
		
		// Сбрасываем неуязвимость
		PlayerController playerController = FindObjectOfType<PlayerController>();
		if (playerController != null)
		{
			playerController.ResetInvincibility();  // Сбрасываем неуязвимость
			playerController.DisableControlForDuration(0, false);
			playerController.ResetPlayerControl();
		}
		
		// Перезапускаем движение объектов
		if (startDesignObject != null)
		{
			startDesignObject.RestartMovement(); // Восстанавливаем движение объекта
		}
		
		Move_Floor[] moveFloors = FindObjectsOfType<Move_Floor>();
		foreach (var moveFloor in moveFloors)
		{
			moveFloor.RestartMovement();  // Восстанавливаем движение объектов
		}
		
				    // Возвращаем физику игроку, если она была отключена
		Rigidbody2D playerRigidbody = playerController?.GetComponent<Rigidbody2D>();
		if (playerRigidbody != null)
		{
			playerRigidbody.isKinematic = false; // Включаем физику
		}
		
		// Возвращаем анимации
		Animator playerAnimator = playerController?.GetComponent<Animator>();
		if (playerAnimator != null)
		{
			playerAnimator.speed = 1; // Возвращаем скорость анимации
		}
		
		PlayerAbilities playerAbilities = FindObjectOfType<PlayerAbilities>();
		if (playerAbilities != null)
		{
			playerAbilities.ResetAbilities();
		}

		// Восстанавливаем скорость игры
		Time.timeScale = 1f;

		// Деактивируем игровую панель
		if (gamePanel != null)
		{
			gamePanel.SetActive(false);
		}

		// Активируем главное меню
		if (mainMenu != null)
		{
			mainMenu.SetActive(true);
		}

		// Деактивируем панель Game Over, если она активна
		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(false);
		}

		// Закрываем панель паузы (если она открыта)
		if (pauseMenuPanel != null)
		{
			pauseMenuPanel.SetActive(false);  // Закрываем панель паузы
		}
		
		if (finishPanel != null)
		{
			finishPanel.SetActive(false);
		}

		// Сбрасываем флаг паузы, чтобы при следующем нажатии пауза срабатывала сразу
		isGamePaused = false;
		
		// Очищаем все объекты на экране (например, врагов, препятствия)
		DestroyObjects(); // Вызов метода очистки объектов

		Debug.Log("Возвращаемся в главное меню.");
	}
	
	// Перезапускаем уровень с самого начала
	public void RestartLevel()
	{
		// Запускаем анимацию перехода (затемнение и отображение текста)
		StartCoroutine(TransitionToRestartLevel());
	}
	
	private IEnumerator TransitionToRestartLevel()
	{
		
		// Можно скрыть панель паузы (если она была открыта)
		pauseMenuPanel.SetActive(false);
		isGamePaused = false;
		
		// Удаляем всех миньонов перед перезапуском уровня
		DestroyAllMinions();
		
		Time.timeScale = 0f;
		
		// Скрыть панель финиша перед перезапуском уровня
		if (finishPanel != null)
		{
			finishPanel.SetActive(false);
		}
		
		// 1. Плавное затемнение экрана
		yield return StartCoroutine(FadeToBlack());
		
		// Получаем индекс текущего уровня
		int levelIndex = levelManager.currentLevelIndex;

		if (levelManager != null)
		{
			// Удаляем старый клон текущего уровня
			levelManager.RemoveCurrentLevel();

			// Загружаем текущий уровень заново
			levelManager.ActivateLevel(levelIndex);
		}
		
		// Восстанавливаем начальные позиции объектов
		if (levelManager != null)
		{
			levelManager.ResetObjectsToInitialPositions();  // Восстановим начальные позиции объектов
		}
		
		// Перезапускаем движение объектов Start_Design
		if (startDesignObject != null)
		{
			startDesignObject.RestartMovement(); // Восстанавливаем движение объекта
		}
		
		// 2. Плавное появление текста с названием уровня
		yield return StartCoroutine(ShowLevelText());

		// 3. Убираем затемнение и текст плавно
		yield return StartCoroutine(FadeFromBlack());
		Time.timeScale = 1f;
		// Восстанавливаем движение объектов Move_Floor
		Move_Floor[] moveFloors = FindObjectsOfType<Move_Floor>();
		foreach (var moveFloor in moveFloors)
		{
			moveFloor.RestartMovement();  // Восстанавливаем движение объектов
		}

		// Восстанавливаем здоровье игрока
		PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
		if (playerHealth != null)
		{
			playerHealth.RestartGame();  // Восстанавливаем здоровье
		}
		
		// Востановить ХП после рестарта. 
		if (heartManager != null)
		{
			heartManager.ResetHearts();
		}

		// Сбрасываем неуязвимость
		PlayerController playerController = FindObjectOfType<PlayerController>();
		if (playerController != null)
		{
			playerController.ResetInvincibility();  // Сбрасываем неуязвимость
			playerController.DisableControlForDuration(0, false); // Сбрасываем блокировку управления
			playerController.ResetPlayerControl();
		}
		
		Rigidbody2D playerRigidbody = FindObjectOfType<PlayerController>()?.GetComponent<Rigidbody2D>();
		if (playerRigidbody != null)
		{
			playerRigidbody.isKinematic = false; // Включаем физику
		}

		Animator playerAnimator = FindObjectOfType<PlayerController>()?.GetComponent<Animator>();
		if (playerAnimator != null)
		{
			playerAnimator.speed = 1; // Возвращаем скорость анимации
		}
		
		PlayerAbilities playerAbilities = FindObjectOfType<PlayerAbilities>();
		if (playerAbilities != null)
		{
			playerAbilities.ResetAbilities();
		}

		// Можно скрыть панель паузы (если она была открыта)
		pauseMenuPanel.SetActive(false);
		isGamePaused = false;

		// Очищаем все объекты на экране (например, врагов, препятствия)
		//DestroyObjects();  // Вызов метода очистки объектов
	}
	
	public IEnumerator FadeToBlack()
	{
		float fadeDuration = 1f;
		float elapsedTime = 0f;

		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.unscaledDeltaTime;
			float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
			blackPanel.GetComponent<Image>().color = new Color(0, 0, 0, alpha);
			yield return null;
		}
		blackPanel.GetComponent<Image>().color = Color.black;
	}

	public IEnumerator FadeFromBlack()
	{
		float fadeDuration = 1f;
		float elapsedTime = 0f;

		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.unscaledDeltaTime;
			float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
			blackPanel.GetComponent<Image>().color = new Color(0, 0, 0, alpha);
			yield return null;
		}
		blackPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0);
	}

	public IEnumerator ShowLevelText()
	{
		float fadeDuration = 1f;  // Время для появления и исчезновения текста
		float elapsedTime = 0f;

		levelNameText.text = levelManager.levelObjects[levelManager.currentLevelIndex].name; // Устанавливаем название уровня
		levelNameText.alpha = 0;  // Начальная прозрачность текста

		// Плавное появление текста
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.unscaledDeltaTime;
			levelNameText.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
			yield return null;
		}
		levelNameText.alpha = 1f;  // Обеспечиваем, что текст стал полностью видимым

		// Пауза на несколько секунд перед исчезновением текста (по желанию)
		//yield return new WaitForSeconds(1f); // Можно изменить время задержки, если нужно

		// Плавное исчезновение текста
		elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.unscaledDeltaTime;
			levelNameText.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
			yield return null;
		}
		levelNameText.alpha = 0f;  // Текст исчезает
	}
	
	private void DestroyObjects()
	{
		// Найдем все объекты с тегом "Obstacle" и удалим их
		GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
		foreach (var obstacle in obstacles)
		{
			Destroy(obstacle);  // Удаляем каждое препятствие
		}

		// Если есть другие объекты, которые должны быть удалены, добавь их сюда
	}
	
	public void SetCurrentLevelIndex(int levelIndex)
	{
		currentLevelIndex = levelIndex;
	}

    // Открытие панели настроек
    void OpenSettings()
    {
        Debug.Log("Открываем меню настроек...");
    }
	
    // Панель Game Over
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // В случае проигрыша, вызываем панель Game Over
    public void OnPlayerDeath()
    {
        ShowGameOverPanel();
    }
	
	private void DestroyAllMinions()
	{
		MinionBehavior[] minions = FindObjectsOfType<MinionBehavior>();
		foreach (var minion in minions)
		{
			Destroy(minion.gameObject);
		}
		Debug.Log("Все миньоны удалены.");
	}
}
