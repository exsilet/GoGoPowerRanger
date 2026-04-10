using System.Collections;
using UnityEngine;
using TMPro;


public class PlayerAbilities : MonoBehaviour
{
    [Header("SHIELD ABILITY")]
    public GameObject shieldObject;        // Объект щита
    public AudioClip shieldActivateSound;  // Звук активации щита
    public AudioClip shieldDeactivateSound; // Звук деактивации щита
    public float shieldDuration = 5f;      // Продолжительность действия щита
    private bool isShieldActive = false;   // Флаг активации щита
	public int shieldSkillCount = 2; // Счётчик зарядов для Щита
	public TextMeshProUGUI[] shieldCounterTexts; // Тексты для отображения зарядов Щита
	
    [Header("SLOW-MO ABILITY")]
    public float slowMoDuration = 10f;       // Длительность замедления
    public float slowMoScale = 0.2f;         // Степень замедления (0.2 = замедление до 20%)
    public AudioClip slowMoSound;           // Звук активации замедления
	public AudioClip slowMoEndSound; 
    private bool isSlowMoActive = false;    // Флаг активации замедления
	public GameObject slowMoEffect; // Ссылка на объект визуального эффекта
	public int slowMoSkillCount = 2; // Счётчик зарядов для Слоу-мо
	public TextMeshProUGUI[] slowMoCounterTexts; // Тексты для отображения зарядов Слоу-мо	
	
	[Header("HEAL ABILITY")]
    public int healSkillCount = 2; // Счётчик копилки
    public ParticleSystem healEffect; // Эффект при активации
    public AudioClip healSound; // Звук при активации
	public TextMeshProUGUI[] healCounterTexts;
	public int healAmount = 1; // Количество ХП для восстановления

    [Header("Dependencies")]
    private PlayerController playerController;
    private PlayerHealth playerHealth;
	public AudioSource abilityAudioSource;
	
	[Header("Level Dependencies")]
	public LevelManager levelManager; // Ссылка на LevelManager
	public GameObject shieldSkillUI;   // Объект UI для отображения щита
	public GameObject slowMoSkillUI;   // Объект UI для отображения Slow-Mo
	public bool isShieldAvailable = false;  // Доступность щита
	public bool isSlowMoAvailable = false;  // Доступность Slow-Mo
	
	[Header("Level Panels")]
	public GameObject panelLevel10; // Панель для уровня 10
	public GameObject panelLevel15; // Панель для уровня 15
	public GameObject panelLevel20; // Панель для уровня 20

    void Start()
    {		
		// LoadSkillCounts();
		
		levelManager = FindObjectOfType<LevelManager>();
		CheckAndActivatePanels();
		
        // Получаем зависимости
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
		
	    // Скрываем скиллы при старте
		if (shieldSkillUI != null) shieldSkillUI.SetActive(false);
		if (slowMoSkillUI != null) slowMoSkillUI.SetActive(false);

        // Убеждаемся, что щит отключен при старте
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
		
		// Убеждаемся, что щит отключен при старте
        if (slowMoEffect != null)
        {
            slowMoEffect.SetActive(false);
        }
		
		// Проверяем, назначен ли конкретный AudioSource
		if (abilityAudioSource == null)
		{
			Debug.LogWarning("AbilityAudioSource не назначен! Используется первый AudioSource.");
			abilityAudioSource = GetComponent<AudioSource>();
		}
		
		UpdateSkillAvailability();
    }

    void Update()
    {
	    if (Input.GetKeyDown(KeyCode.Alpha1))
	    {
		    ActivateHeal();
	    }

	    if (isShieldAvailable && Input.GetKeyDown(KeyCode.Alpha2))
	    {
		    ActivateShield();
	    }

	    if (isSlowMoAvailable && Input.GetKeyDown(KeyCode.Alpha3))
	    {
		    ActivateSlowMo();
	    }

	    UpdateHealCounterUI();
	    UpdateSlowMoCounterUI();
	    UpdateShieldCounterUI();
    }

