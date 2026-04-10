using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Скорость движения препятствия
    public float speed = 3f;

    void Update()
    {
        // Двигаем препятствие влево по оси X
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Удаляем препятствие, если оно вышло за пределы экрана
        if (transform.position.x < -20f)
        {
            Destroy(gameObject);
        }
    }
}
