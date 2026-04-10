using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropperObject : MonoBehaviour
{
    public GameObject objectToDrop;     // Префаб объекта, который будет сброшен
    public float dropIntervalMin = 3f;  // Минимальное время между сбросами
    public float dropIntervalMax = 10f; // Максимальное время между сбросами
    public float fallSpeed = 5f;        // Скорость падения объекта
    public float destroyDelay = 5f;     // Время, через которое объект удалится после сброса
    public AudioClip dropSound;         // Звук при падении объекта
    private AudioSource audioSource;    // Аудиоисточник для воспроизведения звука

    private Transform player;           // Ссылка на игрока
    private bool canDrop = true;        // Флаг для управления сбросом

    void Start()
    {
        // Находим игрока по тегу
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Инициализируем AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = dropSound;
    }

    void Update()
    {
        // Запускаем сброс объекта, если это возможно
        if (canDrop)
        {
            StartCoroutine(DropObject());
        }
    }

    IEnumerator DropObject()
    {
        canDrop = false; // Отключаем возможность сброса на время

        // Ждём случайное время перед сбросом
        float waitTime = Random.Range(dropIntervalMin, dropIntervalMax);
        yield return new WaitForSeconds(waitTime);

        // Сбрасываем копию объекта на позицию игрока
        if (objectToDrop != null && player != null)
        {
            // Создаём копию объекта точно над игроком по X на момент сброса
            Vector3 dropPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
            GameObject droppedObject = Instantiate(objectToDrop, dropPosition, Quaternion.identity);

            // Запускаем падение копии объекта строго вниз
            Rigidbody2D rb = droppedObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(0, -fallSpeed);  // Падение строго вниз без изменения траектории
            }

            // Воспроизводим звук падения
            if (dropSound != null && audioSource != null)
            {
                audioSource.Play();
            }

            // Удаляем объект через destroyDelay секунд
            Destroy(droppedObject, destroyDelay);
        }

        canDrop = true; // Возвращаем возможность сброса
    }
}