    public void ActivateShield()
    {
		if (shieldSkillCount <= 0)
		{
			Debug.Log("Недостаточно зарядов для активации Щита!");
			return;
		}
		
        if (isShieldActive) return;
		
		shieldSkillCount--; // Уменьшаем заряд
		UpdateShieldCounterUI(); // Обновляем текст

        isShieldActive = true;

        // Включаем неуязвимость через PlayerController
        if (playerController != null)
        {
            playerController.isInvincible = true;
        }

        // Отображаем объект щита
        if (shieldObject != null)
        {
            shieldObject.SetActive(true);
        }

        // Воспроизводим звук активации
		if (shieldActivateSound != null && abilityAudioSource != null)
		{
			abilityAudioSource.PlayOneShot(shieldActivateSound);
		}
		
		// Постоянно обновляем неуязвимость, пока щит активен
		StartCoroutine(KeepInvincibilityActive());

        StartCoroutine(DeactivateShieldAfterDuration(shieldDuration));
    }

    private IEnumerator DeactivateShieldAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        isShieldActive = false;

        // Выключаем неуязвимость
        if (playerController != null)
        {
            playerController.isInvincible = false;
        }

        // Скрываем объект щита
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }

        // Воспроизводим звук деактивации
		if (shieldDeactivateSound != null && abilityAudioSource != null)
		{
			abilityAudioSource.PlayOneShot(shieldDeactivateSound);
		}
    }
	
	private IEnumerator KeepInvincibilityActive()
	{
		while (isShieldActive)
		{
			if (playerController != null)
			{
				playerController.isInvincible = true;
			}
			yield return null; // Ждём следующий кадр
		}
	}
	
	public void AddShieldSkill(int amount)
	{
		shieldSkillCount += amount;
		UpdateShieldCounterUI();
		Debug.Log($"Добавлено зарядов Щита: {amount}. Всего: {shieldSkillCount}");
	}
	
	
	public void UpdateShieldCounterUI()
	{
		foreach (var tmp in shieldCounterTexts)
		{
			if (tmp != null)
			{
				tmp.text = shieldSkillCount.ToString(); // Обновляем текст
			}
		}
	}
	
	public void ActivateShieldFromUI()
	{
		ActivateShield();
	}
	
	public void ActivateSlowMo()
    {
		if (slowMoSkillCount <= 0)
		{
			Debug.Log("Недостаточно зарядов для активации Слоу-мо!");
			return;
		}
		
        if (isSlowMoActive) return;
		
		slowMoSkillCount--; // Уменьшаем заряд
		UpdateSlowMoCounterUI(); // Обновляем текст

        isSlowMoActive = true;
		
		if (slowMoEffect != null)
		{
			slowMoEffect.SetActive(true); // Включаем объект
		}

        // Устанавливаем замедление времени
        Time.timeScale = slowMoScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Синхронизируем физику

        // Воспроизводим звук активации
		if (slowMoSound != null && abilityAudioSource != null)
		{
			abilityAudioSource.PlayOneShot(slowMoSound);
		}

        StartCoroutine(DeactivateSlowMoAfterDuration(slowMoDuration));
    }
	
	private IEnumerator DeactivateSlowMoAfterDuration(float duration)
    {		
        yield return new WaitForSecondsRealtime(duration); // Используем реальное время

        // Возвращаем время к нормальному состоянию
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // Возвращаем физику к нормальной скорости

        isSlowMoActive = false;
		
		if (slowMoEffect != null)
		{
			slowMoEffect.SetActive(false); // Выключаем объект
		}
		
		// Воспроизводим звук окончания
		if (slowMoEndSound != null && abilityAudioSource != null)
		{
			abilityAudioSource.PlayOneShot(slowMoEndSound);
		}
    }
	
	public void UpdateSlowMoCounterUI()
	{
		foreach (var tmp in slowMoCounterTexts)
		{
			if (tmp != null)
			{
				tmp.text = slowMoSkillCount.ToString(); // Обновляем текст
			}
		}
	}
	
	public void AddSlowMoSkill(int amount)
	{
		slowMoSkillCount += amount;
		UpdateSlowMoCounterUI();
		Debug.Log($"Добавлено зарядов Слоу-мо: {amount}. Всего: {slowMoSkillCount}");
	}
	
	public void AddHealthSkill(int amount)
    {
        healSkillCount += amount;
		UpdateHealCounterUI(); // Обновляем текст на UI
        Debug.Log($"Добавлено в копилку: {amount}. Текущая копилка: {healSkillCount}");
    }

    public void ActivateHeal()
	{
		if (healSkillCount <= 0)
		{
			Debug.Log("Недостаточно зарядов для лечения!");
			return;
		}

		// Уменьшаем счётчик и восстанавливаем здоровье
		healSkillCount--;
		UpdateHealCounterUI(); // Обновляем текст на UI после использования
		if (playerHealth != null)
		{
			playerHealth.IncreaseHealth(healAmount); // Восстанавливаем 1 здоровье

			// Уведомляем HeartManager об обновлении сердечек
			if (playerHealth.heartManager != null)
			{
				playerHealth.heartManager.UpdateHeartDisplay();
			}
		}

		// Активируем эффект
		if (healEffect != null)
		{
			healEffect.Play();
		}

		// Воспроизводим звук
		if (healSound != null && abilityAudioSource != null)
		{
			abilityAudioSource.PlayOneShot(healSound);
		}

		Debug.Log($"Использован хил. Осталось зарядов: {healSkillCount}");
	}
	
	public void UpdateHealCounterUI()
	{
		foreach (var tmp in healCounterTexts)
		{
			if (tmp != null)
			{
				tmp.text = healSkillCount.ToString(); // Обновляем текст
			}
		}
	}
	
	public void ResetAbilities()
	{
		// Сброс состояния щита
		if (isShieldActive && shieldObject != null)
		{
			isShieldActive = false;
			shieldObject.SetActive(false);

			if (playerController != null)
			{
				playerController.isInvincible = false; // Выключаем неуязвимость
			}
		}

		// Сброс состояния замедления времени
		if (isSlowMoActive)
		{
			isSlowMoActive = false;
			Time.timeScale = 1f; // Возвращаем нормальное время
			Time.fixedDeltaTime = 0.02f;

			if (slowMoEffect != null)
			{
				slowMoEffect.SetActive(false); // Выключаем эффект
			}
		}

		// Сброс счётчика лечения
		//healSkillCount = 0;
		UpdateHealCounterUI(); // Обновляем текст UI
	}
	
	public void UpdateSkillAvailability()
	{
		if (levelManager == null) return;

		int currentLevel = levelManager.unlockedSubLevel;

		isShieldAvailable = currentLevel >= 5;
		if (shieldSkillUI != null)
			shieldSkillUI.SetActive(isShieldAvailable);

		isSlowMoAvailable = currentLevel >= 20;
		if (slowMoSkillUI != null)
			slowMoSkillUI.SetActive(isSlowMoAvailable);
	}
	
	private void CheckAndActivatePanels()
	{
		if (levelManager == null)
		{
			Debug.LogWarning("LevelManager не найден!");
			return;
		}

		int currentLevel = levelManager.unlockedSubLevel;

		if (panelLevel10 != null)
			panelLevel10.SetActive(currentLevel == 10);

		if (panelLevel15 != null)
			panelLevel15.SetActive(currentLevel == 15);

		if (panelLevel20 != null)
			panelLevel20.SetActive(currentLevel == 20);
	}
	
	// public void SaveSkillCounts()
	// {
		// PlayerPrefs.SetInt("HealSkillCount", healSkillCount);
		// PlayerPrefs.SetInt("SlowMoSkillCount", slowMoSkillCount);
		// PlayerPrefs.SetInt("ShieldSkillCount", shieldSkillCount);
		// PlayerPrefs.Save(); // Применяем изменения
		// Debug.Log("Данные способностей сохранены.");
	// }
	
	// public void LoadSkillCounts()
	// {
		// UpdateHealCounterUI();
		// healSkillCount = PlayerPrefs.GetInt("HealSkillCount", 0);
		// slowMoSkillCount = PlayerPrefs.GetInt("SlowMoSkillCount", 0);
		// shieldSkillCount = PlayerPrefs.GetInt("ShieldSkillCount", 0);
		// Debug.Log("Данные способностей загружены.");
	// }
}
