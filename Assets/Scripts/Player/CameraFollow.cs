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
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
