using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    // Скорость вращения
    public float rotationSpeed = 100f;
    
    // Переменная для выбора направления вращения
    public bool clockwise = true;

    void Update()
    {
        // Устанавливаем направление вращения на основе переменной clockwise
        float direction = clockwise ? -1f : 1f;
        
        // Вращаем объект вокруг его оси Z
        transform.Rotate(0f, 0f, rotationSpeed * direction * Time.deltaTime);
    }
}
