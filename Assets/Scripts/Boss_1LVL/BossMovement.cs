using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float initialSpeed = 3f;     // Скорость движения босса по оси X
    public float ySpeed = 2f;           // Скорость движения босса по оси Y после остановки
    public float stopPositionX = 0f;    // Позиция по X, где босс должен остановиться перед игроком
    public float upperLimitY = 3f;      // Верхняя граница по оси Y
    public float lowerLimitY = -3f;     // Нижняя граница по оси Y

    private bool hasStopped = false;    // Флаг, указывающий, остановился ли босс
    private bool movingUp = true;       // Флаг для направления движения по оси Y

    void Update()
    {
        if (!hasStopped)
        {
            // Двигаем босса к игроку по оси X, пока он не достигнет stopPositionX
            transform.position += Vector3.left * initialSpeed * Time.deltaTime;

            // Проверка, достиг ли босс позиции остановки
            if (transform.position.x <= stopPositionX)
            {
                hasStopped = true; // Останавливаем движение по оси X
            }
        }
        else
        {
            // После остановки, босс начинает двигаться по оси Y (вверх и вниз)
            MoveVertically();
        }
    }

    void MoveVertically()
    {
        // Проверяем текущую позицию по оси Y
        if (transform.position.y >= upperLimitY)
        {
            movingUp = false; // Меняем направление на вниз
        }
        else if (transform.position.y <= lowerLimitY)
        {
            movingUp = true; // Меняем направление на вверх
        }

        // Двигаем босса в выбранном направлении
        Vector3 movement = movingUp ? Vector3.up : Vector3.down;
        transform.position += movement * ySpeed * Time.deltaTime;
    }
}
