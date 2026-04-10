using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic4_5 : MonoBehaviour, IBossMechanic
{
    public GameObject weakSpotPrefab;
    public GameObject[] colorObjects;
    public float moveSpeed = 5f;
    public float flashInterval = 1f;
    public int maxRounds = 3;

    private List<string> colorSequence;
    private List<string> playerSequence;
    private Vector3[] spawnPositions;
    private int roundCount;
    private SpriteRenderer spriteRenderer;
    private bool isCollecting;
    private bool isRoundComplete;
    private GameObject weakSpot;
	private bool isWeakSpotDestroyed;
	
	[Header("Audio Settings")]
	public AudioSource colorFlashSound; // Звук при смене цвета
	[Header("Audio Settings - 2")]
	public AudioSource audioSource;         // Источник звука
	public AudioClip correctChoiceClip;     // Звук правильного выбора
	public AudioClip incorrectChoiceClip;   // Звук неправильного выбора

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPositions = new Vector3[]
        {
            new Vector3(-Camera.main.orthographicSize * Camera.main.aspect + 1, -Camera.main.orthographicSize + 1, 0),
            new Vector3(-Camera.main.orthographicSize * Camera.main.aspect / 3, -Camera.main.orthographicSize + 1, 0),
            new Vector3(Camera.main.orthographicSize * Camera.main.aspect / 3, -Camera.main.orthographicSize + 1, 0),
            new Vector3(Camera.main.orthographicSize * Camera.main.aspect - 1, -Camera.main.orthographicSize + 1, 0)
        };
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 5_1 запущена.");
        roundCount = 0;
		isWeakSpotDestroyed = false;
		
		// Делаем дочерний объект невидимым
		Transform childObject = transform.Find("Eye"); // Замените "ChildObjectName" на имя дочернего объекта
		if (childObject != null)
		{
			Renderer childRenderer = childObject.GetComponent<Renderer>();
			if (childRenderer != null)
			{
				childRenderer.enabled = false; // Отключаем отображение
			}
		}

        yield return StartCoroutine(MoveToCorner());
        yield return new WaitForSeconds(1f);

        while (roundCount < maxRounds)
        {
            colorSequence = GenerateRandomColorSequence();
            yield return StartCoroutine(FlashSequence(colorSequence));

            yield return new WaitForSeconds(1f);

            SpawnColorObjects();

            yield return StartCoroutine(FlashColorObjects());

            isCollecting = true;
            isRoundComplete = false;
            playerSequence = new List<string>();

            // Ожидаем завершения раунда (либо правильная последовательность, либо ошибка)
            while (isCollecting && !isRoundComplete)
            {
                yield return null;
            }

            if (!isRoundComplete)
            {
                Debug.Log("Механика завершена из-за ошибки игрока.");
                RemoveAllColorObjects();
				
				if (childObject != null)
				{
					Renderer childRenderer = childObject.GetComponent<Renderer>();
					if (childRenderer != null)
					{
						childRenderer.enabled = true; // Отключаем отображение
					}
				}
                yield break;
            }

            Debug.Log("Раунд завершён успешно!");
            roundCount++;
            RemoveAllColorObjects();

            yield return new WaitForSeconds(1f);

            if (roundCount >= maxRounds)
            {
                Debug.Log("Все 3 раунда завершены успешно. Появляется слабое место.");
                SpawnWeakSpot();
				
				if (childObject != null)
				{
					Renderer childRenderer = childObject.GetComponent<Renderer>();
					if (childRenderer != null)
					{
						childRenderer.enabled = true; // Отключаем отображение
					}
				}
				
				// Ожидаем, пока слабое место не будет разрушено
                while (!isWeakSpotDestroyed)
                {
                    yield return null;
                }

                Debug.Log("Слабое место уничтожено, механика завершена.");
                yield break;
            }
        }

        Debug.Log("Механика 5_1 завершена.");
		if (childObject != null)
		{
			Renderer childRenderer = childObject.GetComponent<Renderer>();
			if (childRenderer != null)
			{
				childRenderer.enabled = true; // Отключаем отображение
			}
		}
    }

    private IEnumerator MoveToCorner()
	{
		// Получаем размеры объекта через Renderer (для учёта границ)
		Renderer renderer = GetComponent<Renderer>();
		float objectWidth = renderer.bounds.extents.x; // Половина ширины объекта
		float objectHeight = renderer.bounds.extents.y; // Половина высоты объекта

		// Рассчитываем границы видимого экрана
		float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
		float screenHalfHeight = Camera.main.orthographicSize;

		// Устанавливаем целевую позицию с учётом границ экрана и размеров объекта
		Vector3 targetPosition = new Vector3(
			Mathf.Clamp(screenHalfWidth - objectWidth, -screenHalfWidth + objectWidth, screenHalfWidth - objectWidth),
			Mathf.Clamp(screenHalfHeight - objectHeight, -screenHalfHeight + objectHeight, screenHalfHeight - objectHeight),
			0
		);

		// Двигаем объект к целевой позиции
		while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
			yield return null;
		}
	}

    private List<string> GenerateRandomColorSequence()
    {
        string[] colors = { "Red", "Blue", "Yellow", "Black" };
        List<string> sequence = new List<string>();

        while (sequence.Count < 4)
        {
            string color = colors[Random.Range(0, colors.Length)];
            if (!sequence.Contains(color))
            {
                sequence.Add(color);
            }
        }

        return sequence;
    }

    private IEnumerator FlashSequence(List<string> sequence)
    {
        foreach (string color in sequence)
        {
            SetColor(color);
			
			// Воспроизводим звук, если он задан
			if (colorFlashSound != null)
			{
				colorFlashSound.Play();
			}
			
            yield return new WaitForSeconds(flashInterval);
        }

        spriteRenderer.color = Color.white;
    }

    private void SetColor(string color)
    {
        switch (color)
        {
            case "Red": spriteRenderer.color = Color.red; break;
            case "Blue": spriteRenderer.color = Color.blue; break;
            case "Yellow": spriteRenderer.color = Color.yellow; break;
            case "Black": spriteRenderer.color = Color.black; break;
        }
    }

	private List<GameObject> spawnedColorObjects = new List<GameObject>(); // Список для хранения созданных объектов

	private void SpawnColorObjects()
	{
		string[] colors = { "Red", "Blue", "Yellow", "Black" };
		for (int i = 0; i < colorObjects.Length; i++)
		{
			GameObject obj = Instantiate(colorObjects[i], spawnPositions[i], Quaternion.identity);
			obj.SetActive(false); // Сначала делаем объекты неактивными
			SetObjectColor(obj, colors[i]);
			spawnedColorObjects.Add(obj); // Добавляем объект в список
		}

		StartCoroutine(FlashColorObjects()); // Запускаем мигание объектов перед активацией
	}
	
	

	private IEnumerator FlashColorObjects()
	{
		// Отключаем коллайдеры перед миганием, чтобы игрок не мог взаимодействовать
		foreach (GameObject obj in spawnedColorObjects)
		{
			Collider2D collider = obj.GetComponent<Collider2D>();
			if (collider != null)
			{
				collider.enabled = false;
			}
		}

		// Дважды мигаем созданными объектами
		for (int i = 0; i < 2; i++)
		{
			foreach (GameObject obj in spawnedColorObjects)
			{
				obj.SetActive(false);
			}
			yield return new WaitForSeconds(0.3f);

			foreach (GameObject obj in spawnedColorObjects)
			{
				obj.SetActive(true);
			}
			yield return new WaitForSeconds(0.3f);
		}

		// После мигания включаем коллайдеры, чтобы игрок мог взаимодействовать с объектами
		foreach (GameObject obj in spawnedColorObjects)
		{
			Collider2D collider = obj.GetComponent<Collider2D>();
			if (collider != null)
			{
				collider.enabled = true;
			}
		}
	}

    private void SetObjectColor(GameObject obj, string color)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (color)
            {
                case "Red": sr.color = Color.red; break;
                case "Blue": sr.color = Color.blue; break;
                case "Yellow": sr.color = Color.yellow; break;
                case "Black": sr.color = Color.black; break;
            }
        }
    }

    public void OnColorObjectTouched(string colorName)
	{
		if (!isCollecting) return;

		playerSequence.Add(colorName);
		Debug.Log($"Игрок выбрал цвет: {colorName}. Проверка с эталоном: {colorSequence[playerSequence.Count - 1]}");

		if (playerSequence[playerSequence.Count - 1] != colorSequence[playerSequence.Count - 1])
		{
			// Проигрываем звук неправильного выбора
			if (audioSource != null && incorrectChoiceClip != null)
			{
				audioSource.PlayOneShot(incorrectChoiceClip);
			}

			Debug.Log("Ошибка в последовательности! Удаляем все цветные объекты.");
			RemoveAllColorObjects();
			isCollecting = false;
			isRoundComplete = false;
			return;
		}

		// Проигрываем звук правильного выбора
		if (audioSource != null && correctChoiceClip != null)
		{
			audioSource.PlayOneShot(correctChoiceClip);
		}

		if (playerSequence.Count == colorSequence.Count)
		{
			Debug.Log("Последовательность собрана правильно!");
			isCollecting = false;
			isRoundComplete = true;
		}
	}

    private void RemoveAllColorObjects()
	{
		foreach (GameObject obj in spawnedColorObjects)
		{
			Destroy(obj);
		}
		spawnedColorObjects.Clear(); // Очищаем список для следующего раунда
	}

	private void SpawnWeakSpot()
	{
		if (weakSpot == null) // Проверка, чтобы избежать создания нескольких слабых мест
		{
			weakSpot = Instantiate(weakSpotPrefab, new Vector3(0, 0, 0), Quaternion.identity);
			WeakSpot_1 weakSpotScript = weakSpot.GetComponent<WeakSpot_1>();
			if (weakSpotScript != null)
			{
				weakSpotScript.OnDestroyed += OnWeakSpotDestroyed;
			}
		}
	}

	private void OnWeakSpotDestroyed()
	{
		if (isWeakSpotDestroyed) return; // Проверка для предотвращения двойного урона

		Debug.Log("Слабое место уничтожено! Босс получает урон.");
		isWeakSpotDestroyed = true;

		// Находим контроллер босса и наносим урон
		BossController_2 bossController = FindObjectOfType<BossController_2>();
		if (bossController != null)
		{
			bossController.TakeDamage(1);
		}
		
		StopAllCoroutines();
	}
}
