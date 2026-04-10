using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float detectionRadius = 5f;  // Радиус, в котором снаряд взрывается при приближении к игроку
    public float explosionForce = 15f;  // Сила отталкивания от взрыва
    public float disableDuration = 1f;  // Время отключения управления игроком после взрыва

    private void Update()
    {
        DetectPlayerAndExplode();
    }

    private void DetectPlayerAndExplode()
    {
        // Ищем игрока в радиусе детекции
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRadius);

        // Проверяем, является ли объект игроком по тегу "Player"
        if (player != null && player.CompareTag("Player"))
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerRb != null && playerController != null)
            {
                playerRb.WakeUp(); // Пробуждаем игрока
                Explode(playerRb, playerController); // Отталкиваем игрока и временно отключаем управление
            }
        }
    }

    private void Explode(Rigidbody2D playerRb, PlayerController playerController)
    {
        // Применяем отталкивающую силу к игроку
        Vector2 repulsionDirection = (playerRb.transform.position - transform.position).normalized;
        playerRb.AddForce(repulsionDirection * explosionForce, ForceMode2D.Impulse);

        // Отключаем управление игроком
        playerController.DisableControlForDuration(disableDuration);

        // Уничтожаем снаряд
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса детекции
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
