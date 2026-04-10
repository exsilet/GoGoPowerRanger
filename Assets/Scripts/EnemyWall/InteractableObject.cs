using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string uniqueID; // Уникальный ID объекта

    private bool isInteracted = false; // Флаг взаимодействия
    private ObjectInteractionManager manager;

    void Start()
    {
        manager = FindObjectOfType<ObjectInteractionManager>();

        // Генерация уникального ID, если он не задан вручную
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = System.Guid.NewGuid().ToString();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInteracted && other.CompareTag("Player"))
        {
            isInteracted = true;
            Debug.Log($"{gameObject.name} взаимодействовал с игроком (ID: {uniqueID})");

            // Уведомляем менеджер
            if (manager != null)
            {
                manager.MarkAsInteractedByID(uniqueID);
            }
        }
    }
}
