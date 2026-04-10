using UnityEngine;

public class Projectile : MonoBehaviour
{
    void Start()
    {
        // Получаем все коллайдеры с тегом "Boundary"
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("Boundary");
        Collider2D projectileCollider = GetComponent<Collider2D>();

        foreach (GameObject boundary in boundaries)
        {
            Collider2D boundaryCollider = boundary.GetComponent<Collider2D>();
            if (boundaryCollider != null && projectileCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, boundaryCollider);
            }
        }
    }

    void Update()
    {
        // Проверяем, находится ли объект за пределами видимого экрана
        if (!IsVisible())
        {
            Destroy(gameObject);
        }
    }

    // Метод для проверки видимости объекта
    bool IsVisible()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
}
