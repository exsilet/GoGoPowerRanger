using System.Collections;
using UnityEngine;

public class Mechanic5_3 : MonoBehaviour, IBossMechanic
{
    public Transform boss;                     // Объект босса
    public GameObject followerPrefab;          // Префаб преследующего объекта
    public GameObject targetPrefab;            // Префаб цели
    public Transform player;                   // Ссылка на игрока
    public int repetitions = 3;                // Количество повторений механики
    public float bossMoveDuration = 2f;        // Время перемещения босса
    public float followerSpeed = 2f;           // Скорость преследующего объекта

    private int currentRepetition = 0;
    private GameObject currentFollower;
    private GameObject currentTarget;
	
	public AudioSource hitSound; // Звук попадания

    public IEnumerator Execute()
    {
        // Перемещаем босса в верхний правый угол
        yield return StartCoroutine(MoveBossToUpperRight());

        // Выполняем механику нужное количество раз
        for (int i = 0; i < repetitions; i++)
        {
            currentRepetition = i + 1;

            // Создаем преследующий объект и цель
            CreateFollower();
            yield return new WaitForSeconds(0.5f); // Пауза перед созданием цели
            CreateTarget();

            // Ждем, пока преследующий объект попадет в цель
            yield return new WaitUntil(() => currentFollower == null && currentTarget == null);
        }

        Debug.Log("Механика 5_3 завершена.");
    }

    private IEnumerator MoveBossToUpperRight()
    {
        Camera cam = Camera.main;
        Vector2 upperRightPosition = new Vector2(cam.orthographicSize * cam.aspect - 1, cam.orthographicSize - 1);
        Vector2 startPosition = boss.position;
        float elapsedTime = 0f;

        while (elapsedTime < bossMoveDuration)
        {
            boss.position = Vector2.Lerp(startPosition, upperRightPosition, elapsedTime / bossMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        boss.position = upperRightPosition;
    }

    private void CreateFollower()
    {
        if (currentFollower != null) Destroy(currentFollower);

        currentFollower = Instantiate(followerPrefab, boss.position, Quaternion.identity);
        StartCoroutine(FollowPlayer(currentFollower));
    }

    private IEnumerator FollowPlayer(GameObject follower)
	{
		while (follower != null && player != null)
		{
			Vector2 direction = (player.position - follower.transform.position).normalized;
			follower.transform.position += (Vector3)(direction * followerSpeed * Time.deltaTime);

			// Проверка попадания в цель
			if (currentTarget != null && Vector2.Distance(follower.transform.position, currentTarget.transform.position) < 0.5f)
			{
				// Воспроизводим звук, если он задан
				if (hitSound != null)
				{
					hitSound.Play();
				}

				Destroy(currentFollower);
				Destroy(currentTarget);
				Debug.Log($"Попадание преследующего объекта в цель. Повтор {currentRepetition} завершен.");
			}

			yield return null;
		}
	}

    private void CreateTarget()
    {
        if (currentTarget != null) Destroy(currentTarget);

        Camera cam = Camera.main;
        float xMin = -cam.orthographicSize * cam.aspect + 1;
        float xMax = cam.orthographicSize * cam.aspect - 1;
        float yMin = -cam.orthographicSize + 1;
        float yMax = cam.orthographicSize - 1;

        Vector2 targetPosition = new Vector2(
            Random.Range(xMin + 1, xMax - 1),
            Random.Range(yMin + 1, yMax - 1)
        );

        currentTarget = Instantiate(targetPrefab, targetPosition, Quaternion.identity);

        // Мигаем целью два раза
        StartCoroutine(BlinkTarget(currentTarget, 2));
    }

    private IEnumerator BlinkTarget(GameObject target, int blinks)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        for (int i = 0; i < blinks; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.2f);
            sr.enabled = true;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
