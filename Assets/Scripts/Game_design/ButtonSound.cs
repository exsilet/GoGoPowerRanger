using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Для работы с событиями UI

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioSource soundSource;      // Ссылка на AudioSource
    public AudioClip hoverSound;         // Звук при наведении
    public AudioClip clickSound;         // Звук при клике

    // Метод для обработки события наведения мыши на кнопку
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            soundSource.PlayOneShot(hoverSound);  // Воспроизведение звука при наведении
        }
    }

    // Метод для обработки события клика по кнопке
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            soundSource.PlayOneShot(clickSound);  // Воспроизведение звука при клике
        }
    }
}
