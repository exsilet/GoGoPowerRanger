using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthAmount = 1;  // Сколько добавляется в копилку
    public AudioSource pickupSound;  // Звук при поднятии здоровья
	private bool isPickedUp = false;

    void OnTriggerEnter2D(Collider2D other)
    {
		if (isPickedUp) return; // Если объект уже поднят, ничего не делаем
		
        if (other.CompareTag("Player"))
        {
			isPickedUp = true; // Устанавливаем флаг сразу после первого столкновения
            // Получаем ссылку на PlayerAbilities
            PlayerAbilities playerAbilities = other.GetComponent<PlayerAbilities>();
            if (playerAbilities != null)
            {
                // Увеличиваем счётчик навыка
                playerAbilities.AddHealthSkill(healthAmount);
            }

            // Запускаем корутину для скрытия и удаления объекта
            StartCoroutine(HideAndDestroy());
        }
    }

    IEnumerator HideAndDestroy()
    {
        SetVisibility(false);

        if (pickupSound != null)
        {
            pickupSound.Play();
            yield return new WaitForSeconds(pickupSound.clip.length);
        }

        Destroy(gameObject);
    }

    private void SetVisibility(bool isVisible)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = isVisible;
        }

        foreach (Transform child in transform)
        {
            SpriteRenderer childSr = child.GetComponent<SpriteRenderer>();
            if (childSr != null)
            {
                childSr.enabled = isVisible;
            }
        }
    }
}
