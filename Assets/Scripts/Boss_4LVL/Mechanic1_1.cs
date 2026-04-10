using System.Collections;
using UnityEngine;

public class Mechanic1_1 : MonoBehaviour, IBossMechanic
{
    public float waveSpeed = 2f;
    public float waveAmplitudeX = 3f;
    public float waveAmplitudeY = 2f;
    public GameObject projectilePrefab;
    public int attackCountMin = 3;
    public int attackCountMax = 5;
    public float shootInterval = 3f;
    public float moveToCenterSpeed = 3f; // Скорость возвращения в центр

    private Vector3 waveStartPosition;
    private float time;
    private bool isExecuting;

    public IEnumerator Execute()
    {
        Debug.Log("Механика 1_1 запущена.");

        // Плавное возвращение в центр
        yield return StartCoroutine(MoveToCenter());

        // Устанавливаем начальную позицию для волнового движения
        waveStartPosition = transform.position;
        time = 0f;
        isExecuting = true;

        StartCoroutine(WaveMovement());
        yield return StartCoroutine(ShootProjectiles());

        isExecuting = false;
        Debug.Log("Механика 1_1 завершена.");
    }

    private IEnumerator MoveToCenter()
    {
        Vector3 centerPosition = new Vector3(0, 0, transform.position.z);
        while (Vector3.Distance(transform.position, centerPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPosition, moveToCenterSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = centerPosition; // Устанавливаем точное положение в центре
    }

    private IEnumerator WaveMovement()
    {
        while (isExecuting)
        {
            time += Time.deltaTime * waveSpeed;

            // Двигаемся по синусоиде по осям X и Y
            float xOffset = Mathf.Sin(time) * waveAmplitudeX;
            float yOffset = Mathf.Sin(time * 1.5f) * waveAmplitudeY;
            Vector3 newPosition = waveStartPosition + new Vector3(xOffset, yOffset, 0);

            // Ограничиваем движение в пределах видимой зоны камеры
            newPosition.x = Mathf.Clamp(newPosition.x, -Camera.main.orthographicSize * Camera.main.aspect + 1, Camera.main.orthographicSize * Camera.main.aspect - 1);
            newPosition.y = Mathf.Clamp(newPosition.y, -Camera.main.orthographicSize + 1, Camera.main.orthographicSize - 1);
            transform.position = newPosition;

            yield return null;
        }
    }

    private IEnumerator ShootProjectiles()
    {
        int attackCount = Random.Range(attackCountMin, attackCountMax + 1);

        for (int i = 0; i < attackCount; i++)
        {
            if (projectilePrefab != null)
            {
                Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                Debug.Log("Босс выстрелил снарядом!");
            }
            else
            {
                Debug.LogWarning("Префаб снаряда не установлен!");
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }
}
