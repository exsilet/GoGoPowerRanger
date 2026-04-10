using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float speed = 5f;               // Скорость снаряда
    private Camera mainCamera;             // Камера для определения границ экрана

    void Start()
    {
        mainCamera = Camera.main;
        MoveTowardsPlayer();
    }

    void Update()
    {
        // Проверяем, находится ли снаряд на экране
        CheckIfOutOfBounds();
    }

    void MoveTowardsPlayer()
    {
        // Направляем снаряд к игроку
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().velocity = direction * speed;
        }
    }

    void CheckIfOutOfBounds()
    {
        Vector3 screenPosition = mainCamera.WorldToViewportPoint(transform.position);
        
        // Если снаряд вышел за пределы экрана, уничтожаем его
        if (screenPosition.x < 0 || screenPosition.x > 1 || screenPosition.y < 0 || screenPosition.y > 1)
        {
            Destroy(gameObject);
        }
    }
}
