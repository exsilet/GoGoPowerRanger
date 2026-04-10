using UnityEngine;

public class FallingObjectCleaner : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Проверяем, если объект выходит за нижнюю границу экрана
        Vector3 screenPosition = mainCamera.WorldToViewportPoint(transform.position);
        if (screenPosition.y < 0)
        {
            Destroy(gameObject); // Удаляем объект, если он выходит за пределы экрана снизу
        }
    }
}
