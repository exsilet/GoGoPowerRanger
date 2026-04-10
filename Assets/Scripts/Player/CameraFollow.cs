using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Ссылка на игрока
    public Vector3 offset;    // Смещение камеры относительно игрока
    public float smoothSpeed = 0.125f;  // Скорость плавного следования камеры

    void LateUpdate()
    {
        // Целевая позиция камеры с учетом смещения
        Vector3 desiredPosition = player.position + offset;

        // Плавное перемещение камеры к целевой позиции
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Обновляем позицию камеры
        transform.position = smoothedPosition;
    }
}
