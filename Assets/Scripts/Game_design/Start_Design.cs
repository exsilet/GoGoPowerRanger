using System.Collections;
using UnityEngine;

public class Start_Design : MonoBehaviour
{
    public float speed = 3f;
    private bool isStopped = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
        // StartCoroutine(MoveObstacle());
		Debug.Log("запустили старт-дизайн");
    }
	
		    
	void OnEnable()
    {
        // Перезапуск корутины при активации объекта
        StartCoroutine(MoveObstacle()); // Перезапускаем корутину
    }

    public IEnumerator MoveObstacle()
    {
        while (!isStopped)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;

            if (transform.position.x < -20f)
            {
                speed = 0f;
                isStopped = true;
				DisableRenderers();
            }
            yield return null; // Ждём следующий кадр
        }
    }

    public void RestartMovement()
    {
        StopAllCoroutines(); // Останавливаем любые текущие корутины
        isStopped = false;
        speed = 3f;
        transform.position = initialPosition;
        StartCoroutine(MoveObstacle()); // Перезапускаем корутину
		EnableRenderers();
    }
	
	private void DisableRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(); // Получаем все Renderer'ы объекта и его детей
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false; // Выключаем рендер
        }
    }
	
	public void EnableRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(); // Получаем все Renderer'ы объекта и его детей
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true; // Включаем рендер
        }
    }
}
