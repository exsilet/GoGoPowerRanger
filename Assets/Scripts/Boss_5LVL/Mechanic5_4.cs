using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic5_4 : MonoBehaviour, IBossMechanic
{
    public Transform boss;
    public GameObject projectilePrefab;
    public Transform player;
    public int shotsPerSequence = 3;
    public float blinkDuration = 0.3f;
    public float teleportDelay = 0.5f;
    public int sequenceCount = 3;

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float bossWidth;
    private float bossHeight;
    private SpriteRenderer bossSpriteRenderer;

    private List<GameObject> projectiles = new List<GameObject>(); // Список для отслеживания всех выпущенных снарядов
    private Collider2D[] boundaryColliders; // Список всех объектов с тегом "Boundary"
	public AudioSource shootSound; // Звук выстрела

    private void Start()
    {
        // Рассчитываем границы камеры и размеры босса
        CalculateCameraBounds();
        bossWidth = boss.GetComponent<SpriteRenderer>().bounds.extents.x;
        bossHeight = boss.GetComponent<SpriteRenderer>().bounds.extents.y;
        bossSpriteRenderer = boss.GetComponent<SpriteRenderer>();

        // Находим все объекты с тегом "Boundary"
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("Boundary");
        boundaryColliders = new Collider2D[boundaries.Length];
        for (int i = 0; i < boundaries.Length; i++)
        {
            boundaryColliders[i] = boundaries[i].GetComponent<Collider2D>();
        }
    }

    public IEnumerator Execute()
    {
        // Отключаем коллайдеры объектов с тегом "Boundary"
        SetBoundaryCollidersEnabled(false);

        for (int i = 0; i < sequenceCount; i++)
        {
            // Шаг 1: Босс мигает
            yield return StartCoroutine(BlinkBoss(2, blinkDuration));

            // Шаг 2: Босс становится невидимым
            SetBossVisibility(false);

            // Задержка перед телепортацией
            yield return new WaitForSeconds(teleportDelay);

            // Шаг 3: Босс телепортируется и становится видимым
            TeleportBoss();
            SetBossVisibility(true);

            // Шаг 4: Босс стреляет в игрока
            yield return StartCoroutine(ShootProjectiles());

            // Пауза перед следующим циклом
            yield return new WaitForSeconds(teleportDelay);
        }

        // Шаг 6: Босс мигает в конце
        yield return StartCoroutine(BlinkBoss(2, blinkDuration));

        // Включаем коллайдеры объектов с тегом "Boundary"
        SetBoundaryCollidersEnabled(true);

        // Удаляем все выпущенные снаряды
        DestroyAllProjectiles();

        // Шаг 7: Конец механики
    }

    private IEnumerator BlinkBoss(int blinkCount, float blinkDuration)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            SetBossVisibility(false); // Скрыть босса
            yield return new WaitForSeconds(blinkDuration);
            SetBossVisibility(true);  // Показать босса
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void TeleportBoss()
    {
        // Получаем случайные координаты в пределах видимой зоны, с учётом размеров босса
        float randomX = Random.Range(minBounds.x + bossWidth, maxBounds.x - bossWidth);
        float randomY = Random.Range(minBounds.y + bossHeight, maxBounds.y - bossHeight);
        
        boss.position = new Vector2(randomX, randomY);
    }

    private void CalculateCameraBounds()
    {
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));
    }

    private IEnumerator ShootProjectiles()
	{
		for (int i = 0; i < shotsPerSequence; i++)
		{
			// Проигрываем звук выстрела, если задан
			if (shootSound != null)
			{
				shootSound.Play();
			}

			// Создаём снаряд
			GameObject projectile = Instantiate(projectilePrefab, boss.position, Quaternion.identity);
			Vector2 direction = (player.position - boss.position).normalized;
			projectile.GetComponent<Rigidbody2D>().velocity = direction * 5f; // Устанавливаем скорость снаряда

			// Добавляем снаряд в список для последующего удаления
			projectiles.Add(projectile);

			yield return new WaitForSeconds(0.2f); // Задержка между выстрелами
		}
	}

    private void SetBossVisibility(bool visible)
    {
        if (bossSpriteRenderer != null)
        {
            bossSpriteRenderer.enabled = visible;
        }
    }

    private void SetBoundaryCollidersEnabled(bool enabled)
    {
        foreach (var collider in boundaryColliders)
        {
            if (collider != null)
            {
                collider.enabled = enabled;
            }
        }
    }

    private void DestroyAllProjectiles()
    {
        foreach (var projectile in projectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }
        projectiles.Clear();
    }
}
