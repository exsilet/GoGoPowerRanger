using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebTrap : MonoBehaviour
{
    public int requiredPresses = 3;  // Количество нажатий для освобождения
    private int pressCount = 0;      // Счетчик нажатий

    private bool isTrapped = false;  // Флаг, что игрок пойман в паутину

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTrapped = true;
            pressCount = 0;  // Сбрасываем счётчик нажатий
            Debug.Log("Игрок пойман в паутину! Нажимай A и D для освобождения.");
        }
    }

    void Update()
    {
        if (isTrapped)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        // Проверяем последовательные нажатия A и D
        if (pressCount % 2 == 0 && Input.GetKeyDown(KeyCode.A))
        {
            pressCount++;
            Debug.Log("Нажата A, прогресс: " + pressCount);
        }
        else if (pressCount % 2 == 1 && Input.GetKeyDown(KeyCode.D))
        {
            pressCount++;
            Debug.Log("Нажата D, прогресс: " + pressCount);
        }

        // Если игрок нажал A и D три раза по очереди, он освобождается
        if (pressCount >= requiredPresses)
        {
            isTrapped = false;
            Debug.Log("Игрок освободился от паутины!");
            Destroy(gameObject);  // Уничтожаем паутину
        }
    }
}
