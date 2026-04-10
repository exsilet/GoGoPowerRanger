using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthController : MonoBehaviour
{
    // Родительский объект для индикатора
    public Transform indicatorParent;

    // Префабы для случайного выбора
    public GameObject bluePrefab;
    public GameObject redPrefab;
    public GameObject blackPrefab;

    // Объекты, закрывающие проходы
    public GameObject topDoor;    // Верхний проход (синий)
    public GameObject middleDoor; // Средний проход (красный)
    public GameObject bottomDoor; // Нижний проход (чёрный)

    private GameObject currentIndicator; // Текущий индикатор

    private void Start()
    {
        AssignRandomPrefab();
    }

    // Метод для назначения случайного префаба объекту и открытия нужного прохода
    private void AssignRandomPrefab()
    {
        // Уничтожаем текущий индикатор, если он существует
        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }

        // Случайный выбор префаба
        int prefabChoice = Random.Range(0, 3);
        GameObject chosenPrefab = null;

        switch (prefabChoice)
        {
            case 0:
                chosenPrefab = bluePrefab;
                OpenPassage("Blue");
                break;
            case 1:
                chosenPrefab = redPrefab;
                OpenPassage("Red");
                break;
            case 2:
                chosenPrefab = blackPrefab;
                OpenPassage("Black");
                break;
        }

        // Создаём выбранный префаб как индикатор
        currentIndicator = Instantiate(chosenPrefab, indicatorParent.position, Quaternion.identity, indicatorParent);
    }

    // Метод для открытия определённого прохода в зависимости от выбранного префаба
    private void OpenPassage(string color)
    {
        // Закрываем все проходы
        topDoor.SetActive(true);
        middleDoor.SetActive(true);
        bottomDoor.SetActive(true);

        // Открываем нужный проход
        if (color == "Blue")
        {
            topDoor.SetActive(false); // Открываем верхний проход
        }
        else if (color == "Red")
        {
            middleDoor.SetActive(false); // Открываем средний проход
        }
        else if (color == "Black")
        {
            bottomDoor.SetActive(false); // Открываем нижний проход
        }
    }
}
