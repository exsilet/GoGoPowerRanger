using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YG;

public class FinishPanelController : MonoBehaviour
{
    // Ссылки на кнопки
    public Button mainMenuButton;  // Кнопка "Главное меню"
    public Button settingsButton;  // Кнопка "Настройки"
    public Button restartLevelButton;  // Кнопка "Рестарт уровня"
    public Button nextLevelButton;  // Кнопка "Следующий уровень"
	public Button pauseButton; // Ссылка на кнопку паузы (скрытие при вызове)
	private LevelManager levelManager; // Ссылка на LevelManager
	public FinishObject finishObject; // Ссылка на объект финиша
	
	public GameObject blackPanel;  // Панель, которая будет затемнять экран
	public TextMeshProUGUI levelNameText;  // TMP текст для отображения названия уровня
	public float transitionDuration = 2f;  // Время анимации
	public Image blackoutImage;  // Ссылка на изображение для затемнения
    public TMP_Text levelText;   // Ссылка на текстовый компонент для уровня
	
	public TextMeshProUGUI finishLevelNameText; // TMP текст для отображения уровня на финиш-панели

    // Панель Finish
    public GameObject finishPanel;

    // Ссылка на PauseMenuController
    private PauseMenuController pauseMenuController;
	
	private bool isAdForNextLevel = false; // Флаг для управления вызовами рекламы

    void Start()
    {
        // Инициализация
        pauseMenuController = FindObjectOfType<PauseMenuController>();

        // Подключаем кнопки к методам
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        restartLevelButton.onClick.AddListener(OnRestartLevelClicked);
        nextLevelButton.onClick.AddListener(OnNextLevelClicked);
		
		levelManager = FindObjectOfType<LevelManager>();
		
		// Сохраняем текущий прогресс перед показом рекламы
		SaveManager saveManager = FindObjectOfType<SaveManager>();
		if (saveManager != null)
		{
			saveManager.SaveData(); // Сохраняем данные
			saveManager.SaveLevelProgress(levelManager); // Сохраняем прогресс уровня
			Debug.Log("Сохранение выполнено перед переходом на следующий уровень.");
		}
    }
	
    // Обработчик для кнопки "Возврат в главное меню"
    void OnMainMenuClicked()
    {
        // Проверяем, есть ли pauseMenuController
        if (pauseMenuController != null)
        {
            // Вызываем метод GoToMainMenu() из PauseMenuController
            pauseMenuController.GoToMainMenu();
        }
        else
        {
            Debug.LogError("PauseMenuController не найден на сцене!");
        }

        // После возвращения в главное меню, скрываем панель Finish
        if (finishPanel != null)
        {
            finishPanel.SetActive(false);
        }
		
		if (pauseButton != null)
        {
            pauseButton.interactable = true;  // Делаем кнопку паузы неактивной
        }
    }

    // Обработчик для кнопки "Настройки"
    void OnSettingsClicked()
    {
        // Открываем настройки
        OpenSettingsMenu();
    }

    // Обработчик для кнопки "Рестарт уровня"
    void OnRestartLevelClicked()
    {
        // Перезапуск уровня
        if (pauseMenuController != null)
        {
            pauseMenuController.RestartLevel();
        }
		
		if (pauseButton != null)
        {
            pauseButton.interactable = true;  // Делаем кнопку паузы неактивной
        }
    }

    // Обработчик для кнопки "Следующий уровень"
	// void OnNextLevelClicked()
	// {
		//Запускаем анимацию перехода
		// StartCoroutine(TransitionToNextLevel());
	// }
	
	private IEnumerator TransitionToNextLevel()
	{
		Time.timeScale = 0f;
				
		if (finishPanel != null)
		{
			CanvasGroup canvasGroup = finishPanel.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				// Если CanvasGroup отсутствует, добавляем его
				canvasGroup = finishPanel.AddComponent<CanvasGroup>();
			}

			// Делаем панель и её дочерние элементы невидимыми
			canvasGroup.alpha = 0f; // Устанавливаем прозрачность
			canvasGroup.interactable = false; // Отключаем интерактивность
			canvasGroup.blocksRaycasts = false; // Отключаем блокировку лучей
		}
		
		// 1. Плавное затемнение экрана
		yield return StartCoroutine(FadeToBlack());
		
		Time.timeScale = 0f;
		
				// 3. Удаляем текущий клон уровня
		levelManager.RemoveCurrentLevel();

		// 4. Загружаем следующий доступный уровень
		levelManager.LoadNextSubLevel();
		
		Time.timeScale = 0f;
		
		// Восстанавливаем начальные позиции объектов
		if (levelManager != null)
		{
			levelManager.ResetObjectsToInitialPositions();  // Восстановим начальные позиции объектов
		}
		
