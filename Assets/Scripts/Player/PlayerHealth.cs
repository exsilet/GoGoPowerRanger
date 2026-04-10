using UnityEngine;
using System.Collections;
using YG;

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
	    ResetHealth();
	    mainCamera = Camera.main;
	    originalCameraPosition = mainCamera.transform.position;

	    if (heartManager != null)
		    heartManager.UpdateHeartDisplay();
    }

    // Метод получения урона
    public void TakeDamage(int damage)
    {
	    if (GetComponent<PlayerController>().isInvincible)
	    {
		    Debug.Log("Урон заблокирован щитом!");
		    return;
	    }

	    currentHealth -= damage;
	    Debug.Log("Получен урон! Текущее здоровье: " + currentHealth);

	    bool isFatalHit = currentHealth <= 0;

	    if (!isFatalHit && useShakeEffect && !isShaking)
	    {
		    StartCoroutine(ShakeCamera());
	    }

	    PlayDamageSound();

	    if (currentHealth <= 0)
	    {
		    Die();
	    }

	    if (heartManager != null)
	    {
		    heartManager.UpdateHeartDisplay();
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
	    {
		    playerController.StopBlinkEffect();
		    playerController.FreezePlayerOnDeath();
	    }

	    if (playerAbilities != null)
		    playerAbilities.ResetAbilities();

	    Time.timeScale = 0f;
	    YG2.GameplayStop();

	    GameObject levelObject = GameObject.FindGameObjectWithTag("LVL_1");
	    if (levelObject != null)
	    {
		    AudioSource audio = levelObject.GetComponent<AudioSource>();
		    if (audio != null)
			    audio.Pause();
	    }

	    if (deathEffect != null)
		    Instantiate(deathEffect, transform.position, Quaternion.identity);

	    if (gameOverPanel != null)
		    StartCoroutine(ShowGameOverPanelAfterEffect());
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
	    PlayerController playerController = GetComponent<PlayerController>();
	    PlayerAbilities playerAbilities = GetComponent<PlayerAbilities>();

	    if (playerController != null)
		    playerController.StopBlinkEffect();

	    if (playerAbilities != null)
		    playerAbilities.ResetAbilities();

	    ResetHealth();
	    Time.timeScale = 1f;
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
		isShaking = true;
		float shakeTime = 0f;

		Vector3 baseCameraPosition = mainCamera.transform.position;

		while (shakeTime < shakeDuration)
		{
			shakeTime += Time.unscaledDeltaTime;

			if (Time.timeScale == 0)
			{
				yield return null;
				continue;
			}

			float xShake = Random.Range(-shakeStrength, shakeStrength);
			float yShake = Random.Range(-shakeStrength, shakeStrength);
			Vector3 shakeOffset = new Vector3(xShake, yShake, 0);

			mainCamera.transform.position = baseCameraPosition + shakeOffset;

			yield return null;
		}

		mainCamera.transform.position = baseCameraPosition;
		isShaking = false;
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