using System.Collections;
using UnityEngine;

public class Mechanic3 : MonoBehaviour, IBossMechanic
{
    public GameObject projectilePrefab;
    public float transitionSpeed = 2f;
    public float shootInterval = 1f;
    public int minShots = 4;
    public int maxShots = 6;
    private Vector3[] cornerPositions;
    private float offsetX;
    private float offsetY;
    private Transform playerTransform;

    private void Start()
    {
        // Определение игрока по тегу (убедитесь, что игроку назначен тег "Player")
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Определяем размеры спрайта босса
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            offsetX = spriteRenderer.bounds.extents.x;
            offsetY = spriteRenderer.bounds.extents.y;
        }

        // Определяем позиции углов экрана с учетом отступов
        Camera mainCamera = Camera.main;
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));
        Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));
        
        // Устанавливаем отступы от краев для каждой угловой позиции
        bottomLeft += new Vector3(offsetX, offsetY, 0);
        topLeft += new Vector3(offsetX, -offsetY, 0);
        bottomRight += new Vector3(-offsetX, offsetY, 0);
        topRight += new Vector3(-offsetX, -offsetY, 0);

        // Сохраняем угловые позиции
        cornerPositions = new Vector3[] { bottomLeft, topLeft, bottomRight, topRight };
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 3: Перемещение в ближайший угол и стрельба по игроку");

        // Выбираем ближайший угол
        Vector3 targetCorner = GetClosestCorner();

        // Плавное перемещение к ближайшему углу
        yield return MoveToCorner(targetCorner);

        // Выполнение серии выстрелов
        int shotCount = Random.Range(minShots, maxShots + 1);
        for (int i = 0; i < shotCount; i++)
        {
            ShootBouncingProjectile();
            yield return new WaitForSeconds(shootInterval);
        }

        Debug.Log("Механика 3 завершена");
    }

    private Vector3 GetClosestCorner()
    {
        Vector3 currentPosition = transform.position;
        Vector3 closestCorner = cornerPositions[0];
        float minDistance = Vector3.Distance(currentPosition, closestCorner);

        foreach (Vector3 corner in cornerPositions)
        {
            float distance = Vector3.Distance(currentPosition, corner);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCorner = corner;
            }
        }

        return closestCorner;
    }

    private IEnumerator MoveToCorner(Vector3 targetCorner)
    {
        while (Vector3.Distance(transform.position, targetCorner) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetCorner, transitionSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Босс достиг угла");
    }

    private void ShootBouncingProjectile()
    {
        if (playerTransform == null) return;

        // Рассчитываем направление к игроку
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // Создаем снаряд и передаем направление
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<BouncingProjectile>().Initialize(directionToPlayer);
    }
}
