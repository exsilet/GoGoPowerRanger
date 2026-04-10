using UnityEngine;

public class ColorObject : MonoBehaviour
{
    public string colorName; // Имя цвета объекта (Red, Blue, Yellow, Black)
    private Mechanic4_5 mechanic;

    void Start()
    {
        mechanic = FindObjectOfType<Mechanic4_5>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && mechanic != null)
        {
            mechanic.OnColorObjectTouched(colorName);
            Destroy(gameObject);
        }
    }
}
