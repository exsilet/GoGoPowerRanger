using UnityEngine;

public class BouncingProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int maxBounces = 3;       // Количество отскоков
    private int currentBounces = 0;  // Счетчик отскоков
    private Vector2 direction;
    private Camera mainCamera;

    [Header("Effects")]
    public AudioSource bounceSound;       // Звук отскока
    public ParticleSystem bounceParticles; // Частицы отскока

    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Метод для инициализации снаряда с заданным направлением
    public void Initialize(Vector2 initialDirection)
    {
        direction = initialDirection.normalized; // Устанавливаем начальное направление
    }

    private void Update()
    {
        // Двигаем снаряд в текущем направлении
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Определение границ экрана
        Vector2 min = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 max = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));

        // Проверка отскоков по оси X
        if (transform.position.x < min.x || transform.position.x > max.x)
        {
            direction.x = -direction.x; // Отскок по горизонтали
            HandleBounce(); // Обрабатываем отскок
        }

        // Проверка отскоков по оси Y
        if (transform.position.y < min.y || transform.position.y > max.y)
        {
            direction.y = -direction.y; // Отскок по вертикали
            HandleBounce(); // Обрабатываем отскок
        }

        // Уничтожение снаряда, если он превысил количество отскоков
        if (currentBounces >= maxBounces)
        {
            Destroy(gameObject);
        }
    }

    private void HandleBounce()
    {
        currentBounces++;

        // Воспроизведение звука
        if (bounceSound != null)
        {
            bounceSound.Play();
        }

        // Воспроизведение частиц
        if (bounceParticles != null)
        {
            bounceParticles.transform.position = transform.position; // Устанавливаем позицию частиц
            bounceParticles.Play();
        }
    }
}
