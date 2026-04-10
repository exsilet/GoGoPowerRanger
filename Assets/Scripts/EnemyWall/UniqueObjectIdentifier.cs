using UnityEngine;

public class UniqueObjectIdentifier : MonoBehaviour
{
    [Header("Unique Object ID")]
    public string objectID; // Уникальный ID объекта

    [Header("Trigger Settings")]
    public float targetX = 5f; // Значение X для фиксации пересечения (настраиваемое)

    private ObjectVisibilityTracker tracker;
    private bool hasTriggered = false; // Флаг, чтобы ID передавался только один раз

    void Start()
    {
        // Ищем трекер на сцене
        tracker = FindObjectOfType<ObjectVisibilityTracker>();
        if (tracker == null)
        {
            Debug.LogError("ObjectVisibilityTracker не найден на сцене!");
        }
    }

    void Update()
    {
        // Проверяем, достиг ли объект целевого X
        if (!hasTriggered && transform.position.x <= targetX && tracker != null)
        {
            // Передаем ID в трекер
            tracker.NotifyObjectVisible(objectID, gameObject);

            // Отмечаем, что объект уже был обработан
            hasTriggered = true;

            // Дополнительно можно отключить этот скрипт, если больше не нужен
            enabled = false;
        }
    }
}
