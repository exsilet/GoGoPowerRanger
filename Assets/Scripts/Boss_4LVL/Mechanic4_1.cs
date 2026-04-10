using System.Collections;
using UnityEngine;

public class Mechanic4_1 : MonoBehaviour, IBossMechanic
{
    public GameObject regularProjectilePrefab;      // Префаб обычного выстрела
    public GameObject collectibleProjectilePrefab;   // Префаб поднимающегося выстрела
    public float moveSpeed = 3f;                    // Скорость перемещения босса по оси Y
    public float verticalMoveRange = 4f;            // Диапазон движения по оси Y
    public float regularShootInterval = 1f;         // Начальный интервал между регулярными выстрелами
    public int regularShootCount = 50;              // Общее количество регулярных выстрелов в серии
    public int collectibleSpawnRangeMin = 5;        // Минимум интервала для появления поднимающегося выстрела
    public int collectibleSpawnRangeMax = 7;        // Максимум интервала для появления поднимающегося выстрела
    public float speedIncreaseFactor = 1.2f;        // Ускорение при пропуске поднимающегося выстрела
    public float projectileSpeed = 5f;              // Начальная скорость снарядов

    private bool isExecuting;

    public IEnumerator Execute()
    {
        Debug.Log("Механика 4_1 запущена.");
        isExecuting = true;

        // Перемещаем босса в точку X = 7
        Vector3 targetPosition = new Vector3(7, transform.position.y, transform.position.z);
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Запускаем движение по оси Y
        StartCoroutine(MoveVertically());

        // Запускаем серию выстрелов
        yield return StartCoroutine(ShootProjectilesWithCollectibles());

        isExecuting = false;
        Debug.Log("Механика 4_1 завершена.");
    }

    private IEnumerator MoveVertically()
    {
        float direction = 1f;
        while (isExecuting)
        {
            transform.position += new Vector3(0, direction * moveSpeed * Time.deltaTime, 0);

            if (transform.position.y >= verticalMoveRange) direction = -1f;
            if (transform.position.y <= -verticalMoveRange) direction = 1f;

            yield return null;
        }
    }

    private IEnumerator ShootProjectilesWithCollectibles()
    {
        int nextCollectiblePosition = Random.Range(collectibleSpawnRangeMin, collectibleSpawnRangeMax); // Случайная позиция для первого поднимающегося объекта

        for (int i = 0; i < regularShootCount; i++)
        {
            if (i == nextCollectiblePosition)
            {
                // Вместо регулярного выстрела создаем поднимающийся объект
                StartCoroutine(ShootCollectibleProjectile());

                // Устанавливаем позицию для следующего поднимающегося выстрела
                nextCollectiblePosition += Random.Range(collectibleSpawnRangeMin, collectibleSpawnRangeMax);
            }
            else
            {
                // Создаем обычный выстрел
                GameObject projectile = Instantiate(regularProjectilePrefab, transform.position, Quaternion.identity);
                SetProjectileMovement(projectile);
            }
            
            yield return new WaitForSeconds(regularShootInterval); // Ждем перед следующим выстрелом
        }
    }

    private IEnumerator ShootCollectibleProjectile()
    {
        GameObject collectibleProjectile = Instantiate(collectibleProjectilePrefab, transform.position, Quaternion.identity);
        CollectibleProjectile collectibleScript = collectibleProjectile.GetComponent<CollectibleProjectile>();

        bool wasCollected = false;

        if (collectibleScript != null)
        {
            collectibleScript.OnCollected += () => 
            {
                wasCollected = true;  // Если игрок собирает объект
            };
        }
        else
        {
            Debug.LogWarning("CollectibleProjectile component not found on collectibleProjectilePrefab!");
        }

        SetProjectileMovement(collectibleProjectile);

        // Ждем, пока объект выйдет за экран или будет собран
        yield return StartCoroutine(DestroyCollectibleProjectileOutsideScreen(collectibleProjectile, wasCollected));

        // Если объект не был собран, увеличиваем скорость и частоту обычных выстрелов
        if (!wasCollected)
        {
            regularShootInterval /= speedIncreaseFactor;
            projectileSpeed *= speedIncreaseFactor;
            Debug.Log("Collectible missed! Increasing regular shoot speed and interval.");
        }
    }

    private IEnumerator DestroyCollectibleProjectileOutsideScreen(GameObject projectile, bool wasCollected)
    {
        while (projectile != null)
        {
            if (projectile.transform.position.x < -Camera.main.orthographicSize * Camera.main.aspect - 1 || 
                projectile.transform.position.y > Camera.main.orthographicSize + 1 || 
                projectile.transform.position.y < -Camera.main.orthographicSize - 1)
            {
                if (!wasCollected)
                {
                    Debug.Log("Collectible missed and destroyed outside screen.");
                }
                Destroy(projectile);
            }
            yield return null;
        }
    }

    private void SetProjectileMovement(GameObject projectile)
    {
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.left * projectileSpeed;
        }
    }
}
