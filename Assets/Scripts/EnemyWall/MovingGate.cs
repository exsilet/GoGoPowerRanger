using System.Collections;
using UnityEngine;

public class MovingGate : MonoBehaviour
{
    public float moveSpeed = 3f;       // Скорость движения по оси X
    public float liftSpeed = 2f;       // Скорость подъёма по оси Y
    public float liftHeight = 5f;      // Высота, на которую поднимаются ворота
    private bool isLifted = false;     // Флаг, подняты ли ворота
    private Vector3 initialPosition;   // Начальная позиция ворот
    private Vector3 targetPosition;    // Целевая позиция для подъёма

    void Start()
    {
        // Сохраняем начальную позицию ворот и вычисляем целевую позицию по Y
        initialPosition = transform.position;
        targetPosition = initialPosition + new Vector3(0, liftHeight, 0);
    }

    void Update()
    {
        // Двигаем ворота по оси X всегда, независимо от состояния
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Если ворота подняты, они плавно поднимаются по оси Y
        if (isLifted && transform.position.y < targetPosition.y)
        {
            transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(transform.position.y, targetPosition.y, liftSpeed * Time.deltaTime), transform.position.z);
        }
    }

    // Метод для подъёма ворот
    public void LiftGate()
    {
        if (!isLifted)
        {
            isLifted = true;
            Debug.Log("Ворота начинают подниматься!");
        }
    }
}
