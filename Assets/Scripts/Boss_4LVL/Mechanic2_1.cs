using System.Collections;
using UnityEngine;

public class Mechanic2_1 : MonoBehaviour, IBossMechanic
{
    public GameObject bossCopyPrefab;
    public float moveSpeed = 5f;
    public float flashDuration = 0.2f;
    public float dashSpeed = 15f;
    public float dashWaitTime = 0.5f;

    [Header("Audio Settings")]
    public AudioSource cloneDashSound; // Звук движения клона

    private Vector3 centerPosition;
    private Vector3[] cornerPositions;
    private GameObject[] bossCopies;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        centerPosition = new Vector3(0, 0, 0); // Центр экрана
        cornerPositions = new Vector3[]
        {
            new Vector3(-Camera.main.orthographicSize * Camera.main.aspect + 1, Camera.main.orthographicSize - 1, 0),
            new Vector3(Camera.main.orthographicSize * Camera.main.aspect - 1, Camera.main.orthographicSize - 1, 0),
            new Vector3(-Camera.main.orthographicSize * Camera.main.aspect + 1, -Camera.main.orthographicSize + 1, 0),
            new Vector3(Camera.main.orthographicSize * Camera.main.aspect - 1, -Camera.main.orthographicSize + 1, 0)
        };
    }

    public IEnumerator Execute()
    {
        Debug.Log("Механика 2_1 запущена.");
        
        // 1. Плавное перемещение в центр экрана
        yield return StartCoroutine(MoveToCenter());
		
		yield return new WaitForSeconds(1f);

        // 2. Мигание 2 раза
        yield return StartCoroutine(Flash(2));
		
		yield return new WaitForSeconds(1f);

        // 3. Босс становится невидимым
        SetVisibility(false);

        // 4. Создание копий в 4 углах экрана
        bossCopies = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            bossCopies[i] = Instantiate(bossCopyPrefab, cornerPositions[i], Quaternion.identity);
        }

        // 5. Запуск атаки копий
        for (int i = 0; i < 4; i++)
        {
            yield return StartCoroutine(CopyAttack(bossCopies[i]));
        }
		
		yield return new WaitForSeconds(1f);

        // 7. Босс возвращается в центр экрана
        SetVisibility(true);
        transform.position = centerPosition;
		
		yield return new WaitForSeconds(1f);
        
        Debug.Log("Механика 2_1 завершена.");
    }

    private IEnumerator MoveToCenter()
    {
        while (Vector3.Distance(transform.position, centerPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator Flash(int flashes)
    {
        for (int i = 0; i < flashes; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private IEnumerator CopyAttack(GameObject copy)
    {
        SpriteRenderer copyRenderer = copy.GetComponent<SpriteRenderer>();

        // Мигание копии 2 раза
        for (int i = 0; i < 2; i++)
        {
            copyRenderer.enabled = false;
            yield return new WaitForSeconds(flashDuration);
            copyRenderer.enabled = true;
            yield return new WaitForSeconds(flashDuration);
        }

        // Воспроизведение звука при начале движения клона
        if (cloneDashSound != null)
        {
            cloneDashSound.Play();
        }

        // Быстрый полёт в сторону игрока
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        Vector3 direction = (player.position - copy.transform.position).normalized;

        while (copy != null && copyRenderer.isVisible)
        {
            copy.transform.position += direction * dashSpeed * Time.deltaTime;
            yield return null;
        }

        // Удаление копии за пределами экрана
        Destroy(copy);
        yield return new WaitForSeconds(dashWaitTime);
    }
	
	private void SetVisibility(bool isVisible)
	{
		// Скрываем/показываем основной объект
		spriteRenderer.enabled = isVisible;

		// Скрываем/показываем все дочерние объекты
		foreach (Transform child in transform)
		{
			SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
			if (childRenderer != null)
			{
				childRenderer.enabled = isVisible;
			}
		}
	}
}
