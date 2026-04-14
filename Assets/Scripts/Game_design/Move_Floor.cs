using UnityEngine;
using System.Collections;

public class Move_Floor : MonoBehaviour
{
    public float speed = 3f;
    public float resetPosition = -10f;
    public float startPosition = 10f;
    public static bool stopMovement = false;

    private Vector3 initialPosition;
    private Coroutine moveFloorCoroutine; // Хранит текущую корутину

    void Start()
    {
        initialPosition = transform.position;
        // StartMovement();
    }
	    
	void OnEnable()
    {
        // Перезапуск корутины при активации объекта
        StartMovement();
    }

    private IEnumerator MoveFloor()
    {
        while (true)
        {
            if (!stopMovement)
            {
                transform.Translate(Vector2.left * speed * Time.deltaTime);

                if (transform.position.x <= resetPosition)
                {
                    Vector3 newPos = new Vector3(startPosition, transform.position.y, transform.position.z);
                    transform.position = newPos;
                }
            }
            yield return null; // Ждём следующий кадр
        }
    }

    public void StartMovement()
    {
        if (moveFloorCoroutine != null)
        {
            StopCoroutine(moveFloorCoroutine); // Останавливаем предыдущую корутину, если она была запущена
        }

        speed = 3f; // Сбрасываем скорость перед запуском
        moveFloorCoroutine = StartCoroutine(MoveFloor());
    }

    public void RestartMovement()
    {
        stopMovement = false;
        speed = 3f; // Сбрасываем скорость
        transform.position = initialPosition; // Возвращаем объект на начальную позицию
        StartMovement(); // Перезапускаем движение
        
        Debug.Log("Start_Design restarted");
        Debug.Log("Move_Floor restarted: " + gameObject.name);
    }

    private void OnDisable()
    {
        if (moveFloorCoroutine != null)
        {
            StopCoroutine(moveFloorCoroutine); // Останавливаем корутину при отключении объекта
        }
    }
}

