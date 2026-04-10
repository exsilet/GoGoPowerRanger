using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    // Ссылки на части препятствия
    public GameObject upperObstacle;  // Верхнее препятствие
    public GameObject lowerObstacle;  // Нижнее препятствие
    public GameObject transparentPart;  // Прозрачная часть, которая создает проход

    // Параметры движения
    public float speed = 2f;  // Скорость движения
    public float upperLimit = 4f;  // Верхняя граница игрового поля
    public float lowerLimit = -4f; // Нижняя граница игрового поля

    private bool movingUp = true;  // Направление движения

    void Start()
    {
        // Устанавливаем случайную начальную позицию для объекта в пределах границ игрового поля
        float randomStartY = Random.Range(lowerLimit, upperLimit);
        transform.position = new Vector3(transform.position.x, randomStartY, transform.position.z);
    }

    void Update()
    {
        // Двигаем объект вверх и вниз
        if (movingUp)
        {
            MoveUp();
        }
        else
        {
            MoveDown();
        }

        // Проверка выхода прозрачной части за пределы
        CheckBounds();
    }

    void MoveUp()
    {
        // Двигаем весь объект вверх
        transform.position += Vector3.up * speed * Time.deltaTime;

        // Если достигли верхней границы, меняем направление
        if (transparentPart.transform.position.y >= upperLimit)
        {
            movingUp = false;
        }
    }

    void MoveDown()
    {
        // Двигаем весь объект вниз
        transform.position += Vector3.down * speed * Time.deltaTime;

        // Если достигли нижней границы, меняем направление
        if (transparentPart.transform.position.y <= lowerLimit)
        {
            movingUp = true;
        }
    }

    void CheckBounds()
    {
        // Ограничиваем движение прозрачной части, чтобы она не выходила за пределы поля
        float clampedY = Mathf.Clamp(transparentPart.transform.position.y, lowerLimit, upperLimit);
        transparentPart.transform.position = new Vector3(transparentPart.transform.position.x, clampedY, transparentPart.transform.position.z);
    }
}