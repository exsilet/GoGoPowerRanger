using System.Collections;
using UnityEngine;

public class Mechanic4 : MonoBehaviour, IBossMechanic
{
    [Header("Boss Settings")]
    public float bossMoveSpeed = 3f;
    public Vector3 centerPosition;
    public float bossReturnDelay = 1f;

    [Header("Safe Zone Settings")]
    public GameObject safeZonePrefab;
    public float safeZoneFlashDuration = 0.5f;
    public float safeZoneMoveSpeed = 2f;
    public float safeZoneMovementTime = 5f;

    [Header("Lava Settings")]
    public GameObject lavaPrefab;
    public int lavaSegments = 4;
    public float lavaSpawnDelay = 1f;
    public float lavaDespawnDelay = 1f;

    private SpriteRenderer bossSprite;
    private Collider2D bossCollider;
    private Camera mainCamera;
	
	public AudioSource lavaSound;


    private void Start()
    {
        mainCamera = Camera.main;
        bossSprite = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 4 началась");

        // 1. Босс движется в центр
        yield return MoveBossToCenter();

        // 2. Ждём 1 секунду
        yield return new WaitForSeconds(1f);

        // 3. Прячем босса
        HideBoss();

        // 4. Создаём безопасную зону
        GameObject safeZone = Instantiate(safeZonePrefab);
        Vector3 safeZonePosition = GetRandomPositionWithinCameraBounds(safeZone.GetComponent<SpriteRenderer>().bounds.size);
        safeZone.transform.position = safeZonePosition;

        // 5. Мигаем безопасной зоной
        yield return FlashSafeZone(safeZone);
		yield return new WaitForSeconds(3f);

        // 6. Появление лавы
        GameObject[] lavaSegments = new GameObject[this.lavaSegments];
        yield return SpawnLava(lavaSegments);

        // 7. Движение безопасной зоны
        yield return MoveSafeZone(safeZone);

        // 8. Постепенное удаление лавы
        yield return DespawnLava(lavaSegments);

        // 9. Удаляем безопасную зону
        Destroy(safeZone);

        // 10. Возвращаем босса
        yield return new WaitForSeconds(bossReturnDelay);
        ShowBoss();

        Debug.Log("Механика 4 завершена");
    }

    private IEnumerator MoveBossToCenter()
    {
        while (Vector3.Distance(transform.position, centerPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPosition, bossMoveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void HideBoss()
	{
		// Скрываем сам объект босса
		if (bossSprite != null) bossSprite.enabled = false;
		if (bossCollider != null) bossCollider.enabled = false;

		// Скрываем все дочерние объекты
		foreach (Transform child in transform)
		{
			SpriteRenderer childSprite = child.GetComponent<SpriteRenderer>();
			if (childSprite != null) childSprite.enabled = false;

			Collider2D childCollider = child.GetComponent<Collider2D>();
			if (childCollider != null) childCollider.enabled = false;
		}
	}

    private void ShowBoss()
	{
		// Включаем сам объект босса
		if (bossSprite != null) bossSprite.enabled = true;
		if (bossCollider != null) bossCollider.enabled = true;

		// Включаем все дочерние объекты
		foreach (Transform child in transform)
		{
			SpriteRenderer childSprite = child.GetComponent<SpriteRenderer>();
			if (childSprite != null) childSprite.enabled = true;

			Collider2D childCollider = child.GetComponent<Collider2D>();
			if (childCollider != null) childCollider.enabled = true;
		}
	}

    private Vector3 GetRandomPositionWithinCameraBounds(Vector3 objectSize)
    {
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        float randomX = Random.Range(min.x + objectSize.x / 2, max.x - objectSize.x / 2);
        float randomY = Random.Range(min.y + objectSize.y / 2, max.y - objectSize.y / 2);

        return new Vector3(randomX, randomY, 0);
    }

    private IEnumerator FlashSafeZone(GameObject safeZone)
    {
        SpriteRenderer renderer = safeZone.GetComponent<SpriteRenderer>();
        for (int i = 0; i < 2; i++)
        {
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0);
            yield return new WaitForSeconds(safeZoneFlashDuration / 2);
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1);
            yield return new WaitForSeconds(safeZoneFlashDuration / 2);
        }
    }

    private IEnumerator SpawnLava(GameObject[] lavaSegments)
    {
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        float segmentWidth = (max.x - min.x) / this.lavaSegments;
		
		// Воспроизводим звук при появлении лавы
        if (lavaSound != null)
        {
            lavaSound.Play();
        }

        for (int i = 0; i < this.lavaSegments; i++)
        {
            Vector3 spawnPosition = new Vector3(min.x + segmentWidth * i + segmentWidth / 2, (min.y + max.y) / 2, 0);
            lavaSegments[i] = Instantiate(lavaPrefab, spawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(lavaSpawnDelay);
        }
    }

    private IEnumerator DespawnLava(GameObject[] lavaSegments)
    {
        for (int i = lavaSegments.Length - 1; i >= 0; i--)
        {
            Destroy(lavaSegments[i]);
            yield return new WaitForSeconds(lavaDespawnDelay);
        }
		
		yield return new WaitForSeconds(3f);
		
		// Останавливаем звук при удалении последней лавы
        if (lavaSound != null)
        {
            lavaSound.Stop();
        }
    }

	private IEnumerator MoveSafeZone(GameObject safeZone)
	{
		float elapsedTime = 0f;

		while (elapsedTime < safeZoneMovementTime)
		{
			Vector3 newPosition = GetRandomPositionWithinCameraBounds(safeZone.GetComponent<SpriteRenderer>().bounds.size);
			Vector3 startPosition = safeZone.transform.position;

			float moveTime = Vector3.Distance(startPosition, newPosition) / safeZoneMoveSpeed;
			float timer = 0f;

			while (timer < moveTime)
			{
				safeZone.transform.position = Vector3.Lerp(startPosition, newPosition, timer / moveTime);
				timer += Time.deltaTime;
				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}
	}
}
