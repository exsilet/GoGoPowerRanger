using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic5_2 : MonoBehaviour, IBossMechanic
{
    public Transform boss;                    // Объект босса
    public GameObject spikePrefab;            // Префаб спицы (длинная тонкая линия)
    public int spikeCount = 10;               // Количество спиц в "заборе"
    public float bossMoveDuration = 2f;       // Время перемещения босса в угол
    public float pauseDuration = 0.5f;        // Время мигания после завершения перемещения
    public int repeats = 3;                   // Количество повторов
    public float spikeSpawnInterval = 0.5f;   // Минимальное расстояние между спицами
    public float arcMoveSpeed = 1f;           // Скорость движения по дуге
    public float trajectoryOffsetRange = 0.3f; // Диапазон смещения траектории босса

    private Vector2 leftBottomCorner;          // Левый нижний угол экрана
    private Vector2 rightBottomCorner;         // Правый нижний угол экрана
    private Vector2 lastSpikePosition;         // Позиция последней созданной спицы
    private List<GameObject> activeSpikes = new List<GameObject>(); // Список активных спиц
    private Vector2 currentCorner;             // Текущий угол, откуда начинается движение
	public AudioSource spikeClearSound; // Звук очистки спиц

    private void Start()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        leftBottomCorner = new Vector2(-halfWidth + 1, -halfHeight + 1);
        rightBottomCorner = new Vector2(halfWidth - 1, -halfHeight + 1);

        currentCorner = (Random.value > 0.5f) ? leftBottomCorner : rightBottomCorner;
        Debug.Log("Начальный угол выбран: " + currentCorner);
    }

    public IEnumerator Execute()
    {
        Debug.Log("Перемещаемся в начальный угол: " + currentCorner);
        yield return StartCoroutine(MoveBossToPosition(currentCorner));

        for (int i = 0; i < repeats; i++)
        {
            Debug.Log("Запуск движения по дуге из угла: " + currentCorner);
            yield return StartCoroutine(MoveInArc(currentCorner));

            ClearSpikes();
			// Воспроизводим звук после очистки спиц
			if (spikeClearSound != null)
			{
				spikeClearSound.Play();
			}
            yield return StartCoroutine(BlinkBoss());

            currentCorner = (currentCorner == leftBottomCorner) ? rightBottomCorner : leftBottomCorner;
            Debug.Log("Следующий угол для движения: " + currentCorner);
        }
    }

    private IEnumerator MoveBossToPosition(Vector2 targetPosition)
    {
        Debug.Log("Начинаем плавное перемещение в угол: " + targetPosition);
        Vector2 startPosition = boss.position;
        float elapsedTime = 0f;

        while (elapsedTime < bossMoveDuration)
        {
            boss.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / bossMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        boss.position = targetPosition;
        Debug.Log("Завершено перемещение. Текущая позиция босса: " + boss.position);
    }

    private IEnumerator MoveInArc(Vector2 startCorner)
	{
		Debug.Log("Начинаем движение по дуге из угла: " + startCorner + ", текущая позиция босса: " + boss.position);

		// Определяем направление движения по дуге
		float angleDirection = (startCorner == leftBottomCorner) ? 1f : -1f;
		float radius = Vector2.Distance(leftBottomCorner, rightBottomCorner) / 2;
		Vector2 centerPoint = (leftBottomCorner + rightBottomCorner) / 2;
		centerPoint.x += Random.Range(-trajectoryOffsetRange, trajectoryOffsetRange);

		// Вместо установки угла в 0 или PI, начнем с угла, соответствующего текущей позиции босса
		float angle = Mathf.Atan2(boss.position.y - centerPoint.y, boss.position.x - centerPoint.x);

		while ((angle < Mathf.PI && angleDirection == 1) || (angle > 0f && angleDirection == -1))
		{
			float x = centerPoint.x + Mathf.Cos(angle) * radius;
			float y = centerPoint.y + Mathf.Sin(angle) * radius;

			boss.position = new Vector2(x, y);

			if (Vector2.Distance(boss.position, lastSpikePosition) >= spikeSpawnInterval)
			{
				CreateSpike(boss.position);
				lastSpikePosition = boss.position;
			}

			angle += angleDirection * arcMoveSpeed * Time.deltaTime;
			yield return null;
		}
		Debug.Log("Движение по дуге завершено. Текущая позиция босса: " + boss.position);
	}
	
    private void CreateSpike(Vector2 position)
    {
        float spacing = 2f / spikeCount;

        for (int i = 0; i < spikeCount; i++)
        {
            if (i == spikeCount / 2) continue;

            GameObject spike = Instantiate(spikePrefab, new Vector2(position.x, -spacing * i), Quaternion.identity);
            spike.transform.localScale = new Vector3(0.1f, Camera.main.orthographicSize * 2, 1f);
            activeSpikes.Add(spike);
        }
    }

    private void ClearSpikes()
    {
        foreach (GameObject spike in activeSpikes)
        {
            if (spike != null)
            {
                Destroy(spike);
            }
        }
        activeSpikes.Clear();
        Debug.Log("Спицы очищены");
    }

    private IEnumerator BlinkBoss()
    {
        SpriteRenderer sr = boss.GetComponent<SpriteRenderer>();
        for (int i = 0; i < 2; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(pauseDuration / 2);
            sr.enabled = true;
            yield return new WaitForSeconds(pauseDuration / 2);
        }
        Debug.Log("Мигание босса завершено");
    }
}
