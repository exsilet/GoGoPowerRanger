using System.Collections;
using UnityEngine;

public class Mechanic1 : MonoBehaviour, IBossMechanic
{
    // Параметры движения
    public float entrySpeed = 2f;
    public float circularSpeed = 1f;
    public float radiusX = 6f;
    public float radiusY = 4f;
    private float angle = 0f;
    private Vector3 centerPosition;

    // Параметры стрельбы
    public GameObject projectilePrefab;
    public int projectilesPerWave = 8;
    public float projectileSpeed = 5f;
    public float minShootInterval = 3f;
    public float maxShootInterval = 5f;

    [Header("Audio")]
    public AudioSource shootSound; // Звук стрельбы

    private void Start()
    {
        // Устанавливаем центр орбиты в центр экрана
        centerPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
        centerPosition.z = 0;
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 1: Переход на орбиту и овальное движение со стрельбой");

        // Плавный переход к начальной точке орбиты
        yield return MoveToOrbit();

        // Запуск кругового движения и стрельбы параллельно
        Coroutine circularMovementCoroutine = StartCoroutine(CircularMovement());
        Coroutine shootingCoroutine = StartCoroutine(ShootProjectileWaves());

        // Продолжительность выполнения механики
        yield return new WaitForSeconds(Random.Range(5f, 8f));

        if (circularMovementCoroutine != null) StopCoroutine(circularMovementCoroutine);
        if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);

        Debug.Log("Завершение механики 1");
    }

    // Плавный переход к орбите
    private IEnumerator MoveToOrbit()
    {
        Vector3 startPositionOnOrbit = new Vector3(centerPosition.x + Mathf.Cos(angle) * radiusX, centerPosition.y + Mathf.Sin(angle) * radiusY, transform.position.z);
        while (Vector3.Distance(transform.position, startPositionOnOrbit) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPositionOnOrbit, entrySpeed * Time.deltaTime);
            yield return null;
        }
    }

    // Круговое движение по орбите
    private IEnumerator CircularMovement()
    {
        while (true)
        {
            angle += circularSpeed * Time.deltaTime;
            float x = centerPosition.x + Mathf.Cos(angle) * radiusX;
            float y = centerPosition.y + Mathf.Sin(angle) * radiusY;
            transform.position = new Vector3(x, y, transform.position.z);
            yield return null;
        }
    }

    // Стрельба волнами
    private IEnumerator ShootProjectileWaves()
    {
        while (true)
        {
            int waveCount = Random.Range(3, 6);

            for (int i = 0; i < waveCount; i++)
            {
                LaunchProjectileWave();
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(Random.Range(minShootInterval, maxShootInterval));
        }
    }

    private void LaunchProjectileWave()
    {
        // Воспроизведение звука стрельбы
        if (shootSound != null)
        {
            shootSound.Play();
        }

        float angleStep = 360f / projectilesPerWave;
        float angle = 0f;

        for (int i = 0; i < projectilesPerWave; i++)
        {
            float projectileDirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float projectileDirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 projectileMoveDirection = new Vector3(projectileDirX, projectileDirY, 0);

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody2D>().velocity = projectileMoveDirection * projectileSpeed;

            angle += angleStep;
        }
    }
}
