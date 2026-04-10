using System.Collections;
using UnityEngine;

public class Mechanic3_1 : MonoBehaviour, IBossMechanic
{
    public GameObject wallPrefab; // Префаб для стены
    public GameObject gapPrefab; // Префаб для прохода
    public float flashInterval = 0.3f; // Интервал мигания
    public float wallFallSpeed = 5f; // Скорость падения стен
    public float wallDestroyY = -10f; // Y-координата, ниже которой стены удаляются
    public float gapWidth = 2f; // Ширина прохода между стенами
    public float minGapOffset = 0.1f; // Минимальное смещение от центра (доля ширины экрана)
    public float maxGapOffset = 0.9f; // Максимальное смещение от центра (доля ширины экрана)

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 3_1 запущена.");

        // 0. Босс встает на месте и мигает
        yield return StartCoroutine(BossFlash());

        // 1. Босс делается невидимым
        SetVisibility(false);

        // 2. Генерация и падение нескольких наборов стен с проходом
        int fallCount = Random.Range(5, 10); // Количество падений
        for (int i = 0; i < fallCount; i++)
        {
            GenerateWallsWithGap();
            yield return new WaitForSeconds(2.5f); // Интервал между падениями
        }

        // 3. Босс снова становится видимым
        SetVisibility(true);
        Debug.Log("Механика 3_1 завершена.");
    }

    private IEnumerator BossFlash()
    {
        // Босс мигает 2 раза
        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }

        yield return new WaitForSeconds(1f); // Ждет 1 секунду после мигания
    }

    private void GenerateWallsWithGap()
	{
		// Определяем половину ширины экрана
		float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;

		// Определяем случайное положение для центра прохода
		float gapCenterX = Random.Range(-screenHalfWidth + gapWidth / 2, screenHalfWidth - gapWidth / 2);

		// Создаём проход (gap)
		Vector3 gapPos = new Vector3(gapCenterX, Camera.main.orthographicSize + 1, 0);
		GameObject gap = Instantiate(gapPrefab, gapPos, Quaternion.identity);
		gap.transform.localScale = new Vector3(gapWidth, 1, 1);

		// Определяем размеры прохода
		float actualGapWidth = gap.GetComponent<Renderer>().bounds.size.x;

		// Создаём левую стену
		GameObject leftWall = Instantiate(wallPrefab);
		float leftWallWidth = leftWall.GetComponent<Renderer>().bounds.size.x;
		Vector3 leftWallPos = new Vector3(gapPos.x - actualGapWidth / 2 - leftWallWidth / 2, gapPos.y, 0);
		leftWall.transform.position = leftWallPos;

		// Создаём правую стену
		GameObject rightWall = Instantiate(wallPrefab);
		float rightWallWidth = rightWall.GetComponent<Renderer>().bounds.size.x;
		Vector3 rightWallPos = new Vector3(gapPos.x + actualGapWidth / 2 + rightWallWidth / 2, gapPos.y, 0);
		rightWall.transform.position = rightWallPos;

		// Запускаем падение всех объектов
		StartCoroutine(FallAndDestroyWall(leftWall));
		StartCoroutine(FallAndDestroyWall(rightWall));
		StartCoroutine(FallAndDestroyWall(gap));
	}

    private IEnumerator FallAndDestroyWall(GameObject wall)
    {
        while (wall.transform.position.y > wallDestroyY)
        {
            // Перемещаем стену или проход вниз
            wall.transform.position += Vector3.down * wallFallSpeed * Time.deltaTime;
            yield return null;
        }

        // Удаляем стену или проход, когда они выходят за нижнюю границу
        Destroy(wall);
    }
	
	private void SetVisibility(bool isVisible)
	{
		// Скрываем/показываем основной объект
		spriteRenderer.enabled = isVisible;

		// Скрываем/показываем все дочерние объекты
		foreach (Transform child in transform)
		{
			SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
			if (childRenderer != null)
			{
				childRenderer.enabled = isVisible;
			}
		}
	}
}
