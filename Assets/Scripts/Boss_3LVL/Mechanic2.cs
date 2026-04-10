using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic2 : MonoBehaviour, IBossMechanic
{
    public GameObject spokeObjectPrefab;     // Префаб объекта спиц
    public float minSpokeDuration = 5f;      // Минимальная продолжительность вращения спиц
    public float maxSpokeDuration = 10f;     // Максимальная продолжительность вращения спиц
    public float rotationSpeed = 60f;        // Скорость вращения спиц
    public float transitionSpeed = 2f;       // Скорость перемещения босса к центру
    private Vector3 centerPosition;          // Позиция центра экрана

    [Header("Audio Settings")]
    public AudioSource blinkSound;           // Звук для мигания
    public AudioSource rotationSound;        // Звук для вращения

    private void Start()
    {
        // Устанавливаем центр экрана
        centerPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
        centerPosition.z = 0;
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 2: Вращение спиц");

        // Перемещаем босса в центр экрана
        yield return MoveToCenter();

        // Создаем объект спиц
        GameObject spokeObject = Instantiate(spokeObjectPrefab, centerPosition, Quaternion.identity);

        // Получаем дочерние объекты, чтобы временно изменить их теги
        Transform[] childObstacles = spokeObject.GetComponentsInChildren<Transform>();

        // Сохраняем оригинальные теги дочерних объектов и временно убираем их
        Dictionary<Transform, string> originalTags = new Dictionary<Transform, string>();
        foreach (var child in childObstacles)
        {
            originalTags[child] = child.tag; // Сохраняем текущий тег
            child.tag = "Untagged"; // Временно убираем тег, чтобы не наносил урон
        }

        // Мигание спиц перед вращением
        SpriteRenderer[] renderers = spokeObject.GetComponentsInChildren<SpriteRenderer>();
        yield return FlashSpokes(renderers);

        // Возвращаем оригинальные теги дочерних объектов, чтобы они снова наносили урон
        foreach (var child in childObstacles)
        {
            if (originalTags.ContainsKey(child))
            {
                child.tag = originalTags[child];
            }
        }

        // Включаем звук вращения
        if (rotationSound != null)
        {
            rotationSound.loop = true;
            rotationSound.Play();
        }

        // Вращение объекта спиц с случайным изменением направления
        yield return RotateSpokeObject(spokeObject);

        // Останавливаем звук вращения
        if (rotationSound != null)
        {
            rotationSound.Stop();
        }

        // Удаляем объект спиц
        Destroy(spokeObject);

        Debug.Log("Механика 2 завершена");
    }

    private IEnumerator MoveToCenter()
    {
        while (Vector3.Distance(transform.position, centerPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPosition, transitionSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Босс достиг центра");
    }

    private IEnumerator FlashSpokes(SpriteRenderer[] renderers)
    {
        for (int i = 0; i < 2; i++)
        {
            // Воспроизведение звука мигания
            if (blinkSound != null)
            {
                blinkSound.Play();
            }

            // Отключаем отображение объектов
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
            yield return new WaitForSeconds(0.5f);

            // Включаем отображение объектов
            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator RotateSpokeObject(GameObject spokeObject)
    {
        float rotationDuration = Random.Range(minSpokeDuration, maxSpokeDuration);
        float timer = 0f;
        float direction = Random.value > 0.5f ? -1f : 1f;

        // Определяем случайные смены направления
        int directionChanges = Random.Range(1, 2);
        float[] changeTimes = new float[directionChanges];
        for (int i = 0; i < directionChanges; i++)
        {
            changeTimes[i] = rotationDuration * (i + 1) / (directionChanges + 1);
        }

        int changeIndex = 0;

        while (timer < rotationDuration)
        {
            spokeObject.transform.Rotate(0f, 0f, rotationSpeed * direction * Time.deltaTime);
            timer += Time.deltaTime;

            if (changeIndex < changeTimes.Length && timer >= changeTimes[changeIndex])
            {
                direction *= -1;  // Смена направления
                changeIndex++;
            }

            yield return null;
        }

        // Пауза перед удалением объекта спиц
        yield return new WaitForSeconds(0.5f);
    }
}
