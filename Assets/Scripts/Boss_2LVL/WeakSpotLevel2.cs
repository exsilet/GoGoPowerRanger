using System.Collections;
using UnityEngine;

public class WeakSpotLevel2 : MonoBehaviour
{
    private BossLevel2 boss;

    // Активируем слабое место и задаем босс для вызова урона
    public void ActivateForDuration(float duration, BossLevel2 assignedBoss)
    {
        boss = assignedBoss;  // Назначаем босса для нанесения урона
        StartCoroutine(WeakSpotRoutine(duration));
    }

    IEnumerator WeakSpotRoutine(float duration)
    {
        // Делаем слабое место активным на заданное время
        yield return new WaitForSeconds(duration);

        // Удаляем слабое место по истечении времени
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerController>().isBoosting)
        {
            // Если игрок попал в слабое место с бустом, нанесем урон боссу
            boss.TakeDamage();
            Destroy(gameObject);  // Удаляем слабое место после попадания
        }
    }
}
