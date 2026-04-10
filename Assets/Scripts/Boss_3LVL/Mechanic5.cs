using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic5 : MonoBehaviour, IBossMechanic
{
    public GameObject weakSpotPrefab;        // Префаб слабого места
    public GameObject orbitingObjectPrefab;  // Префаб объектов вокруг босса
    public float initialOrbitRadius = 2f;    // Начальный радиус (для мигания)
    public float minOrbitRadius = 1f;        // Минимальный радиус для расширения
    public float maxOrbitRadius = 6f;        // Максимальный радиус для расширения
    public float orbitingRotationSpeed = 50f;// Скорость вращения объектов
    public float bossMoveUpSpeed = 1.5f;     // Скорость подъёма босса к орбите
    public float bossRotationSpeed = 30f;    // Скорость кругового движения босса

    private GameObject weakSpot;             // Слабое место
    private List<GameObject> orbitingObjects = new List<GameObject>();
    private bool weakSpotDestroyed = false;
    private Vector3 centerPosition;
    private Vector3 bossOrbitPosition;
    private float bossAngle = 0f;
	
	[Header("Audio Settings")]
	public AudioSource contractionSound; // Звук при сужении
	public AudioSource expansionSound;   // Звук при увеличении

    private void Start()
    {
        centerPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
        centerPosition.z = 0;
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 5: Начало последовательности");
		weakSpotDestroyed = false;

        // Шаг 1: Перемещение босса в центр
        yield return MoveToCenter();

        // Шаг 2: Появление 12 объектов и их мигание
        CreateOrbitingObjects(12);
        yield return FlashOrbitingObjects(2);

        // Шаг 3: Переход к плавному расширению и сужению радиуса
        StartCoroutine(ExpandAndContractOrbitRadius());

        // Шаг 4: Перемещение босса вверх к орбите и начало кругового движения
        yield return MoveBossToOrbit();

        // Шаг 5: Появление слабого места
        yield return ActivateWeakSpot();

        // Ждём разрушения слабого места
        while (!weakSpotDestroyed)
        {
            yield return null;
        }
		
        // Шаг 8: Возвращаем босса в центр и завершаем механику
        yield return ReturnToCenter();

        Debug.Log("Механика 5 завершена");
    }

    private IEnumerator MoveToCenter()
    {
        while (Vector3.Distance(transform.position, centerPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPosition, Time.deltaTime * 2f);
            yield return null;
        }
    }

    private void CreateOrbitingObjects(int objectCount)
    {
        for (int i = 0; i < objectCount; i++)
        {
            float angle = i * Mathf.PI * 2f / objectCount;
            Vector3 position = centerPosition + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * initialOrbitRadius;
            GameObject orbitingObject = Instantiate(orbitingObjectPrefab, position, Quaternion.identity);
            orbitingObjects.Add(orbitingObject);
        }
    }

    private IEnumerator FlashOrbitingObjects(int flashCount)
	{
		// Мигание объектов
		for (int i = 0; i < flashCount; i++)
		{
			foreach (var obj in orbitingObjects)
			{
				if (obj != null)
				{
					var renderer = obj.GetComponent<SpriteRenderer>();
					if (renderer != null) renderer.enabled = false;
				}
			}
			yield return new WaitForSeconds(0.3f);

			foreach (var obj in orbitingObjects)
			{
				if (obj != null)
				{
					var renderer = obj.GetComponent<SpriteRenderer>();
					if (renderer != null) renderer.enabled = true;
				}
			}
			yield return new WaitForSeconds(0.3f);
		}

		// После мигания запускаем плавное расширение радиуса объектов
		StartCoroutine(ExpandAndContractOrbitRadius());
	}


	private IEnumerator ExpandAndContractOrbitRadius()
	{
		float currentRadius = initialOrbitRadius; // Начальный радиус
		float time = 0f;
		bool isExpanding = true; // Флаг для отслеживания текущей фазы

		while (!weakSpotDestroyed)
		{
			// Определяем целевой радиус
			float targetRadius = Mathf.Lerp(minOrbitRadius, maxOrbitRadius, (Mathf.Sin(time) + 1) / 2);

			// Проверяем, в какой фазе находится процесс (расширение или сужение)
			if (targetRadius > currentRadius && !isExpanding)
			{
				// Если переходим к расширению
				isExpanding = true;
				if (expansionSound != null)
				{
					expansionSound.Play();
				}
			}
			else if (targetRadius < currentRadius && isExpanding)
			{
				// Если переходим к сужению
				isExpanding = false;
				if (contractionSound != null)
				{
					contractionSound.Play();
				}
			}

			// Плавное изменение радиуса
			currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * 2f);

			// Обновляем позиции объектов с вращением
			UpdateOrbitingObjectsPositions(currentRadius, time);

			time += Time.deltaTime * 2f; // Управляем скоростью изменения радиуса и вращения
			yield return null;
		}
	}

	private void UpdateOrbitingObjectsPositions(float radius, float rotationTime)
	{
		for (int i = 0; i < orbitingObjects.Count; i++)
		{
			if (orbitingObjects[i] == null) continue;

			// Вычисляем угол для каждого объекта с учётом rotationTime для плавного вращения
			float angle = i * Mathf.PI * 2f / orbitingObjects.Count;
			
			// Вычисляем смещение от центра
			Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
			orbitingObjects[i].transform.position = centerPosition + offset;
		}
	}

    private IEnumerator MoveBossToOrbit()
	{
		bossOrbitPosition = centerPosition + new Vector3(0, 3f, 0); // Целевая точка для подъёма

		// Поднимаем босса до Y = 3 плавно
		while (Vector3.Distance(transform.position, bossOrbitPosition) > 0.1f)
		{
			transform.position = Vector3.MoveTowards(transform.position, bossOrbitPosition, bossMoveUpSpeed * Time.deltaTime);
			yield return null;
		}

		// Устанавливаем начальный угол для плавного перехода в круговое движение
		bossAngle = Mathf.Atan2(transform.position.y - centerPosition.y, transform.position.x - centerPosition.x);

		// Начинаем движение по орбите с текущей позиции
		StartCoroutine(OrbitMovement());
	}

	private IEnumerator OrbitMovement()
	{
		while (!weakSpotDestroyed)
		{
			bossAngle += bossRotationSpeed * Time.deltaTime; // Увеличиваем угол для кругового движения
			float x = centerPosition.x + Mathf.Cos(bossAngle) * 7f; // Радиус X = 7
			float y = centerPosition.y + Mathf.Sin(bossAngle) * 3f; // Радиус Y = 3
			transform.position = new Vector3(x, y, transform.position.z);

			yield return null;
		}
	}
	
    public void HandleWeakSpotHit()
    {
        weakSpotDestroyed = true;
    }
	
    public void EndMechanic()
    {
        StartCoroutine(ReturnToCenter());
    }
	
	public void Cleanup()
	{
		if (weakSpot != null)
		{
			Destroy(weakSpot);
		}

		foreach (var obj in orbitingObjects)
		{
			if (obj != null)
			{
				Destroy(obj);
			}
		}

		orbitingObjects.Clear();
		Debug.Log("Все объекты механики 5 успешно удалены.");
	}
	
	private IEnumerator ActivateWeakSpot()
    {
        // Создаём слабое место и связываем его с BossController
        weakSpot = Instantiate(weakSpotPrefab, centerPosition, Quaternion.identity);
        WeakSpotLevel3 weakSpotScript = weakSpot.GetComponent<WeakSpotLevel3>();
        if (weakSpotScript != null)
        {
            weakSpotScript.Activate(GetComponent<BossController>());
            weakSpotDestroyed = false;
        }

        yield return null;  // Слабое место остаётся активным, пока не будет уничтожено
    }
	
    private IEnumerator ReturnToCenter()
    {
        while (Vector3.Distance(transform.position, centerPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPosition, Time.deltaTime * 2f);
            yield return null;
        }

        DestroyWeakSpotAndOrbitingObjects();
        Debug.Log("Механика завершена, босс вернулся в центр.");
    }
	
    private void DestroyWeakSpotAndOrbitingObjects()
    {
        if (weakSpot != null) Destroy(weakSpot);
        foreach (var obj in orbitingObjects) Destroy(obj);
        orbitingObjects.Clear();
    }
}
