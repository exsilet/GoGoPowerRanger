using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private bool isPlayerInside = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Игрок вошёл в безопасную зону.");
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Игрок покинул безопасную зону.");
            isPlayerInside = false;
        }
    }

    public bool IsPlayerInside()
    {
        return isPlayerInside;
    }
}
