using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonCloud : MonoBehaviour
{
    public float speed = 1.5f;         // Скорость движения облака
    public float slowEffect = 0.5f;    // Влияние замедления (например, 0.5 — уменьшает скорость в 2 раза)
    public float destroyXPosition = -12f; // Координата X, за пределами которой облако уничтожается
    public Color cloudColor = new Color(0.1f, 0.1f, 0.1f, 0.1f); // Полупрозрачный зелёный цвет

    private PlayerController playerController;
    private bool isPlayerInside = false;

    void Start()
    {
        // Устанавливаем полупрозрачный цвет для облака
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = cloudColor;
        }
    }

    void Update()
    {
        // Двигаем облако влево
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Проверяем, если облако выехало за пределы экрана, уничтожаем его
        if (transform.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Если игрок вошёл в облако, уменьшаем его скорость
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !isPlayerInside)
            {
                playerController.moveSpeed *= slowEffect;
                isPlayerInside = true;
                Debug.Log("Игрок замедлен ядовитым облаком!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Если игрок покидает облако, восстанавливаем его скорость
        if (other.CompareTag("Player") && isPlayerInside)
        {
            if (playerController != null)
            {
                playerController.moveSpeed /= slowEffect;
                isPlayerInside = false;
                Debug.Log("Игрок вышел из облака, скорость восстановлена.");
            }
        }
    }
}
