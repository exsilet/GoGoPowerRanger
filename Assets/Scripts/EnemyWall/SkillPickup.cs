using System.Collections;
using UnityEngine;

public class SkillPickup : MonoBehaviour
{
    [Header("Skill Objects")]
    public GameObject healSkillObject;
    public GameObject shieldSkillObject;
    public GameObject slowMoSkillObject;

    [Header("Interaction Sound")]
    public AudioClip interactionSound;

    private AudioSource audioSource;
    private PlayerAbilities playerAbilities;
    private bool isPickedUp = false;
	private Coroutine cycleCoroutine; // Хранит ссылку на текущую корутину

    void Start()
    {
        // Находим PlayerAbilities на сцене
        playerAbilities = FindObjectOfType<PlayerAbilities>();
        if (playerAbilities == null)
        {
            Debug.LogError("PlayerAbilities не найден!");
            return;
        }

        // Проверяем наличие AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Обновляем видимость объектов в зависимости от доступных умений
        UpdateSkillObjectVisibility();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp || playerAbilities == null) return;

        if (other.CompareTag("Player"))
        {
            // Проверяем доступные умения и добавляем случайное
            GrantRandomSkill();

            // Проигрываем звук
            if (interactionSound != null)
            {
                audioSource.PlayOneShot(interactionSound);
            }

            // Делаем объект невидимым
            HidePickupObject();

            // Удаляем объект после завершения звука
            StartCoroutine(DestroyAfterSound());
        }
    }

    private void GrantRandomSkill()
    {
        // Получаем доступные умения
        playerAbilities.UpdateSkillAvailability();

        bool isShieldAvailable = playerAbilities.isShieldAvailable;
        bool isSlowMoAvailable = playerAbilities.isSlowMoAvailable;

        int randomSkill = -1;

        // Генерируем случайное умение на основе доступных
        if (isShieldAvailable && isSlowMoAvailable)
        {
            randomSkill = Random.Range(0, 3); // 0 = Heal, 1 = Shield, 2 = SlowMo
        }
        else if (isShieldAvailable)
        {
            randomSkill = Random.Range(0, 2); // 0 = Heal, 1 = Shield
        }
        else
        {
            randomSkill = 0; // Только Heal
        }

        // Применяем умение
        switch (randomSkill)
        {
            case 0:
                playerAbilities.AddHealthSkill(1);
                Debug.Log("Добавлен HealSkillCount");
                break;
            case 1:
                playerAbilities.AddShieldSkill(1);
                Debug.Log("Добавлен ShieldSkillCount");
                break;
            case 2:
                playerAbilities.AddSlowMoSkill(1);
                Debug.Log("Добавлен SlowMoSkillCount");
                break;
        }
    }

    private void UpdateSkillObjectVisibility()
	{
		
		// Если объект уже поднят, ничего не делаем
		if (isPickedUp)
		{
			return;
		}
		
		// Обновляем доступность умений
		playerAbilities.UpdateSkillAvailability();

		// Проверяем состояния
		bool isShieldAvailable = playerAbilities.isShieldAvailable;
		bool isSlowMoAvailable = playerAbilities.isSlowMoAvailable;

		// Если корутина уже запущена, ничего не делаем
		if (cycleCoroutine != null)
		{
			return;
		}

		// Включаем круговое переключение, если доступно несколько умений
		if (isShieldAvailable || isSlowMoAvailable)
		{
			cycleCoroutine = StartCoroutine(CycleSkillObjects());
		}
	}

    private IEnumerator CycleSkillObjects()
	{
		while (true)
		{
			// Heal
			healSkillObject.SetActive(true);
			shieldSkillObject.SetActive(false);
			slowMoSkillObject.SetActive(false);
			yield return new WaitForSeconds(0.2f);

			// Shield
			if (playerAbilities.isShieldAvailable)
			{
				healSkillObject.SetActive(false);
				shieldSkillObject.SetActive(true);
				slowMoSkillObject.SetActive(false);
				yield return new WaitForSeconds(0.2f);
			}

			// Slow-Mo
			if (playerAbilities.isSlowMoAvailable)
			{
				healSkillObject.SetActive(false);
				shieldSkillObject.SetActive(false);
				slowMoSkillObject.SetActive(true);
				yield return new WaitForSeconds(0.2f);
			}
		}
	}

    private void HidePickupObject()
	{
		isPickedUp = true;

		// Немедленно останавливаем корутину переключения объектов
		if (cycleCoroutine != null)
		{
			StopCoroutine(cycleCoroutine);
			cycleCoroutine = null;
		}

		// Отключаем все дочерние объекты
		foreach (Transform child in transform)
		{
			if (child != null)
			{
				child.gameObject.SetActive(false);
			}
		}

		// Отключаем коллайдер объекта, чтобы предотвратить повторное взаимодействие
		var collider = GetComponent<Collider2D>();
		if (collider != null)
		{
			collider.enabled = false;
		}
	}

    private IEnumerator DestroyAfterSound()
    {
        // Ждём завершения звука
        yield return new WaitWhile(() => audioSource.isPlaying);

        // Удаляем объект
        Destroy(gameObject);
    }
}
