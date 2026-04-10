using UnityEngine;

public class ProjectileCleaner : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 screenPosition = mainCamera.WorldToViewportPoint(transform.position);

        // Проверяем, если снаряд вышел за границы экрана
        if (screenPosition.x < 0 || screenPosition.x > 1 || screenPosition.y < 0 || screenPosition.y > 1)
        {
            Destroy(gameObject); // Удаляем снаряд
        }
    }
}
