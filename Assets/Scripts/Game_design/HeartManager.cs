using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Для работы с компонентами Image

public class HeartManager : MonoBehaviour
{
    public PlayerHealth playerHealth;  // Ссылка на PlayerHealth

    public Image[] hearts;  // Массив для активных сердечек (Image)
    public Image[] heartsInactive;  // Массив для неактивных сердечек (Image)
    public ParticleSystem[] heartParticles;  // Массив партиклей для каждого сердечка
    
    private bool isPlayerActive = false;
    private int previousHealth;  // Переменная для хранения предыдущего значения здоровья
	
	[Header("Heart Objects for Max HP = 4")]
    public GameObject heart4Object1;
    public GameObject heart4Object2;

    [Header("Heart Objects for Max HP = 5")]
    public GameObject heart5Object1;
    public GameObject heart5Object2;

    void Start()
    {
        // Убедимся, что у нас есть ссылка на PlayerHealth
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
		
		CheckAndActivateHearts(playerHealth.maxHealth);
        
        // Проверяем активность игрока
        if (playerHealth.gameObject.activeInHierarchy)
        {
            isPlayerActive = true;
            previousHealth = playerHealth.currentHealth;  // Сохраняем начальное значение здоровья
            UpdateHeartDisplay();  // Обновляем отображение сердечек
        }
        else
        {
            Debug.Log("Игрок не активен на старте");
        }
    }
    
    void Update()
    {
        // Проверяем, активен ли игрок, и если активен, обновляем отображение здоровья
        if (playerHealth.gameObject.activeInHierarchy && !isPlayerActive)
        {
            isPlayerActive = true;
            previousHealth = playerHealth.currentHealth;  // Сохраняем начальное значение здоровья
            UpdateHeartDisplay();  // Обновляем отображение сердечек
        }
		
		CheckAndActivateHearts(playerHealth.maxHealth);
		
    }
	
	public void CheckAndActivateHearts(int maxHealth)
    {
        if (maxHealth >= 4)
        {
            if (heart4Object1 != null) heart4Object1.SetActive(true);
            if (heart4Object2 != null) heart4Object2.SetActive(true);
        }

        if (maxHealth == 5)
        {
            if (heart5Object1 != null) heart5Object1.SetActive(true);
            if (heart5Object2 != null) heart5Object2.SetActive(true);
        }
    }

    // Метод для обновления отображения сердечек и партиклей
    public void UpdateHeartDisplay()
    {
        if (playerHealth == null) return;  // Если ссылки нет, выходим

        int currentHealth = playerHealth.currentHealth;  // Получаем текущее здоровье
        int maxHealth = playerHealth.maxHealth;  // Получаем максимальное здоровье

        // Проходим по всем сердечкам
        for (int i = 0; i < maxHealth; i++)
        {
            // Если индекс меньше текущего здоровья, активируем сердце
            if (i < currentHealth)
            {
                hearts[i].enabled = true;  // Включаем активное сердце
                heartsInactive[i].enabled = false;  // Выключаем неактивное сердце
                heartParticles[i].Stop();  // Останавливаем партикль, если здоровье восстанавливается
            }
            else
            {
                hearts[i].enabled = false;  // Выключаем активное сердце
                heartsInactive[i].enabled = true;  // Включаем неактивное сердце

                // Запускаем партикль только для того сердца, которое деактивировалось
                if (previousHealth > currentHealth && i == currentHealth && !heartParticles[i].isPlaying)
                {
                    heartParticles[i].Play();  // Запускаем партикль для только что деактивированного сердца
                    Debug.Log("Активируем партикль на индексе " + i);
                }
            }
        }

        // Обновляем предыдущее здоровье
        previousHealth = currentHealth;
    }

    // Метод для добавления одного сердечка при получении здоровья
    public void AddHeart()
    {
        UpdateHeartDisplay();
    }
	
	public void ResetHearts()
	{
		// Получаем текущее здоровье игрока
		int currentHealth = playerHealth.currentHealth;

		// Восстанавливаем все сердца в активное состояние
		for (int i = 0; i < hearts.Length; i++)
		{
			hearts[i].enabled = true;  // Включаем активное сердце
			heartsInactive[i].enabled = false;  // Выключаем неактивное сердце
		}

		// Останавливаем все партикли
		for (int i = 0; i < heartParticles.Length; i++)
		{
			heartParticles[i].Stop();  // Останавливаем партикль
		}

		// Обновляем отображение с текущим здоровьем
		UpdateHeartDisplay();  // Обновим отображение сердечек
	}
}
