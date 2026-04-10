using System.Collections;
using UnityEngine;

public class FlipXToggle : MonoBehaviour
{
    private SpriteRenderer spriteRenderer; // Ссылка на SpriteRenderer
    public float toggleInterval = 0.3f;    // Интервал между переключениями
    private Coroutine toggleCoroutine;    // Ссылка на текущую корутину

    void Awake()
    {
        // Получаем компонент SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // Запускаем корутину для чередования, если объект включён
        if (toggleCoroutine == null)
        {
            toggleCoroutine = StartCoroutine(ToggleFlipX());
        }
    }

    void OnDisable()
    {
        // Останавливаем корутину, если объект выключается
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }
    }

    IEnumerator ToggleFlipX()
    {
        while (true) // Бесконечный цикл
        {
            // Переключаем значение FlipX
            spriteRenderer.flipX = !spriteRenderer.flipX;

            // Ждём указанный интервал
            yield return new WaitForSeconds(toggleInterval);
        }
    }
}
