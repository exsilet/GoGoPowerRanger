using UnityEngine;
using System.Collections;

public class ExplosiveSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject projectilePrefab; // Префаб снаряда, который нужно запускать
    public float minInterval = 10f;     // Минимальное время между запусками
    public float maxInterval = 15f;     // Максимальное время между запусками

    [Header("Spawn Range")]
    public float minY = -3.5f;          // Нижняя граница спауна по Y
    public float maxY = 3.5f;           // Верхняя граница спауна по Y

    [Header("Projectile Settings")]
    public float detectionRadius = 5f;  // Радиус, в котором снаряд взрывается при приближении к игроку
    public float explosionForce = 15f;  // Сила отталкивания от взрыва
    public float disableDuration = 1f;  // Время отключения управления игроком после взрыва

    private void Start()
    {
        StartCoroutine(SpawnProjectiles());
    }

    private IEnumerator SpawnProjectiles()
    {
        while (true) // Бесконечный цикл для запуска
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            SpawnProjectile();
        }
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab != null)
        {
            // Генерируем случайную позицию по Y
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(transform.position.x, randomY, transform.position.z);

            // Создаем снаряд в указанной позиции
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // Передаем параметры в снаряд
            ExplosiveProjectile explosiveProjectile = projectile.GetComponent<ExplosiveProjectile>();
            if (explosiveProjectile != null)
            {
                explosiveProjectile.detectionRadius = detectionRadius;
                explosiveProjectile.explosionForce = explosionForce;
                explosiveProjectile.disableDuration = disableDuration;
            }
        }
    }
}
