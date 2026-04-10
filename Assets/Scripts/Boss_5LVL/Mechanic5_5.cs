using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic5_5 : MonoBehaviour, IBossMechanic
{
    public Transform boss;
    public GameObject thimblePrefab;
    public GameObject soughtPrefab;
    public GameObject weakSpotPrefab;
    public Transform player;
    public int repetitions = 3;

    private List<GameObject> thimbles = new List<GameObject>();
    private List<Vector2> thimblePositions = new List<Vector2>();
    private GameObject soughtObject;
    private int correctThimbleIndex;
    private BossController_3 bossController;
    private int correctChoices;
    private bool mechanicActive;
    private bool playerMadeChoice;
	
	public AudioSource audioSource;
	public AudioClip thimbleAppearClip;      // Звук появления Thimble
	public AudioClip shuffleLoopClip;       // Звук перемешивания (в лупе)
	public AudioClip correctChoiceClip;     // Звук при правильном выборе
	public AudioClip wrongChoiceClip;       // Звук при неправильном выборе

    private void Start()
    {
        bossController = GetComponentInParent<BossController_3>();
    }

	public IEnumerator Execute()
	{
		mechanicActive = true;
		correctChoices = 0;

		yield return StartCoroutine(MoveBossToCenter());

		while (correctChoices < repetitions && mechanicActive)
		{
			playerMadeChoice = false;

			CreateThimbles();
			yield return StartCoroutine(CreateAndHideSoughtObject());

			yield return StartCoroutine(ShuffleThimbles());

			EnableThimbleInteraction(true);

			yield return new WaitUntil(() => playerMadeChoice);

			EnableThimbleInteraction(false);

			if (!mechanicActive)
			{
				EndMechanic();
				yield break;
			}

			if (correctChoices >= repetitions)
			{
				if (soughtObject != null)
				{
					soughtObject.transform.SetParent(null); // Освобождаем искомый объект из-под наперстка
				}

				// Добавляем эффект мигания
				yield return StartCoroutine(BlinkThimbles(2, 0.3f));

				DestroyThimbles(); // Удаляем все Thimble после мигания
				break;
			}
			else
			{
				DestroyThimbles(); // Удаляем Thimble при каждой попытке, если меньше трех правильных ответов
			}

			yield return new WaitForSeconds(1f);
		}

		// Появление слабого места после трёх успешных раундов
		if (mechanicActive && correctChoices >= repetitions)
		{
			TransformSoughtObjectToWeakSpot();
			yield return new WaitUntil(() => mechanicActive == false);
		}
		else
		{
			Debug.LogWarning("Механика была остановлена до появления слабого места.");
		}
	}
	
    private IEnumerator MoveBossToCenter()
	{
		// Получаем размеры босса
		Renderer bossRenderer = boss.GetComponent<Renderer>();
		float bossHalfWidth = bossRenderer.bounds.extents.x;
		float bossHalfHeight = bossRenderer.bounds.extents.y;

		// Рассчитываем границы видимого экрана
		float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
		float screenHalfHeight = Camera.main.orthographicSize;

		// Целевая позиция с учётом границ экрана
		Vector2 targetPosition = new Vector2(
			Mathf.Clamp(0, -screenHalfWidth + bossHalfWidth, screenHalfWidth - bossHalfWidth),
			Mathf.Clamp(Camera.main.orthographicSize - bossHalfHeight, -screenHalfHeight + bossHalfHeight, screenHalfHeight - bossHalfHeight)
		);

		// Линейное перемещение босса к целевой позиции
		Vector2 startPosition = boss.position;
		float elapsedTime = 0f;
		float moveDuration = 2f;

		while (elapsedTime < moveDuration)
		{
			boss.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Устанавливаем точную финальную позицию
		boss.position = targetPosition;
	}

    private void CreateThimbles()
	{
		thimbles.Clear();
		thimblePositions.Clear();

		float spacing = 3f;
		int centralThimbleIndex = 1; // Центрируем искомый объект в начале игры

		for (int i = -1; i <= 1; i++)
		{
			Vector2 position = new Vector2(i * spacing, -1);
			thimblePositions.Add(position);

			GameObject thimble = Instantiate(thimblePrefab, position, Quaternion.identity);
			Thimble thimbleScript = thimble.AddComponent<Thimble>();
			
			bool isCorrect = (i + 1) == centralThimbleIndex; // Ставим объект под центральный наперсток
			thimbleScript.Initialize(this, isCorrect);

			if (isCorrect)
			{
				soughtObject = Instantiate(soughtPrefab, thimble.transform.position, Quaternion.identity);
				soughtObject.transform.SetParent(thimble.transform);
				soughtObject.SetActive(false); // Прячем искомый объект
			}

			thimbles.Add(thimble);
		}
		
		// Проигрываем звук появления
		if (audioSource != null && thimbleAppearClip != null)
		{
			audioSource.PlayOneShot(thimbleAppearClip);
		}

		EnableThimbleInteraction(false);
	}

    private IEnumerator CreateAndHideSoughtObject()
	{
		correctThimbleIndex = 1;

		// Устанавливаем начальную позицию ниже экрана
		Vector3 startPosition = new Vector3(thimbles[correctThimbleIndex].transform.position.x, -Camera.main.orthographicSize - 1, 0);
		soughtObject = Instantiate(soughtPrefab, startPosition, Quaternion.identity);

		SpriteRenderer sr = soughtObject.GetComponent<SpriteRenderer>();
		sr.sortingLayerName = "BehindThimble";

		// Пауза на 1 секунду перед началом движения
		yield return new WaitForSeconds(1f);

		// Целевая позиция - под наперстком
		Vector3 targetPosition = thimbles[correctThimbleIndex].transform.position;

		// Перемещение снизу вверх
		float moveDuration = 1f;
		float elapsedTime = 0f;

		while (elapsedTime < moveDuration)
		{
			soughtObject.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Закрепляем объект под наперстком
		soughtObject.transform.SetParent(thimbles[correctThimbleIndex].transform);
		soughtObject.SetActive(false);
	}

    private IEnumerator ShuffleThimbles()
	{
		soughtObject.SetActive(true);

		int shuffleCount = 5;
		float shuffleDuration = 0.5f;
		
		// Включаем луп звука перемешивания
		if (audioSource != null && shuffleLoopClip != null)
		{
			audioSource.clip = shuffleLoopClip;
			audioSource.loop = true;
			audioSource.Play();
		}

		for (int i = 0; i < shuffleCount; i++)
		{
			int indexA = Random.Range(0, thimbles.Count);
			int indexB = (indexA + 1) % thimbles.Count;

			yield return StartCoroutine(SwapThimbles(indexA, indexB, shuffleDuration));
			shuffleDuration *= 0.9f;
		}
		
		    // Останавливаем звук перемешивания
		if (audioSource != null && audioSource.clip == shuffleLoopClip)
		{
			audioSource.loop = false;
			audioSource.Stop();
		}
		Debug.Log("Перемешивание завершено. Правильный наперсток находится в позиции: " + soughtObject.transform.position);
	}

	private IEnumerator SwapThimbles(int indexA, int indexB, float duration)
	{
		Vector2 startPosA = thimbles[indexA].transform.position;
		Vector2 startPosB = thimbles[indexB].transform.position;

		float elapsed = 0f;

		while (elapsed < duration)
		{
			thimbles[indexA].transform.position = Vector2.Lerp(startPosA, startPosB, elapsed / duration);
			thimbles[indexB].transform.position = Vector2.Lerp(startPosB, startPosA, elapsed / duration);

			elapsed += Time.deltaTime;
			yield return null;
		}

		// Устанавливаем позиции окончательно
		thimbles[indexA].transform.position = startPosB;
		thimbles[indexB].transform.position = startPosA;

		// Перемещаем правильный наперсток, если он участвовал в перемешивании, но не меняем его объект
		if (thimbles[indexA] == thimbles[correctThimbleIndex])
		{
			correctThimbleIndex = indexB;
		}
		else if (thimbles[indexB] == thimbles[correctThimbleIndex])
		{
			correctThimbleIndex = indexA;
		}
	}

	public void SetPlayerChoice(bool isCorrect)
	{
		if (isCorrect)
		{
			Debug.Log("Правильный выбор!");
			correctChoices++;

			// Проигрываем звук правильного ответа
			if (audioSource != null && correctChoiceClip != null)
			{
				audioSource.PlayOneShot(correctChoiceClip);
			}
		}
		else
		{
			Debug.Log("Неправильный выбор. Механика завершена.");
			mechanicActive = false;

			// Проигрываем звук неправильного ответа
			if (audioSource != null && wrongChoiceClip != null)
			{
				audioSource.PlayOneShot(wrongChoiceClip);
			}
		}

		playerMadeChoice = true;
		EnableThimbleInteraction(false);
	}

    private void EnableThimbleInteraction(bool enable)
    {
        foreach (GameObject thimble in thimbles)
        {
            if (thimble != null)
            {
                var collider = thimble.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = enable;
                }
            }
        }
    }

	private void EndMechanic()
	{
		DestroyThimbles();
		if (correctChoices < repetitions && soughtObject != null) 
		{
			Destroy(soughtObject); // Удаляем искомый объект только если механика завершена из-за ошибки
		}
		StopAllCoroutines();
	}

    private void DestroyThimbles()
    {
        foreach (GameObject thimble in thimbles)
        {
            if (thimble != null) Destroy(thimble);
        }
        thimbles.Clear();
        thimblePositions.Clear();
    }

	private void TransformSoughtObjectToWeakSpot()
	{
		if (soughtObject == null) return;

		Vector2 weakSpotPosition = soughtObject.transform.position;

		Destroy(soughtObject); // Удаляем искомый объект и создаём слабое место в его позиции
		GameObject weakSpot = Instantiate(weakSpotPrefab, weakSpotPosition, Quaternion.identity);
		WeakSpot_2 weakSpotScript = weakSpot.GetComponent<WeakSpot_2>();

		weakSpotScript.OnDestroyed += () =>
		{
			bossController.TakeDamage(1);
			mechanicActive = false;
		};

		Debug.Log("Слабое место создано в позиции: " + weakSpotPosition);
	}
	
	private IEnumerator BlinkThimbles(int blinkCount, float blinkDuration)
	{
		for (int i = 0; i < blinkCount; i++)
		{
			foreach (var thimble in thimbles)
			{
				if (thimble != null)
				{
					var renderer = thimble.GetComponent<SpriteRenderer>();
					if (renderer != null)
					{
						renderer.enabled = false; // Скрыть
					}
				}
			}
			yield return new WaitForSeconds(blinkDuration);

			foreach (var thimble in thimbles)
			{
				if (thimble != null)
				{
					var renderer = thimble.GetComponent<SpriteRenderer>();
					if (renderer != null)
					{
						renderer.enabled = true; // Показать
					}
				}
			}
			yield return new WaitForSeconds(blinkDuration);
		}
	}
}
