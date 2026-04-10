using UnityEngine;
using YG;

public class ItemSoul : MonoBehaviour
{
    public int soulAmount = 1; // Количество душ, которое даёт этот объект
    public AudioClip pickupSound; // Звук при взаимодействии
    private AudioSource audioSource;
    private bool isPickedUp = false; // Флаг для предотвращения повторного взаимодействия

    private void Start()
    {
        // Проверяем наличие или создаем AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true; // Устанавливаем флаг

            // Добавляем души
            SkillLevelManager skillManager = FindObjectOfType<SkillLevelManager>();
            if (skillManager != null)
            {
                skillManager.AddSouls(soulAmount);
                Debug.Log($"Игрок подобрал душу! Добавлено {soulAmount} душ.");
            }
			
			SaveManager saveManager = FindObjectOfType<SaveManager>();
			if (saveManager != null)
			{
				saveManager.SaveData(); // Сохраняем данные
				Debug.Log("Сохранение выполнено перед переходом на следующий уровень.");
			}

            // Выключаем SpriteRenderer у объекта и его дочерних объектов
            DisableSprites();

            // Воспроизводим звук
            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            // Уничтожаем объект после завершения звука
            StartCoroutine(DestroyAfterSound());
        }
    }

    private void DisableSprites()
    {
        // Отключаем SpriteRenderer у текущего объекта
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.enabled = false;
        }

        // Отключаем SpriteRenderer у всех дочерних объектов
        foreach (Transform child in transform)
        {
            SpriteRenderer childSprite = child.GetComponent<SpriteRenderer>();
            if (childSprite != null)
            {
                childSprite.enabled = false;
            }
        }
    }

    private System.Collections.IEnumerator DestroyAfterSound()
    {
        if (pickupSound != null)
        {
            yield return new WaitForSeconds(pickupSound.length);
        }
        Destroy(gameObject);
    }
}
