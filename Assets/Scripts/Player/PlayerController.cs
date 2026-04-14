using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;        // Обычная скорость движения
    public float boostSpeed = 10f;      // Скорость при ускорении
    public float boostDistance = 2f;    // Дистанция, на которую игрок продвигается вперед при ускорении
    public float boostDuration = 0.5f;  // Длительность ускорения

    public bool isBoosting = false;    // Флаг для состояния ускорения
    private Vector3 targetPosition;     // Целевая точка при ускорении
	public ParticleSystem destructionEffect;
	public ParticleSystem crackBossEffect;
	private bool isControlDisabled = false;

    // Ограничения для движения по трассе
    private float minX = -8f;
    private float maxX = 8f;
    private float minY = -4.3f;
    private float maxY = 4.3f;

    // Ссылка на компонент здоровья
    private PlayerHealth playerHealth;

    // Переменная для контроля неуязвимости и мигания
    public bool isInvincible = false;   // Теперь используется для предотвращения получения повторного урона
    private float invincibleTime = 1f;   // Время неуязвимости после удара
	
	public AudioSource audioSource;  // Ссылка на компонент AudioSource
    public AudioClip destructionSound; // Звук разрушения
	
	public AudioClip boostSound; // Звук для ускорения
	public AudioSource crackBossDestructionSound; // Звук разрушения CrackBoss
	
	public AudioClip disableControlSound; // Новый звук для отключения управления
	
	public GameObject keyA;               // Объект для отображения буквы A
	public GameObject keyD;               // Объект для отображения буквы D
	public ParticleSystem keyAParticle;   // Партикл для буквы A
	public ParticleSystem keyDParticle;   // Партикл для буквы D
	
	private Coroutine blinkCoroutine;
	private Coroutine disableControlCoroutine;
	private SpriteRenderer spriteRenderer;
	private SafeZone safeZone;
	
	// Ссылки для мобильного управления
    public Joystick joystick; // Джойстик для мобильных устройств

    // Определяем платформу
    private Platform currentPlatform;
    private bool isMobile;
	
	public Button boostButton; // Ссылка на кнопку для мобильного буста

    void Start()
    {
	    spriteRenderer = GetComponent<SpriteRenderer>();
	    safeZone = FindObjectOfType<SafeZone>();
	    
        // Получаем компонент PlayerHealth
        playerHealth = GetComponent<PlayerHealth>();
		
		currentPlatform = PlatformDetector.GetPlatform();
		
		isMobile = YG2.envir.isMobile || YG2.envir.isTablet;

        // Отключаем джойстик, если это десктоп
        if (!isMobile && joystick != null)
        {
            joystick.gameObject.SetActive(false);
        }
		
		// Привязываем метод Boost() к кнопке
        if (boostButton != null)
        {
            boostButton.onClick.AddListener(Boost);
        }
    }

    void Update()
	{
		if (isControlDisabled)
			return;
		
		if (isControlDisabled) return;
		
		if (isMobile)
        {
            MobileControl();
        }
        else
        {
            DesktopControl();
        }

		// Получаем данные от игрока для движения
		// float horizontal = Input.GetAxis("Horizontal");
		// float vertical = Input.GetAxis("Vertical");
		
		// SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		// if (Input.GetKey(KeyCode.A))
		// {
			// spriteRenderer.flipX = true; // Включаем FlipX при нажатии A
		// }
		// else if (Input.GetKey(KeyCode.D))
		// {
			// spriteRenderer.flipX = false; // Выключаем FlipX при нажатии D
		// }

		if (isBoosting)
		{
			// Ускоренное движение игрока к целевой точке
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, boostSpeed * Time.deltaTime);

			// Проверяем, достиг ли игрок целевой точки
			if (transform.position == targetPosition)
			{
				isBoosting = false;
			}
		}

		// Ускорение при нажатии пробела
		// if (Input.GetKeyDown(KeyCode.Space) && !isBoosting)
		// {
			// Boost();
		// }

		// Ограничиваем движение игрока по границам трассы
		float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
		float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
		transform.position = new Vector3(clampedX, clampedY, transform.position.z);
		
	}

    void Boost()
	{
		Vector3 boostDirection;

		// Проверяем, используется ли джойстик на мобильных устройствах
		if (isMobile && joystick != null)
		{
			// Получаем направление из джойстика
			boostDirection = new Vector3(joystick.Horizontal, joystick.Vertical, 0).normalized;

			// Если направление джойстика слишком слабое (почти не перемещается), устанавливаем направление по умолчанию
			if (boostDirection.magnitude < 0.1f)
			{
				boostDirection = Vector3.right; // По умолчанию вправо
			}
		}
		else
		{
			// Если это ПК, используем клавиши для определения направления
			if (Input.GetKey(KeyCode.A)) // Движение влево
			{
				boostDirection = Vector3.left;
			}
			else if (Input.GetKey(KeyCode.D)) // Движение вправо
			{
				boostDirection = Vector3.right;
			}
			else
			{
				boostDirection = Vector3.right; // По умолчанию вправо
			}
		}

		// Рассчитываем целевую точку для ускорения
		targetPosition = transform.position + boostDirection * boostDistance;

		// Ограничиваем целевую точку, чтобы она не вышла за пределы карты
		float clampedTargetX = Mathf.Clamp(targetPosition.x, minX, maxX);
		float clampedTargetY = Mathf.Clamp(targetPosition.y, minY, maxY);
		targetPosition = new Vector3(clampedTargetX, clampedTargetY, targetPosition.z);

		isBoosting = true;

		// Воспроизводим звук ускорения
		if (boostSound != null && audioSource != null)
		{
			audioSource.PlayOneShot(boostSound);
			Debug.Log("Boost sound played");
		}
	}
	
	// Управление для ПК
    void DesktopControl()
    {
	    float horizontal = Input.GetAxis("Horizontal");
	    float vertical = Input.GetAxis("Vertical");

	    Vector3 movement = new Vector3(horizontal, vertical, 0f);
	    transform.position += movement * moveSpeed * Time.deltaTime;

	    if (spriteRenderer != null)
	    {
		    if (Input.GetKey(KeyCode.A))
			    spriteRenderer.flipX = true;
		    else if (Input.GetKey(KeyCode.D))
			    spriteRenderer.flipX = false;
	    }

	    if (Input.GetKeyDown(KeyCode.LeftShift) && !isBoosting)
	    {
		    Boost();
	    }
    }

    // Управление для мобильных
    void MobileControl()
    {
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;

        Vector3 movement = new Vector3(horizontal, vertical, 0f);
        transform.position += movement * moveSpeed * Time.deltaTime;
    }
	
    public void DisableControlForDuration(float duration, bool playSound = true)
    {
	    if (disableControlCoroutine != null)
		    StopCoroutine(disableControlCoroutine);

	    disableControlCoroutine = StartCoroutine(DisableControlCoroutine(duration, playSound));
    }

	private IEnumerator DisableControlCoroutine(float duration, bool playSound)
	{
		isControlDisabled = true;

		// Воспроизводим звук только если playSound == true
		if (playSound && audioSource != null && disableControlSound != null)
		{
			audioSource.PlayOneShot(disableControlSound);
			Debug.Log("Disable control sound played");
		}

		yield return new WaitForSecondsRealtime(duration);

		isControlDisabled = false;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("Crack"))
		{
			if (isBoosting)
			{
				// Если игрок находится в состоянии ускорения, разрушить трещину
				Debug.Log("Игрок пролетел через трещину и разрушил её!");

				// Создаём эффект разрушения
				TriggerDestructionEffect(collision.transform.position, destructionEffect);
				
				// Воспроизводим звук разрушения
                PlayDestructionSound();

				// Уничтожаем трещину
				Destroy(collision.gameObject);
			}
			else
			{
				// Если игрок не в ускорении, получать урон от трещины
				TakeDamageFromCrack();
			}
		}
		else if (collision.gameObject.CompareTag("CrackBoss"))
		{
			if (isBoosting)
			{
				// Игрок находится в состоянии ускорения, просто логируем событие
				Debug.Log("Игрок пролетел через CrackBoss, но ничего не произошло.");
				
				// Создаём эффект разрушения для CrackBoss
				TriggerDestructionEffect(collision.transform.position, crackBossEffect, true);
				
				// Уничтожаем трещину
				Destroy(collision.gameObject);
				
				PlayCrackBossDestructionSound();
			}
			else
			{
				// Игрок не в ускорении, можно добавить другую логику при необходимости
				Debug.Log("Игрок столкнулся с CrackBoss, но эффект не реализован.");
			}
		}
		else if (collision.gameObject.CompareTag("Obstacle") && !isInvincible)
		{
			// Игрок сталкивается с обычным препятствием и получает урон, если не неуязвим
			TakeDamageFromObstacle();
		}
	}

	void TriggerDestructionEffect(Vector3 position, ParticleSystem effect, bool rotateForCrackBoss = false)
	{
		if (effect != null)
		{
			Quaternion rotation = Quaternion.identity;

			// Устанавливаем поворот для CrackBoss
			if (rotateForCrackBoss)
			{
				rotation = Quaternion.Euler(-90f, 0f, 0f); // Устанавливаем Rotation X = -90
			}

			// Создаём систему частиц
			ParticleSystem spawnedEffect = Instantiate(effect, position, rotation);
			
			// Устанавливаем Sorting Layer и Order in Layer
			Renderer particleRenderer = spawnedEffect.GetComponent<Renderer>();
			if (particleRenderer != null)
			{
				particleRenderer.sortingLayerName = "Foreground"; // Укажите ваш Sorting Layer
				particleRenderer.sortingOrder = 2; // Укажите желаемый Order in Layer
			}

			// Уничтожаем систему частиц после завершения
			Destroy(spawnedEffect.gameObject, spawnedEffect.main.duration);
		}
		else
		{
			Debug.LogWarning("Попытка вызвать TriggerDestructionEffect без заданного эффекта.");
		}
	}
	
	public void FreezePlayerOnDeath()
	{
		isControlDisabled = true;
		isBoosting = false;
		targetPosition = transform.position;
		
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			rb.velocity = Vector2.zero;
			rb.angularVelocity = 0f;
		}

		if (disableControlCoroutine != null)
		{
			StopCoroutine(disableControlCoroutine);
			disableControlCoroutine = null;
		}
	}

	public void UnfreezeAfterRevive()
	{
		isControlDisabled = false;
		isBoosting = false;
		targetPosition = transform.position;
		transform.position = transform.position;
	}
	
	private void TakeDamageFromCrack()
	{
		if (!isInvincible)
		{
			playerHealth.TakeDamage(1);

			if (blinkCoroutine != null)
				StopCoroutine(blinkCoroutine);

			blinkCoroutine = StartCoroutine(BlinkEffect());
		}
	}
	
	public void ResetInvincibility()
	{
		isInvincible = false;  // Сбрасываем флаг неуязвимости
	}
	
	public void ForceResetControl()
	{
		isControlDisabled = false;

		if (disableControlCoroutine != null)
		{
			StopCoroutine(disableControlCoroutine);
			disableControlCoroutine = null;
		}
	}

    // Метод для получения урона при столкновении с препятствием
    private void TakeDamageFromObstacle()
    {
	    if (!isInvincible)
	    {
		    if (safeZone != null)
		    {
			    Debug.Log($"SafeZone status: {safeZone.IsPlayerInside()}");
			    if (safeZone.IsPlayerInside())
			    {
				    Debug.Log("Игрок в безопасной зоне, урон не нанесён.");
				    return;
			    }
		    }
		    else
		    {
			    Debug.LogWarning("SafeZone не найдена!");
		    }

		    Debug.Log("Игрок получил урон от препятствия!");
		    playerHealth.TakeDamage(1);

		    if (blinkCoroutine != null)
			    StopCoroutine(blinkCoroutine);

		    blinkCoroutine = StartCoroutine(BlinkEffect());
	    }
    }

    // Корутина для мигания и временной неуязвимости
    IEnumerator BlinkEffect()
    {
	    isInvincible = true;

	    SpriteRenderer playerSprite = spriteRenderer;
	    if (playerSprite == null)
		    yield break;

	    for (int i = 0; i < 5; i++)
	    {
		    playerSprite.color = new Color(1f, 1f, 1f, 0.5f);
		    yield return new WaitForSeconds(0.1f);
		    playerSprite.color = new Color(1f, 1f, 1f, 1f);
		    yield return new WaitForSeconds(0.1f);
	    }

	    yield return new WaitForSeconds(invincibleTime);
	    playerSprite.color = new Color(1f, 1f, 1f, 1f);
	    isInvincible = false;
    }
	
	public void StopBlinkEffect()
	{
		if (blinkCoroutine != null)
		{
			StopCoroutine(blinkCoroutine);
			blinkCoroutine = null;
		}

		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = true;
			spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
		}
	}
	
	void PlayDestructionSound()
    {
        if (destructionSound != null)
        {
            audioSource.PlayOneShot(destructionSound); // Воспроизводим звук
            Debug.Log("Destruction sound played");
        }
        else
        {
            Debug.LogWarning("Destruction sound not assigned");
        }
    }
	
	void PlayCrackBossDestructionSound()
	{
		if (crackBossDestructionSound != null)
		{
			crackBossDestructionSound.Play();
		}
	}
	
	// Включить буквы A и D
	public void EnableKeys()
	{
		if (keyA != null) keyA.SetActive(true);
		if (keyD != null) keyD.SetActive(true);
	}

	// Выключить буквы A и D
	public void DisableKeys()
	{
		if (keyA != null) keyA.SetActive(false);
		if (keyD != null) keyD.SetActive(false);
	}
	
	// Метод для изменения цвета буквы
	public void HighlightKey(GameObject key, Color color)
	{
		if (key != null)
		{
			var renderer = key.GetComponent<SpriteRenderer>();
			if (renderer != null)
			{
				renderer.color = color;
			}
		}
	}
	
	// Метод для сброса цвета буквы
	public void ResetKeyColor(GameObject key)
	{
		if (key != null)
		{
			var renderer = key.GetComponent<SpriteRenderer>();
			if (renderer != null)
			{
				renderer.color = Color.white; // Вернуть исходный цвет
			}
		}
	}
	
	// Метод для запуска партиклов
	public void PlayKeyParticle(ParticleSystem particle)
	{
		if (particle != null)
		{
			particle.Play();
		}
	}
	
	public void ResetPlayerControl()
	{
		isControlDisabled = false;
	}
	
	public void SetMinX(float value)
	{
		minX = value;
	}

	public void SetMaxX(float value)
	{
		maxX = value;
	}

}
