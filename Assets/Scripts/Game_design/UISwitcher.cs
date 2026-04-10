using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISwitcher : MonoBehaviour
{
    [Header("UI Objects for Desktop")]
    public GameObject[] desktopUIObjects; // Массив объектов для Desktop

    [Header("UI Objects for Mobile")]
    public GameObject[] mobileUIObjects;  // Массив объектов для Mobile
	
	[SerializeField] private PlayerController playerController; // Ссылка на PlayerController

    void Start()
    {
        // Получаем текущую платформу
        Platform currentPlatform = PlatformDetector.GetPlatform();
		
        // if (PlatformDetector.GetPlatform() == Platform.Mobile)
        // {
            // playerController.SetMinX(-11f); // Устанавливаем значение для мобильных устройств
            // playerController.SetMaxX(10f); // Устанавливаем значение для мобильных устройств
            // Debug.Log("Мобильная платформа: изменены minX и maxX.");
        // }
        // else
        // {
            // Debug.Log("Десктопная платформа: значения minX и maxX не изменены.");
        // }

        // Включаем/выключаем UI в зависимости от платформы
        if (currentPlatform == Platform.Desktop)
        {
            EnableUIObjects(desktopUIObjects);
            DisableUIObjects(mobileUIObjects);
        }
        else if (currentPlatform == Platform.Mobile|| currentPlatform == Platform.Tablet)
        {
            EnableUIObjects(mobileUIObjects);
            DisableUIObjects(desktopUIObjects);
        }
        
        ConfigurePlatform();
    }

    private void EnableUIObjects(GameObject[] uiObjects)
    {
        foreach (GameObject obj in uiObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true); // Включаем объект
            }
        }
    }

    private void DisableUIObjects(GameObject[] uiObjects)
    {
        foreach (GameObject obj in uiObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false); // Выключаем объект
            }
        }
    }
	
	public void ConfigurePlatform()
    {
        // Получаем текущую платформу
        Platform currentPlatform = PlatformDetector.GetPlatform();
		
        if (currentPlatform == Platform.Mobile)
        {
            playerController.SetMinX(-11f); // Устанавливаем значение для мобильных устройств
            playerController.SetMaxX(10f); // Устанавливаем значение для мобильных устройств
            Debug.Log("Мобильная платформа: изменены minX и maxX.");
        }
        else
        {
            Debug.Log("Десктопная платформа: значения minX и maxX не изменены.");
        }
	}	
}

