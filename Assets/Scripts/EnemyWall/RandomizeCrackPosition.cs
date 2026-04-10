using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeCrackPosition : MonoBehaviour
{
    // Массив для хранения всех частей (4 препятствия и 1 трещина)
    public GameObject[] parts; // Убедись, что ты добавил сюда все части в инспекторе

    // Индекс части, которая является трещиной
    public int crackIndex = 4; // Индекс трещины в массиве (например, последний элемент)

    void Start()
    {
        // Перемешиваем позиции всех частей
        RandomizeParts();
    }

    // Метод для перемешивания трещины среди препятствий
    private void RandomizeParts()
    {
        // Создаем список для хранения возможных позиций
        List<Vector3> originalPositions = new List<Vector3>();

        // Запоминаем исходные позиции всех частей
        foreach (GameObject part in parts)
        {
            originalPositions.Add(part.transform.position);
        }

        // Перемешиваем список позиций
        for (int i = 0; i < originalPositions.Count; i++)
        {
            Vector3 temp = originalPositions[i];
            int randomIndex = Random.Range(0, originalPositions.Count);
            originalPositions[i] = originalPositions[randomIndex];
            originalPositions[randomIndex] = temp;
        }

        // Присваиваем перемешанные позиции всем частям
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].transform.position = originalPositions[i];
        }
    }
}
