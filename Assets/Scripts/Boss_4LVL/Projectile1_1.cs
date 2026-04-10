using UnityEngine;

public class Projectile1_1 : MonoBehaviour
{
    public float speed = 4f;
    public float splitDistance = 3f;
    public GameObject projectilePrefab;
    private Vector3 targetDirection;
    private bool hasSplit = false;
	public AudioSource explosionSound; // Звук взрыва

    void Start()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player != null)
        {
            targetDirection = (player.position - transform.position).normalized;
        }
    }

    void Update()
    {
        transform.position += targetDirection * speed * Time.deltaTime;

        if (!hasSplit && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= splitDistance)
        {
            Split();
        }
    }

    private void Split()
	{
		hasSplit = true;
		
		// Воспроизведение звука
        if (explosionSound != null)
        {
            explosionSound.Play();
        }

		for (int i = 0; i < 12; i++)
		{
			// Расчёт угла для текущего снаряда
			float angle = i * 30f; // Угол (360° / 12 = 30° для каждого объекта)
			Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0).normalized;

			// Создание нового снаряда
			GameObject newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

			// Поворот снаряда, чтобы он смотрел в направлении движения
			newProjectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

			// Придание скорости снаряду
			Rigidbody2D rb = newProjectile.GetComponent<Rigidbody2D>();
			if (rb != null)
			{
				rb.velocity = direction * speed;
			}
		}
		HideObjectAndChildren(gameObject);

		// Уничтожаем объект после длительности звука
		Destroy(gameObject, explosionSound.clip.length);
	}

    void OnBecameInvisible()
    {
        //Destroy(gameObject);
    }
	
	private void HideObjectAndChildren(GameObject obj)
	{
		SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
		if (sr != null)
		{
			sr.enabled = false;
		}

		foreach (Transform child in obj.transform)
		{
			HideObjectAndChildren(child.gameObject);
		}
	}
}