		// Перезапускаем движение объектов Start_Design
		Start_Design[] startDesignObjects = FindObjectsOfType<Start_Design>();
		foreach (Start_Design startDesign in startDesignObjects)
		{
			startDesign.RestartMovement();  // Восстанавливаем движение для каждого объекта
		}

		// 6. Восстанавливаем движение объектов Move_Floor
		Move_Floor[] moveFloors = FindObjectsOfType<Move_Floor>();
		foreach (var moveFloor in moveFloors)
		{
			moveFloor.RestartMovement();
		}

		// 7. Восстанавливаем здоровье игрока
		PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
		if (playerHealth != null)
		{
			playerHealth.RestartGame();
		}
		
		Time.timeScale = 0f;

		// 2. Плавное появление текста с названием уровня
		yield return StartCoroutine(ShowLevelText());
		
		// 3. Убираем затемнение и текст плавно
		yield return StartCoroutine(FadeFromBlack());
		
		if (finishPanel != null)
		{
			CanvasGroup canvasGroup = finishPanel.GetComponent<CanvasGroup>();
			if (canvasGroup != null)
			{
				canvasGroup.alpha = 1f; // Восстанавливаем прозрачность
				canvasGroup.interactable = true; // Включаем интерактивность
				canvasGroup.blocksRaycasts = true; // Включаем блокировку лучей
			}
		}
		
		//Скрываем панель финиша
		if (finishPanel != null)
		{
			finishPanel.SetActive(false);
		}
		
		
        if (pauseButton != null)
        {
            pauseButton.interactable = true;  // Делаем кнопку паузы неактивной
        }
		
		Time.timeScale = 1f;
	}
	
	private IEnumerator FadeToBlack()
	{
		float fadeDuration = 1f;
		float time = 0f;

		// Постепенно затемняем экран
		while (time < fadeDuration)
		{
			time += Time.unscaledDeltaTime; // Используем unscaledDeltaTime, чтобы анимация не зависела от времени игры
			float alpha = Mathf.Lerp(0, 1, time / fadeDuration); // Интерполяция от 0 до 1
			blackoutImage.color = new Color(0, 0, 0, alpha); // Изменяем альфа-канал
			yield return null;
		}
		blackoutImage.color = Color.black; // Устанавливаем финальный цвет (черный)
	}

	private IEnumerator ShowLevelText()
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
	
	private IEnumerator FadeFromBlack()
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
	
    // Открытие панели настроек
    void OpenSettingsMenu()
    {
        Debug.Log("Открываем меню настроек...");
        // Здесь можно активировать панель настроек, например:
        // settingsPanel.SetActive(true);
    }

    // Показать панель Finish
    public void ShowFinishPanel()
    {
		Debug.Log("ShowFinishPanel вызван"); // Проверяем, вызывается ли метод
        finishPanel.SetActive(true);  // Показать панель финиша
		
		// Получаем название текущего уровня и устанавливаем текст
		if (levelManager != null && finishLevelNameText != null)
		{
			string levelName = levelManager.levelObjects[levelManager.currentLevelIndex].name;
			Debug.Log($"Level Name: {levelName}"); // Проверяем, что имя уровня получено
			finishLevelNameText.text = levelName;
		}
		
        // Отключаем кнопку паузы
        if (pauseButton != null)
        {
            pauseButton.interactable = false;  // Делаем кнопку паузы неактивной
        }
    }
	
	    // Обработчик для кнопки "Следующий уровень"
    void OnNextLevelClicked()
    {
        Debug.Log("Показываем рекламу перед переходом на следующий уровень.");
        isAdForNextLevel = true; // Устанавливаем флаг для следующего уровня
		
		// Сохраняем текущий прогресс перед показом рекламы
		SaveManager saveManager = FindObjectOfType<SaveManager>();
		if (saveManager != null)
		{
			saveManager.SaveData(); // Сохраняем данные
			saveManager.SaveLevelProgress(levelManager); // Сохраняем прогресс уровня
		}

        // Подписываемся на события
        YG2.onOpenAnyAdv += OnAdStarted;
        YG2.onCloseInterAdv += OnNextLevelAdFinished;
        YG2.InterstitialAdvShow();
    }

    private void OnAdStarted()
    {
        Debug.Log("Реклама началась.");
        Time.timeScale = 0; // Ставим игру на паузу
    }

    private void OnNextLevelAdFinished()
    {
        if (!isAdForNextLevel)
        {
            Debug.Log("Реклама завершена, но не для следующего уровня. Игнорируем.");
            return;
        }

        Debug.Log("Реклама завершена для следующего уровня. Переходим на следующий уровень.");
        isAdForNextLevel = false; // Сбрасываем флаг

        // Отписываемся от событий
        YG2.onOpenAnyAdv -= OnAdStarted;
        YG2.onCloseInterAdv -= OnNextLevelAdFinished;

        Time.timeScale = 1; // Возвращаем время

        // Запускаем анимацию перехода на следующий уровень
        StartCoroutine(TransitionToNextLevel());
    }
}
