using UnityEngine;

public class FinishObject : MonoBehaviour
{
    public float targetX = 10f; // Целевая координата по X
    public float moveSpeed = 5f; // Скорость перемещения

    private bool hasReachedTarget = false; // Флаг для остановки движения
    private bool isBossInView = false;  // Флаг для проверки, вошел ли босс в поле зрения

    public GameObject[] bossesOnLevel;  // Массив объектов боссов на уровне (например, для 1-5, 2-5 и т.д.)

    private LevelManager levelManager; // Ссылка на LevelManager для уведомления о завершении уровня
    public int currentSubLevelIndex;  // Индекс текущего подуровня
	private FinishPanelController finishPanelController; // Ссылка на FinishPanelController для вызова удаления уровня
	private SaveManager savemanager;

    void Start()
	{
		levelManager = FindObjectOfType<LevelManager>();  // Получаем ссылку на LevelManager
		finishPanelController = FindObjectOfType<FinishPanelController>(); // Получаем ссылку на FinishPanelController
		savemanager = FindObjectOfType<SaveManager>();

		// Инициализируем currentSubLevelIndex из значения levelManager
		// Получаем текущий подуровень
		currentSubLevelIndex = levelManager.GetCurrentSubLevelIndex(); 

		Debug.Log($"Текущий подуровень: {currentSubLevelIndex}");  // Для дебага
	}

    void Update()
    {
        // Проверяем, есть ли активные боссы на уровне
        if (!hasReachedTarget && AreAllBossesDefeated())
        {
            // Если боссов нет, финиш двигается до целевой точки
            MoveToTarget();
        }

        // Если есть босс в поле зрения, проверяем, достиг ли финиш своей цели
        if (AreAllBossesDefeated() && isBossInView)
        {
            MoveToTarget();
        }
    }

    // Двигаем финишный объект
    private void MoveToTarget()
	{
		// Двигаем объект по оси X
		float step = moveSpeed * Time.deltaTime; // Рассчитываем шаг
		transform.position = new Vector3(
			Mathf.MoveTowards(transform.position.x, targetX, step),
			transform.position.y,
			transform.position.z
		);

		// Проверяем, достиг ли объект цели
		if (Mathf.Approximately(transform.position.x, targetX))
		{
			hasReachedTarget = true; // Останавливаем движение
			Move_Floor.stopMovement = true; // Останавливаем движение пола
			savemanager.SaveLevelProgress(levelManager);

			if (finishPanelController != null)
			{
				finishPanelController.ShowFinishPanel(); // Вызываем метод отображения финиш-панели
				Debug.Log("ShowFinishPanel вызван"); // Проверяем, вызывается ли метод
			}
			else
			{
				Debug.LogWarning("FinishPanelController не найден!");
			}

			// Теперь проверим, не был ли подуровень уже завершен:
			if (currentSubLevelIndex == levelManager.unlockedSubLevel)
			{
				// Если текущий подуровень не был завершен ранее, передаем его в LevelManager
				levelManager.CompleteSubLevel(currentSubLevelIndex);
				// Добавляем дебаг, чтобы отслеживать какой подуровень был завершен
				Debug.Log($"Завершен подуровень {currentSubLevelIndex}. Передан в LevelManager.");

				// Обновляем unlockedSubLevel на следующий
				currentSubLevelIndex++; // Увеличиваем текущий подуровень
				Debug.Log($"Следующий доступный подуровень: {levelManager.unlockedSubLevel}");
			}
			else
			{
				// Если подуровень уже был пройден
				Debug.Log($"Подуровень {currentSubLevelIndex} уже был завершен ранее. Ничего не происходит.");
			}
		}
	}
    // Проверка, все ли боссы уничтожены
    private bool AreAllBossesDefeated()
    {
        foreach (GameObject boss in bossesOnLevel)
        {
            if (boss != null && boss.activeInHierarchy)
            {
                return false; // Если хотя бы один босс еще жив
            }
        }
        return true; // Если все боссы уничтожены
    }

    // Проверяем, вошел ли босс в поле зрения игрока
    public void OnBossEnteredView()
    {
        isBossInView = true; // Босс вошел в поле зрения
    }

    // Проверка на смерть босса
    public void OnBossDefeated()
    {
        // Когда босс убит, продолжаем движение финиша
        isBossInView = false; // Останавливаем проверку на поле зрения
        MoveToTarget();  // Продолжаем движение
    }
}
