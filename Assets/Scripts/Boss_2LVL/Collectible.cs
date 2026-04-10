using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public event Action OnCollected;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollected?.Invoke(); // Уведомляем о сборе объекта
            Destroy(gameObject);   // Уничтожаем объект
        }
    }
}
